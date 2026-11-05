using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace StorageUpgrade;

public static class Configs
{
    private static ConfigEntry<Toggle> refund;
    private static ConfigEntry<Toggle> addColumns;
    private static ConfigEntry<string> upgradeMap;
    private static ConfigEntry<Vector2> columnButtonPos;
    private static ConfigEntry<Vector2> rowButtonPos;
    private static ConfigEntry<JoyButton> addColumnTrigger;
    private static ConfigEntry<JoyButton> addRowTrigger;
    private static ConfigEntry<CostType> costType;
    private static ConfigEntry<UpgradeType> upgradeType;

    public static bool Refund => refund.Value is Toggle.On;
    public static bool AddColumns => addColumns.Value is Toggle.On;

    public static UpgradeMap UpgradeMap => new UpgradeMap(upgradeMap.Value);
    
    public static Vector2 ColumnPos => columnButtonPos.Value;
    public static Vector2 RowPos => rowButtonPos.Value;

    public static string AddColumnTrigger => addColumnTrigger.Value.GetAttributeOfType<InternalName>().name;
    public static string AddRowTrigger => addRowTrigger.Value.GetAttributeOfType<InternalName>().name;
    
    public static CostType CostType => costType.Value;
    public static UpgradeType UpgradeType => upgradeType.Value;

    public static void Setup()
    {
        refund = ConfigManager.config("Settings", "Refund", Toggle.On,
            "If on, destroying container piece will refund upgrade costs");
        addColumns = ConfigManager.config("Settings", "AddColumns", Toggle.Off,
            "If on, players can upgrade columns ( to a max of 7 ); Warning: removing plugin, will cause added columns items to break UI");
        upgradeMap = ConfigManager.config("Settings", "Custom Upgrade Map", "",
            "Define upgrade costs, [prefabId]:[itemId]*[amount],...;...");

        columnButtonPos =
            ConfigManager.config("UI", "Column Button", new Vector2(370f, 105f), "Set column button position", false);
        columnButtonPos.SettingChanged += (sender, args) =>
        {
            if (UI.AddColumnButton == null) return;
            UI.AddColumnButton.SetAnchoredPosition(columnButtonPos.Value);
        };
        rowButtonPos = 
            ConfigManager.config("UI", "Row Button", new Vector2(370f, 153f),  "Set row button position", false);
        rowButtonPos.SettingChanged += (sender, args) =>
        {
            if (UI.AddRowButton == null) return;
            UI.AddRowButton.SetAnchoredPosition(rowButtonPos.Value);
        };

        addColumnTrigger = ConfigManager.config("Gamepad", "Add Column", JoyButton.LTrigger,
            "Set gamepad trigger to add column", false);
        addColumnTrigger.SettingChanged += (sender, args) =>
        {
            if (UI.AddColumnButton == null) return;
            UI.AddColumnButton.SetGamepadKey(AddColumnTrigger);
        };

        addRowTrigger =
            ConfigManager.config("Gamepad", "Add Row", JoyButton.RTrigger, 
                "Set gamepad trigger to add row", false);
        addRowTrigger.SettingChanged += (sender, args) =>
        {
            if (UI.AddRowButton == null) return;
            UI.AddRowButton.SetGamepadKey(AddRowTrigger);
        };

        costType = ConfigManager.config("Settings", "Cost Type", CostType.Percentage,
            "Full = total piece requirements, Half = 50% piece requirements, Percentage = percentage of piece requirements calculated from added slots");

        upgradeType = ConfigManager.config("Settings", "Upgrade Limit", UpgradeType.Infinite,
            "Infinite = no limit, Double = twice the original container size, Triple = triple the original containter size");
    }
}

public class UpgradeMap
{
    private Dictionary<string, ItemRes[]> map = new Dictionary<string, ItemRes[]>();

    public UpgradeMap(string configBase)
    {
        var parts = configBase.Split(';');
        for (int i = 0; i < parts.Length; ++i)
        {
            var part = parts[i];
            var s = part.Split(':');
            if (s.Length < 2) continue;
            var prefabId = s[0];
            var items = s[1].Split(',');
            map[prefabId] = items
                .Select(x => new ItemRes(x))
                .ToArray();

        }
    }

    public bool GetCost(Container container, out Piece.Requirement[] cost)
    {
        cost = [];
        var prefabId = Utils.GetPrefabName(container.name);
        if (!map.TryGetValue(prefabId, out var items)) return false;
        List<Piece.Requirement> result = [];
        for (int i = 0; i < items.Length; ++i)
        {
            var res = items[i];
            var prefab = ObjectDB.instance.GetItemPrefab(res.itemId);
            if (prefab == null || !prefab.TryGetComponent(out ItemDrop component)) continue;
            var req = new Piece.Requirement
            {
                m_resItem = component,
                m_amount = res.amount
            };
            result.Add(req);
        }

        cost = result.ToArray();
        return cost.Length > 0;
    }

    public class ItemRes
    {
        public string itemId;
        public int amount;

        public ItemRes(string str)
        {
            var parts = str.Split('*');
            itemId = parts[0];
            if (!int.TryParse(parts[1], out amount))
            {
                amount = 1;
            }
        }
    }
}