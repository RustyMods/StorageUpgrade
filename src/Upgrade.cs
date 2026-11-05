using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace StorageUpgrade;

public static class Upgrade
{
    [HarmonyPatch(typeof(Container), nameof(Container.Load))]
    private static class Container_Load_Patch
    {
        private static void Postfix(Container __instance, bool __result)
        {
            if (!__result) return;
            var height = __instance.m_nview.GetZDO().GetInt(Vars.height, __instance.m_height);
            var width = __instance.m_nview.GetZDO().GetInt(Vars.width, __instance.m_width);

            __instance.m_inventory.m_height = height;
            __instance.m_inventory.m_width = width;
        }
    }

    extension(Container container)
    {
        public bool CanAddWidth()
        {
            return container.m_inventory.m_width < 7;
        }

        public bool CanPurchaseWidth(Player player, int amount, out Piece.Requirement[] cost)
        {
            cost = container.GetColumnCost(amount);

            if (Configs.UpgradeType != UpgradeType.Infinite)
            {
                var baseTotal = container.m_height * container.m_width;
                var newTotal = container.m_inventory.m_height * (container.m_inventory.m_width + 1);
                switch (Configs.UpgradeType)
                {
                    case UpgradeType.Double:
                        if (newTotal > baseTotal * 2) return false;
                        break;
                    case UpgradeType.Triple:
                        if (newTotal > baseTotal * 3) return false;
                        break;
                }
            }

            if (player.NoCostCheat()) return true;
            
            for (int i = 0; i < cost.Length; ++i)
            {
                var res = cost[i];
                var count = player.GetInventory().CountItems(res.m_resItem.m_itemData.m_shared.m_name);
                if (count < res.m_amount) return false;
            }

            return true;
        }

        public bool CanPurchaseHeight(Player player, int amount, out Piece.Requirement[] cost)
        {
            cost = container.GetRowCost(amount);
            
            if (Configs.UpgradeType != UpgradeType.Infinite)
            {
                var baseTotal = container.m_height * container.m_width;
                var newTotal = (container.m_inventory.m_height + 1) * container.m_inventory.m_width;
                switch (Configs.UpgradeType)
                {
                    case UpgradeType.Double:
                        if (newTotal > baseTotal * 2) return false;
                        break;
                    case UpgradeType.Triple:
                        if (newTotal > baseTotal * 3) return false;
                        break;
                }
            }

            if (player.NoCostCheat()) return true;
            
            for (int i = 0; i < cost.Length; ++i)
            {
                var res = cost[i];
                var count = player.GetInventory().CountItems(res.m_resItem.m_itemData.m_shared.m_name);
                if (count < res.m_amount) return false;
            }
            return true;
        }

        public Piece.Requirement[] GetRowCost(int amount)
        {
            var baseTotal = container.m_height * container.m_width;
            var height = container.m_inventory.m_height;
            var width = container.m_inventory.m_width;
        
            var total = height * width;

            var newHeight = height + amount;
            var newTotal = width * newHeight;

            var difference = newTotal - total;

            var percentage = (float)difference / baseTotal;
            var requirements = container.m_piece.m_resources;
            if (Configs.UpgradeMap.GetCost(container, out var cost))
            {
                requirements = cost;
            }
            cost = CalculateCost(requirements, percentage);
            
            return cost;
        }

        public string GetHeightCostTooltip(int amount)
        {
            var cost = container.GetRowCost(amount);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < cost.Length; ++i)
            {
                var res = cost[i];
                sb.Append($"{res.m_resItem.m_itemData.m_shared.m_name} x{res.m_amount}\n");
            }

            return sb.ToString();
        }

        public Piece.Requirement[] GetColumnCost(int amount)
        {
            var baseTotal = container.m_height * container.m_width;
        
            var height = container.m_inventory.m_height;
            var width = container.m_inventory.m_width;
            var total = height * width;

            var newWidth = width + amount;
            var newTotal = height * newWidth;

            var difference = newTotal - total;

            var percentage = (float)difference / baseTotal;
            var requirements = container.m_piece.m_resources;
            if (Configs.UpgradeMap.GetCost(container, out var cost))
            {
                requirements = cost;
            }
            cost = CalculateCost(requirements, percentage);
            return cost;
        }

