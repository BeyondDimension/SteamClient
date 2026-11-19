#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
namespace BD.SteamClient.Models;

static class SteamAppEditProperties
{
    const string EditNode = "steam_edit";
    const string Originals = "wtts_original";
    const string LegacyName = "base_name";
    const string LegacyLocalizedName = "base_name_localized";

    // Each stored slot wraps the old property. An empty slot means the key was absent.
    // Preserve its type and the entire original launch table, including unknown Steam keys.
    public static bool Apply(SteamAppPropertyTable properties, SteamAppEdit edit)
    {
        var app = Table(properties, "appinfo");
        if (app == null)
            return false;
        var marker = Table(app, EditNode);
        var originals = Table(marker, Originals);
        var restored = originals != null || marker?.HasProperty(LegacyName) == true;
        if (!restored && edit.IsEmpty)
            return false;

        var common = Table(app, "common");
        var extended = Table(app, "extended");
        var config = Table(app, "config");
        if (originals != null)
        {
            Restore(common, "name", originals);
            Restore(common, "name_localized", originals);
            Restore(common, "sortas", originals);
            Restore(extended, "developer", originals);
            Restore(extended, "publisher", originals);
            Restore(common, "associations", originals);
            Restore(config, "launch", originals);
        }
        else if (marker?.GetPropertyValue<string>(LegacyName) is { } legacy)
        {
            common?.SetPropertyValue("name", SteamAppPropertyType.String, legacy);
            if (Table(marker, LegacyLocalizedName) is { } localized)
                common?.SetPropertyValue("name_localized", SteamAppPropertyType.Table, localized);
        }
        marker?.RemoveProperty(Originals);
        marker?.RemoveProperty(LegacyName);
        marker?.RemoveProperty(LegacyLocalizedName);
        if (edit.IsEmpty)
        {
            if (marker?.Count == 0)
                app.RemoveProperty(EditNode);
            return restored;
        }

        originals = new();
        common ??= EnsureTable(app, "common");
        if (edit.Name != null)
        {
            Remember(common, "name_localized", originals);
            common.RemoveProperty("name_localized");
            SetText(common, "name", edit.Name, originals);
        }
        if (edit.SortAs != null)
            SetText(common, "sortas", edit.SortAs, originals);
        if (edit.Developer != null || edit.Publisher != null)
        {
            extended ??= EnsureTable(app, "extended");
            if (edit.Developer != null)
                SetText(extended, "developer", edit.Developer, originals);
            if (edit.Publisher != null)
                SetText(extended, "publisher", edit.Publisher, originals);
            RewriteAssociations(common, originals, edit);
        }
        if (edit.LaunchItems != null)
        {
            config ??= EnsureTable(app, "config");
            Remember(config, "launch", originals);
            var launch = new SteamAppPropertyTable();
            for (var i = 0; i < edit.LaunchItems.Length; i++)
            {
                var item = edit.LaunchItems[i];
                var fields = new SteamAppPropertyTable();
                fields.SetPropertyValue("executable", SteamAppPropertyType.String, item.Executable ?? string.Empty);
                if (item.Label is { Length: > 0 }) fields.SetPropertyValue("description", SteamAppPropertyType.String, item.Label);
                if (item.Arguments is { Length: > 0 }) fields.SetPropertyValue("arguments", SteamAppPropertyType.String, item.Arguments);
                if (item.WorkingDir is { Length: > 0 }) fields.SetPropertyValue("workingdir", SteamAppPropertyType.String, item.WorkingDir);
                if (item.Platform is { Length: > 0 }) EnsureTable(fields, "config").SetPropertyValue("oslist", SteamAppPropertyType.String, item.Platform);
                launch.AddPropertyValue(i.ToString(System.Globalization.CultureInfo.InvariantCulture), SteamAppPropertyType.Table, fields);
            }
            config.SetPropertyValue("launch", SteamAppPropertyType.Table, launch);
        }
        (marker ?? EnsureTable(app, EditNode)).SetPropertyValue(Originals, SteamAppPropertyType.Table, originals);
        return true;
    }

