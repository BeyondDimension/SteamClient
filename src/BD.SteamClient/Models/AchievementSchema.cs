namespace BD.SteamClient.Models;

public sealed record AchievementSet(int Id, string Name, uint DlcAppId, int Order);

public static class AchievementSchema
{
    // Distinguish an ambiguous key from a missing key. Only a missing restriction may default to zero.
    static readonly KeyValue Ambiguous = new();

    /// <summary>
    /// Reads achievement metadata from an app's schema node. Base rows come first, then sets by
    /// ascending order and ID, retaining schema order within each set.
    /// </summary>
    public static List<AchievementInfo> ReadAchievements(KeyValue game, int appId, string language, string gameLanguage)
    {
        if (GetChildren(game) is null || GetChildren(FindChild(game, "stats")) is not { } stats)
            return new List<AchievementInfo>();

        var groups = ReadGroups(FindChild(game, "groups"), language, gameLanguage);
        var rows = new List<(AchievementInfo Info, long Order, int Index)>();
        var names = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var stat in stats)
        {
            if (GetStatType(stat) is not (UserStatType.Achievements or UserStatType.GroupAchievements))
                continue;

            foreach (var bits in stat.Children!)
            {
                if (!string.Equals(bits.Name, "bits", StringComparison.OrdinalIgnoreCase) || GetChildren(bits) is not { } definitions)
                    continue;

                foreach (var bit in definitions)
                {
                    if (GetChildren(bit) is null || ReadString(FindChild(bit, "name")) is not { } id)
                        continue;

                    // Don't select one of two definitions for the same API name, even if its restrictions look valid.
                    if (!names.TryAdd(id, true))
                    {
                        names[id] = false;
                        continue;
                    }

                    if (!TryReadOptionalInt(FindChild(bit, "groupid"), out var groupId) || groupId < 0 ||
                        !TryReadOptionalInt(FindChild(bit, "permission"), out var permission) || permission < 0 ||
                        !TryReadOptionalInt(FindChild(bit, "archived"), out var archived))
                        continue;

                    (AchievementSet? Set, int? Order) group = default;
                    if (groupId != 0 && (!groups.TryGetValue(groupId, out group) || group.Set is null))
                        continue;

                    var display = FindChild(bit, "display");
                    rows.Add((new AchievementInfo
                    {
                        AppId = appId,
                        Id = id,
                        Name = Localize(FindChild(display, "name"), language, gameLanguage, id),
                        Description = Localize(FindChild(display, "desc"), language, gameLanguage, string.Empty),
                        IconNormal = ReadString(FindChild(display, "icon")) ?? string.Empty,
                        IconLocked = ReadString(FindChild(display, "icon_gray")) ?? string.Empty,
                        IsHidden = TryReadInt(FindChild(display, "hidden"), out var hidden) && hidden != 0,
                        Permission = permission,
                        Set = group.Set,
                        IsArchived = archived != 0,
                    }, group.Set is null ? long.MinValue : (long?)group.Order ?? long.MaxValue, rows.Count));
                }
            }
        }

        if (groups.Count != 0)
        {
            rows.Sort(static (left, right) =>
            {
                var order = left.Order.CompareTo(right.Order);
                if (order != 0)
                    return order;
                var group = (left.Info.Set?.Id ?? 0).CompareTo(right.Info.Set?.Id ?? 0);
                return group != 0 ? group : left.Index.CompareTo(right.Index);
            });
        }

        var result = new List<AchievementInfo>(rows.Count);
        foreach (var row in rows)
        {
            if (names[row.Info.Id!])
                result.Add(row.Info);
        }

