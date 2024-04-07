namespace BD.SteamClient8.Models;

/// <summary>
/// 提供谷歌身份验证
/// </summary>
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
[MessagePackObject(keyAsPropertyName: true)]
public partial class GoogleAuthenticator : AuthenticatorValueModel
{
    /// <summary>
    /// 代码中的位数
    /// </summary>
    const int CODE_DIGITS = 6;

    /// <summary>
    /// 创建一个新的 Authenticator 对象
    /// </summary>
    [SerializationConstructor]
    public GoogleAuthenticator() : base(CODE_DIGITS)
    {
    }

    /// <inheritdoc/>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public override AuthenticatorPlatform Platform => AuthenticatorPlatform.Google;

    /// <summary>
    /// 如果网络错误，忽略同步的分钟数
    /// </summary>
    const int SYNC_ERROR_MINUTES = 5;

    /// <summary>
    /// 上次同步时间错误
    /// </summary>
    static DateTime _lastSyncError = DateTime.MinValue;

    /// <summary>
    /// 获取序列号
    /// </summary>
    [IgnoreDataMember]
    [MPIgnore]
    [NewtonsoftJsonIgnore]
    [SystemTextJsonIgnore]
    public string Serial
    {
        get
        {
            var result = Base32.GetInstance().Encode(SecretKey.ThrowIsNull(nameof(SecretKey)));
            return result;
        }
    }

    /// <summary>
    /// 向服务器注册身份验证器
    /// </summary>
    /// <param name="b32key"></param>
    public void Enroll(string b32key)
    {
        SecretKey = Base32.GetInstance().Decode(b32key);
        Sync();
    }

    /// <summary>
    /// 将验证者的时间与 Google 同步，我们用与 UTC 时间的差异来更新数据记录
    /// </summary>
    public override async void Sync()
    {
        // check if data is protected
        if (SecretKey == null && EncryptedData != null)
        {
            throw new AuthenticatorEncryptedSecretDataException();
        }

        // don't retry for 5 minutes
        if (_lastSyncError >= DateTime.Now.AddMinutes(0 - SYNC_ERROR_MINUTES))
        {
            return;
        }

        try
        {
            // we use the Header response field from a request to www.google.come
            var response = await IAuthenticatorNetService.Instance.TimeSync();

            // OK?
            if (response.Content.statusCode != HttpStatusCode.OK)
            {
                throw new ApplicationException(string.Format("{0}: {1}", response.Content, response.GetMessage()));
            }

            var headerdate = response.Content.date;
            if (string.IsNullOrEmpty(headerdate) == false)
            {
                if (DateTime.TryParse(headerdate, out var dt) == true)
                {
                    // get as ms since epoch
                    long dtms = Convert.ToInt64((dt.ToUniversalTime() - new DateTime(1970, 1, 1)).TotalMilliseconds);

                    // get the difference between the server time and our current time
                    long serverTimeDiff = dtms - CurrentTime;

                    // update the Data object
                    ServerTimeDiff = serverTimeDiff;
                    LastServerTime = DateTime.Now.Ticks;
                }
            }

            // clear any sync error
            _lastSyncError = DateTime.MinValue;
        }
        catch
        {
            // don't retry for a while after error
            _lastSyncError = DateTime.Now;

            // set to zero to force reset
            ServerTimeDiff = 0;
        }
    }
}