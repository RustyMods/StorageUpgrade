using HarmonyLib;

namespace StorageUpgrade;

public static class UI
{
    public static InvButton AddRowButton;
    public static InvButton AddColumnButton;
    
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Awake))]
    private static class InventoryGUI_Awake_Patches
    {
        private static void Prefix(InventoryGui __instance)
        {
            var takeAll = __instance.m_takeAllButton.gameObject;
            
            AddRowButton = new InvButton(takeAll, takeAll.transform.parent, Configs.RowPos);
            AddRowButton.button.onClick.AddListener(AddHeight);
            AddRowButton.SetLabel("$storage_upgrade_add_row");
            AddRowButton.SetGamepadKey(Configs.AddRowTrigger);
            
            AddColumnButton = new InvButton(takeAll, takeAll.transform.parent, Configs.ColumnPos);
            AddColumnButton.button.onClick.AddListener(AddWidth);
            AddColumnButton.SetLabel("$storage_upgrade_add_column");
            AddColumnButton.SetGamepadKey(Configs.AddColumnTrigger);
        }

        private static void Postfix(InventoryGui __instance)
        {
            var tooltip = __instance.m_containerGrid.m_elementPrefab.GetComponent<UITooltip>().m_tooltipPrefab;
            
            AddRowButton.tooltip.m_tooltipPrefab = tooltip;
            AddColumnButton.tooltip.m_tooltipPrefab = tooltip;
        }
    }

    private static void AddHeight()
    {
        if (InventoryGui.instance == null) return;
        if (InventoryGui.instance.m_currentContainer == null) return;
        if (InventoryGui.instance.m_currentContainer.m_nview == null) return;
        
        UnifiedPopup.Push(new YesNoPopup(AddRowButton.tmp.text, InventoryGui.instance.m_currentContainer.GetHeightCostTooltip(1),
            () =>
            {
                if (!InventoryGui.instance.m_currentContainer.AddHeight(1))
                {
                    UnifiedPopup.instance.Hide();
                    return;
                }
                
                AddColumnButton.instance.SetActive(InventoryGui.instance.m_currentContainer.CanAddWidth());
                AddRowButton.instance.SetActive(InventoryGui.instance.m_currentContainer.CanAddHeight());
        
                AddRowButton.tooltip.Set(AddRowButton.tmp.text, InventoryGui.instance.m_currentContainer.GetHeightCostTooltip(1));
                AddColumnButton.tooltip.Set(AddColumnButton.tmp.text, InventoryGui.instance.m_currentContainer.GetWidthCostTooltip(1));
        
                AddRowButton.button.interactable = InventoryGui.instance.m_currentContainer.CanPurchaseHeight(Player.m_localPlayer, 1, out _);
                AddColumnButton.button.interactable = InventoryGui.instance.m_currentContainer.CanPurchaseWidth(Player.m_localPlayer, 1, out _);

                if (!AddRowButton.button.interactable)
                {
                    UnifiedPopup.instance.Hide();
                }
                
            }, () =>
            {
                UnifiedPopup.instance.Hide();
            }));
    }

    private static void AddWidth()
    {
        if (InventoryGui.instance == null) return;
        if (InventoryGui.instance.m_currentContainer == null) return;
        if (InventoryGui.instance.m_currentContainer.m_nview == null) return;
        
        UnifiedPopup.Push(new YesNoPopup(AddColumnButton.tmp.text, InventoryGui.instance.m_currentContainer.GetWidthCostTooltip(1),
            () =>
            {
                if (!InventoryGui.instance.m_currentContainer.AddWidth(1))
                {
                    UnifiedPopup.instance.Hide();
                    return;
                }
        
                AddColumnButton.instance.SetActive(InventoryGui.instance.m_currentContainer.CanAddWidth());
                AddRowButton.instance.SetActive(InventoryGui.instance.m_currentContainer.CanAddHeight());
        
                AddRowButton.tooltip.Set(AddRowButton.tmp.text, InventoryGui.instance.m_currentContainer.GetHeightCostTooltip(1));
                AddColumnButton.tooltip.Set(AddColumnButton.tmp.text, InventoryGui.instance.m_currentContainer.GetWidthCostTooltip(1));
        
                AddRowButton.button.interactable = InventoryGui.instance.m_currentContainer.CanPurchaseHeight(Player.m_localPlayer, 1, out _);
                AddColumnButton.button.interactable = InventoryGui.instance.m_currentContainer.CanPurchaseWidth(Player.m_localPlayer, 1, out _);

                if (!AddColumnButton.button.interactable)
                {
                    UnifiedPopup.instance.Hide();
                }
            }, () =>
            {
                UnifiedPopup.instance.Hide();

            }));
    }
    
    [HarmonyPatch(typeof(InventoryGui), nameof(InventoryGui.Show))]
    private static class InventoryGUI_Show_Postfix
    {
        private static void Postfix(Container container)
        {
            bool show = container != null && container.m_nview != null;
            AddRowButton.instance.SetActive(show && container.CanAddHeight());
            AddColumnButton.instance.SetActive(Configs.AddColumns && show && container.CanAddWidth());

            if (container == null) return;
            AddRowButton.button.interactable = container.CanPurchaseHeight(Player.m_localPlayer, 1, out _);
            AddColumnButton.button.interactable = container.CanPurchaseWidth(Player.m_localPlayer, 1, out _);
            
            AddRowButton.tooltip.Set(AddRowButton.tmp.text, container.GetHeightCostTooltip(1));
            AddColumnButton.tooltip.Set(AddColumnButton.tmp.text, container.GetWidthCostTooltip(1));
        }
    }
}