        return result;
    }

    public static string GetLocalizedString(KeyValue node, string language, string gameLanguage, string fallback)
        => Localize(node, language, gameLanguage, fallback);

    public static UserStatType GetStatType(KeyValue stat)
    {
        if (GetChildren(stat) is null)
            return UserStatType.Invalid;

        var typeNode = FindChild(stat, "type");
        var typeIntNode = FindChild(stat, "type_int");
        var type = ReadStatType(typeNode, allowName: true);
        if (typeIntNode is null)
            return type;

        var legacyType = ReadStatType(typeIntNode, allowName: false);
        return typeNode is null || type == legacyType ? legacyType : UserStatType.Invalid;
    }

    static Dictionary<int, (AchievementSet? Set, int? Order)> ReadGroups(KeyValue? node, string language, string gameLanguage)
    {
        var groups = new Dictionary<int, (AchievementSet?, int?)>();
        if (GetChildren(node) is not { } definitions)
            return groups;

        foreach (var group in definitions)
        {
            if (!int.TryParse(group.Name, NumberStyles.None, CultureInfo.InvariantCulture, out var id) || id == 0)
                continue;

            if (!groups.TryAdd(id, default))
            {
                groups[id] = default;
                continue;
            }

            if (GetChildren(group) is null || !TryReadDlcAppId(FindChild(group, "dlcappid"), out var dlcAppId))
                continue;

            var orderNode = FindChild(group, "order");
            if (!TryReadOptionalInt(orderNode, out var order))
                continue;

            var name = Localize(FindChild(group, "name"), language, gameLanguage, group.Name);
            groups[id] = (new AchievementSet(id, name, dlcAppId, orderNode is null ? int.MaxValue : order),
                orderNode is null ? null : order);
        }

        return groups;
    }

    static string Localize(KeyValue? node, string language, string gameLanguage, string fallback)
        => ReadString(FindChild(node, language))
            ?? ReadString(FindChild(node, gameLanguage))
            ?? ReadString(FindChild(node, "english"))
            ?? ReadString(node)
            ?? fallback;

    static UserStatType ReadStatType(KeyValue? node, bool allowName)
    {
        if (TryReadInt(node, out var value) && value is >= (int)UserStatType.Integer and <= (int)UserStatType.GroupAchievements)
            return (UserStatType)value;

        if (allowName && ReadString(node) is { } name)
        {
            var trimmed = name.AsSpan().Trim();
            if (trimmed.Equals("INT", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals(nameof(UserStatType.Integer), StringComparison.OrdinalIgnoreCase))
                return UserStatType.Integer;
            if (trimmed.Equals(nameof(UserStatType.Float), StringComparison.OrdinalIgnoreCase))
                return UserStatType.Float;
            if (trimmed.Equals("AVGRATE", StringComparison.OrdinalIgnoreCase) ||
                trimmed.Equals(nameof(UserStatType.AverageRate), StringComparison.OrdinalIgnoreCase))
                return UserStatType.AverageRate;
            if (trimmed.Equals(nameof(UserStatType.Achievements), StringComparison.OrdinalIgnoreCase))
                return UserStatType.Achievements;
            if (trimmed.Equals(nameof(UserStatType.GroupAchievements), StringComparison.OrdinalIgnoreCase))
                return UserStatType.GroupAchievements;
        }

        return UserStatType.Invalid;
    }

    static bool TryReadOptionalInt(KeyValue? node, out int value) => TryReadInt(node, out value) || node is null;

    static bool TryReadInt(KeyValue? node, out int value)
    {
        if (node is { Valid: true, Type: KeyValueType.Int32, Children: null, Value: int number })
        {
            value = number;
            return true;
        }

        return int.TryParse(ReadScalarString(node), NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    static bool TryReadDlcAppId(KeyValue? node, out uint appId)
    {
        if (node is null)
        {
            appId = 0;
            return true;
        }

        if (node is { Valid: true, Type: KeyValueType.Int32, Children: null, Value: int number } && number >= 0)
        {
            appId = (uint)number;
            return true;
        }

        return uint.TryParse(ReadScalarString(node), NumberStyles.None, CultureInfo.InvariantCulture, out appId);
    }

    static string? ReadScalarString(KeyValue? node) => node is { Children: null } ? ReadString(node) : null;

    static string? ReadString(KeyValue? node)
        => node is { Valid: true, Type: KeyValueType.String or KeyValueType.WideString, Value: string value } &&
            !string.IsNullOrWhiteSpace(value) ? value : null;

    static List<KeyValue>? GetChildren(KeyValue? node)
        => node is { Valid: true, Type: KeyValueType.None, Value: null } ? node.Children : null;

    static KeyValue? FindChild(KeyValue? node, string name)
    {
        if (node?.Children is not { } children || !node.Valid)
            return null;

        KeyValue? match = null;
        foreach (var child in children)
        {
            if (!string.Equals(child.Name, name, StringComparison.OrdinalIgnoreCase))
                continue;
            if (match is not null)
                return Ambiguous;
            match = child;
        }

        return match;
    }
}