    static void RewriteAssociations(SteamAppPropertyTable common, SteamAppPropertyTable originals, SteamAppEdit edit)
    {
        var current = Table(common, "associations");
        if (current == null)
            return;
        Remember(common, "associations", originals);
        var associations = new SteamAppPropertyTable();
        bool developer = false, publisher = false;
        foreach (var property in current.Properties)
        {
            var value = property.GetValue<SteamAppPropertyTable>();
            string? replacement = null;
            switch (value?.GetPropertyValue<string>("type"))
            {
                case "developer" when edit.Developer != null:
                    if (!developer) replacement = edit.Developer;
                    developer = true;
                    break;
                case "publisher" when edit.Publisher != null:
                    if (!publisher) replacement = edit.Publisher;
                    publisher = true;
                    break;
                default:
                    associations.AddPropertyValue(property.Name, property.PropertyType, property.Value!);
                    continue;
            }
            if (!string.IsNullOrEmpty(replacement))
            {
                var renamed = new SteamAppPropertyTable(value!);
                renamed.SetPropertyValue("name", SteamAppPropertyType.String, replacement);
                associations.AddPropertyValue(property.Name, property.PropertyType, renamed);
            }
        }
        if (!developer && edit.Developer is { Length: > 0 }) AddAssociation("developer", edit.Developer);
        if (!publisher && edit.Publisher is { Length: > 0 }) AddAssociation("publisher", edit.Publisher);
        common.SetPropertyValue("associations", SteamAppPropertyType.Table, associations);

        void AddAssociation(string type, string name)
        {
            var index = associations.Count;
            var key = index.ToString(System.Globalization.CultureInfo.InvariantCulture);
            while (associations.GetPropertyType(key) != SteamAppPropertyType._Invalid_)
                key = (++index).ToString(System.Globalization.CultureInfo.InvariantCulture);
            var value = new SteamAppPropertyTable();
            value.SetPropertyValue("type", SteamAppPropertyType.String, type);
            value.SetPropertyValue("name", SteamAppPropertyType.String, name);
            associations.AddPropertyValue(key, SteamAppPropertyType.Table, value);
        }
    }

    public static SteamAppEdit? ReadOriginals(SteamAppPropertyTable? app)
    {
        var marker = Table(app, EditNode);
        var originals = Table(marker, Originals);
        if (originals == null)
            return marker?.GetPropertyValue<string>(LegacyName) is { Length: > 0 } name ? new() { Name = name } : null;
        var nameSlot = ReadText("name");
        var launchSlot = Table(originals, "launch");
        return new SteamAppEdit
        {
            Name = nameSlot,
            SortAs = ReadText("sortas") is { } sortAs ? (sortAs.Length == 0 ? nameSlot ?? Table(app, "common")?.GetPropertyValue<string>("name") ?? string.Empty : sortAs) : null,
            Developer = ReadText("developer"),
            Publisher = ReadText("publisher"),
            LaunchItems = launchSlot == null ? null : ReadLaunch(Table(launchSlot, "launch")),
        };

        string? ReadText(string key) => Table(originals, key)?.GetPropertyValue(key, string.Empty);
    }

    public static SteamAppEdit ReadLegacyEdit(SteamAppPropertyTable app)
    {
        var common = Table(app, "common");
        var extended = Table(app, "extended");
        var launch = Table(Table(app, "config"), "launch");
        var name = common?.GetPropertyValue<string>("name");
        var baseName = Table(app, EditNode)?.GetPropertyValue<string>(LegacyName);
        return new SteamAppEdit
        {
            Name = name == baseName ? null : name,
            SortAs = common?.GetPropertyValue<string>("sortas"),
            Developer = extended?.GetPropertyValue<string>("developer"),
            Publisher = extended?.GetPropertyValue<string>("publisher"),
            LaunchItems = launch == null ? null : ReadLaunch(launch),
        };
    }

    static SteamLaunchEdit[] ReadLaunch(SteamAppPropertyTable? launch) => launch == null ? [] : launch.Properties
        .Where(static p => p.PropertyType == SteamAppPropertyType.Table)
        .Select(static p => p.GetValue<SteamAppPropertyTable>()!)
        .Select(static p => new SteamLaunchEdit(p.GetPropertyValue<string>("description"), p.GetPropertyValue<string>("executable"),
            p.GetPropertyValue<string>("arguments"), p.GetPropertyValue<string>("workingdir"), Table(p, "config")?.GetPropertyValue<string>("oslist")))
        .ToArray();

    static SteamAppPropertyTable? Table(SteamAppPropertyTable? table, string key) => table?.GetPropertyValue<SteamAppPropertyTable>(key);

    static SteamAppPropertyTable EnsureTable(SteamAppPropertyTable table, string key)
    {
        if (Table(table, key) is { } existing)
            return existing;
        var child = new SteamAppPropertyTable();
        table.SetPropertyValue(key, SteamAppPropertyType.Table, child);
        return child;
    }

    static void SetText(SteamAppPropertyTable table, string key, string value, SteamAppPropertyTable originals)
    {
        Remember(table, key, originals);
        table.SetPropertyValue(key, SteamAppPropertyType.String, value);
    }

    static void Remember(SteamAppPropertyTable from, string key, SteamAppPropertyTable originals)
    {
        var slot = new SteamAppPropertyTable();
        if (from[key] is { Value: { } value } property)
            slot.AddPropertyValue(key, property.PropertyType, value);
        originals.AddPropertyValue(key, SteamAppPropertyType.Table, slot);
    }

    static void Restore(SteamAppPropertyTable? to, string key, SteamAppPropertyTable originals)
    {
        if (to == null || Table(originals, key) is not { } slot)
            return;
        if (slot[key] is { Value: { } value } property)
            to.SetPropertyValue(key, property.PropertyType, value);
        else
            to.RemoveProperty(key);
    }
}
#endif