        public string GetWidthCostTooltip(int amount)
        {
            var cost = container.GetColumnCost(amount);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < cost.Length; ++i)
            {
                var res = cost[i];
                sb.Append($"{res.m_resItem.m_itemData.m_shared.m_name} x{res.m_amount}\n");
            }

            return sb.ToString();
        }

        public bool AddHeight(int amount)
        {
            var height = container.m_inventory.m_height;
            var newHeight = height + amount;
            if (!container.CanPurchaseHeight(Player.m_localPlayer, amount, out var cost)) return false;
            if (!Player.m_localPlayer.NoCostCheat())
            {
                Player.m_localPlayer.ConsumeResources(cost, 1);
                container.SaveResources(cost);
            }
            container.m_nview.GetZDO().Set(Vars.height, newHeight);
            container.m_inventory.m_height = newHeight;
            return true;
        }

        public bool AddWidth(int amount)
        {
            var width = container.m_inventory.m_width;
            if (width >= 7) return false;
            var newWidth = width + amount;
            if (!container.CanPurchaseWidth(Player.m_localPlayer, amount, out var cost)) return false;
            if (!Player.m_localPlayer.NoCostCheat())
            {
                Player.m_localPlayer.ConsumeResources(cost, 1);
                container.SaveResources(cost);
            }
            container.m_nview.GetZDO().Set(Vars.width, newWidth);
            container.m_inventory.m_width = newWidth;
            return true;
        }

        public void SaveResources(Piece.Requirement[] cost)
        {
            Dictionary<string, int> resources = new Dictionary<string, int>();
            var record = container.m_nview.GetZDO().GetString(Vars.resources);
            if (!string.IsNullOrEmpty(record))
            {
                var pkg = new ZPackage(record);
                var count = pkg.ReadInt();
                for (int i = 0; i < count; ++i)
                {
                    var prefab = pkg.ReadString();
                    var amount = pkg.ReadInt();
                    resources[prefab] = amount;
                }
            }

            for (int i = 0; i < cost.Length; ++i)
            {
                var res = cost[i];
                var prefab = Utils.GetPrefabName(res.m_resItem.name);
                var amount = res.m_amount;
                if (!resources.ContainsKey(prefab))
                {
                    resources[prefab] = amount;
                }
                else
                {
                    resources[prefab] += amount;
                }
            }

            var package = new ZPackage();
            package.Write(resources.Count);
            foreach (var kvp in resources)
            {
                package.Write(kvp.Key);
                package.Write(kvp.Value);
            }
            
            container.m_nview.GetZDO().Set(Vars.resources, package.GetBase64());
        }

        public Piece.Requirement[] GetUsedUpgradeResources()
        {
            List<Piece.Requirement> result = [];
            var record = container.m_nview.GetZDO().GetString(Vars.resources);
            if (string.IsNullOrEmpty(record))
            {
                return result.ToArray();
            }
            
            var pkg = new ZPackage(record);
            var count = pkg.ReadInt();
            for (int i = 0; i < count; ++i)
            {
                var name = pkg.ReadString();
                var amount = pkg.ReadInt();
                var prefab = ObjectDB.instance.GetItemPrefab(name);
                if (prefab == null || !prefab.TryGetComponent(out ItemDrop component)) continue;
                var res = new Piece.Requirement
                {
                    m_resItem = component,
                    m_amount = amount,
                };
                result.Add(res);
            }

            return result.ToArray();
        }
    }

    public static Piece.Requirement[] CalculateCost(Piece.Requirement[] baseRequirements, float multiplier)
    {
        switch (Configs.CostType)
        {
            case CostType.Full:
                return baseRequirements;
            case CostType.Half:
                return baseRequirements
                    .Select(r => new Piece.Requirement{m_resItem = r.m_resItem, m_amount = Mathf.Max(Mathf.CeilToInt(r.m_amount / 2f), 1)})
                    .ToArray();
            default:
                return baseRequirements
                    .Select(r => new Piece.Requirement{m_resItem = r.m_resItem, m_amount = Mathf.Max(Mathf.CeilToInt(r.m_amount * multiplier), 1)})
                    .ToArray();
        }
    }
}