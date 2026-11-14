using Google.Protobuf;

namespace BD.SteamClient.Services;

public sealed partial class SteamAccountService
{
    async Task<TResponse> SendQrLoginRequestAsync<TResponse>(string method, IMessage input,
        MessageParser<TResponse> parser, CancellationToken cancellationToken)
        where TResponse : IMessage<TResponse>
    {
        cancellationToken.ThrowIfCancellationRequested();
        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://api.steampowered.com/IAuthenticationService/{method}/v1/")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["input_protobuf_encoded"] = input.ToByteString().ToBase64(),
            }),
        };
        request.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException(BD.SteamClient.Resources.SteamStrings.Format("QrHttpError", (int)response.StatusCode), null, response.StatusCode);
        if (!response.Headers.TryGetValues("X-eresult", out var results)
            || !int.TryParse(results.SingleOrDefault(), NumberStyles.None, CultureInfo.InvariantCulture, out var result))
            throw new InvalidOperationException(BD.SteamClient.Resources.SteamStrings.Get("QrInvalidResult"));
        if (result != 1)
        {
            var messageKey = result switch
            {
                15 => "QrRejected",
                27 => "QrExpired",
                84 => "RequestTooFrequent",
                _ => "QrRequestFailed",
            };
            throw new InvalidOperationException(BD.SteamClient.Resources.SteamStrings.Format("QrResultError", BD.SteamClient.Resources.SteamStrings.Get(messageKey), result));
        }
        return parser.ParseFrom(await response.Content.ReadAsByteArrayAsync(cancellationToken));
    }

    async Task CompleteQrWebLoginAsync(SteamLoginState loginState,
        CAuthentication_PollAuthSessionStatus_Response poll, CancellationToken cancellationToken)
    {
        var store = new Uri(STEAM_STORE_URL);
        loginState.SeesionId = cookieContainer.GetCookieValue(store, "sessionid");
        if (string.IsNullOrEmpty(loginState.SeesionId))
        {
            using var response = await client.GetAsync("https://store.steampowered.com/login/", cancellationToken);
            response.EnsureSuccessStatusCode();
            loginState.SeesionId = cookieContainer.GetCookieValue(store, "sessionid");
        }
        var finalized = await FinalizeLoginAsync(poll.RefreshToken, loginState.SeesionId, cancellationToken);
        if (!ulong.TryParse(finalized.SteamId, NumberStyles.None, CultureInfo.InvariantCulture, out var steamId) || steamId == 0)
            throw new InvalidOperationException(BD.SteamClient.Resources.SteamStrings.Get("WebInvalidSteamId"));

        if (finalized.TransferInfo is not null)
        {
            foreach (var transfer in finalized.TransferInfo)
            {
                var uri = new Uri(transfer.Url ?? throw new InvalidOperationException(BD.SteamClient.Resources.SteamStrings.Get("WebMissingTransferUrl")));
                if (uri.Host is "help.steampowered.com" or "steam.tv")
                    continue;
                using var request = new HttpRequestMessage(HttpMethod.Post, uri)
                {
                    Content = new FormUrlEncodedContent(new Dictionary<string, string?>
                    {
                        ["nonce"] = transfer.Params?.Nonce,
                        ["auth"] = transfer.Params?.Auth,
                        ["steamID"] = finalized.SteamId,
                    }),
                };
                request.Headers.UserAgent.ParseAdd(uas.GetUserAgent());
                using var response = await client.SendAsync(request, cancellationToken);
                response.EnsureSuccessStatusCode();
            }
        }
        cancellationToken.ThrowIfCancellationRequested();
        var steamSession = new SteamSession
        {
            SteamId = finalized.SteamId!,
            AccessToken = poll.AccessToken,
            RefreshToken = poll.RefreshToken,
            CookieContainer = cookieContainer,
        };
        sessions.AddOrSetSeesion(steamSession);
        loginState.SteamId = steamId;
        loginState.SteamIdString = finalized.SteamId;
        loginState.Username = poll.AccountName;
        loginState.AccessToken = poll.AccessToken;
        loginState.RefreshToken = poll.RefreshToken;
        loginState.Cookies = cookieContainer.GetAllCookies();
        loginState.Message = null;
        loginState.Success = true;
    }
}
