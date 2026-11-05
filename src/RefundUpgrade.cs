using System.Text;
using HarmonyLib;
using UnityEngine;

namespace StorageUpgrade;

public static class OnDestroy
{
    [HarmonyPatch(typeof(Piece), nameof(Piece.DropResources))]
    private static class Piece_DropResources_Postfix
    {
        private static void Postfix(Piece __instance, HitData hitData)
        {
            if (!__instance.TryGetComponent(out Container container)) return;
            if (!Configs.Refund) return;

            var cost = container.GetResources();

            for (int i = 0; i < cost.Length; ++i)
            {
                var res = cost[i];
                var dropCount = res.m_amount;
                var itemPrefab = ObjectDB.instance.GetItemPrefab(Utils.GetPrefabName(res.m_resItem.name));
                var original = Game.instance.CheckDropConversion(hitData, res.m_resItem, itemPrefab, ref dropCount);
                var prefab = Object.Instantiate(original, __instance.transform.position + Vector3.up * __instance.m_returnResourceHeightOffset, Quaternion.identity);
                var itemDrop = prefab.GetComponent<ItemDrop>();
                itemDrop.SetStack(Mathf.Min(dropCount, itemDrop.m_itemData.m_shared.m_maxStackSize));
                ItemDrop.OnCreateNew(itemDrop);
                if (__instance.m_nview.GetZDO().IsCheated() && !PlayerProfile.s_bypassCheatChecks)
                {
                    itemDrop.m_itemData.m_cheated = true;
                }
            }
        }
    }
}