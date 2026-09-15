using TMPro;
using UnityEngine;
using UnityEngine.UI;

// High-fidelity authored layout for WildRoot Player Menu and dedicated Net UI.
// Reparents the actual inventory panel at runtime, preserving all slots and state.
public static class PlayerMenuLayout
{
    public static void Build(GameObject inventory, out GameObject root, out GameObject player,
        out GameObject settings, out Button playerTab, out Button settingsTab,
        out NetStorageListUI netList, out UnityEngine.UI.Image portrait,
        out GameObject craftingPage, out Button craftingTab, out HandCraftingMenuUI craftingView)
    {
        // 1. Root Container (Centered, responsive fixed-canvas scale)
        RectTransform frame = Panel(inventory.transform.parent, "Player Menu (Runtime)",
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), WildRootUITheme.PanelFrame);
        frame.anchoredPosition = Vector2.zero;
        frame.sizeDelta = new Vector2(1180, 720);
        root = frame.gameObject;

        // 2. Header Bar
        RectTransform headerBar = new GameObject("HeaderBar", typeof(RectTransform)).GetComponent<RectTransform>();
        headerBar.SetParent(frame, false);
        headerBar.anchorMin = new Vector2(0f, 1f);
        headerBar.anchorMax = new Vector2(1f, 1f);
        headerBar.pivot = new Vector2(0.5f, 1f);
        headerBar.anchoredPosition = new Vector2(0, -14);
        headerBar.sizeDelta = new Vector2(-40, 52);

        // Navigation Tabs (Top-Left)
        playerTab = CreateTabButton(headerBar, "Player Tab", "✦ Adventurer", true);
        RectTransform pTabRect = (RectTransform)playerTab.transform;
        pTabRect.anchorMin = new Vector2(0f, 0f);
        pTabRect.anchorMax = new Vector2(0f, 1f);
        pTabRect.pivot = new Vector2(0f, 0.5f);
        pTabRect.anchoredPosition = new Vector2(0, 0);
        pTabRect.sizeDelta = new Vector2(170, 0);

        craftingTab = CreateTabButton(headerBar, "Crafting Tab", "⚒ Crafting", false);
        RectTransform cTabRect = (RectTransform)craftingTab.transform;
        cTabRect.anchorMin = new Vector2(0f, 0f);
        cTabRect.anchorMax = new Vector2(0f, 1f);
        cTabRect.pivot = new Vector2(0f, 0.5f);
        cTabRect.anchoredPosition = new Vector2(178, 0);
        cTabRect.sizeDelta = new Vector2(160, 0);

        settingsTab = CreateTabButton(headerBar, "Settings Tab", "⚙ Settings", false);
        RectTransform sTabRect = (RectTransform)settingsTab.transform;
        sTabRect.anchorMin = new Vector2(0f, 0f);
        sTabRect.anchorMax = new Vector2(0f, 1f);
        sTabRect.pivot = new Vector2(0f, 0.5f);
        sTabRect.anchoredPosition = new Vector2(346, 0);
        sTabRect.sizeDelta = new Vector2(150, 0);

        // Header Title (Center)
        TMP_Text mainTitle = Label(headerBar, "Main Title", "WILDROOT EXPEDITION", 22f, FontStyles.Bold,
            new Vector2(0.48f, 0f), new Vector2(0.80f, 1f), WildRootUITheme.ColorTextGold);

        // Close Hint & Button (Top-Right)
        RectTransform closeBadge = Panel(headerBar, "Close Badge", new Vector2(1f, 0f), new Vector2(1f, 1f), WildRootUITheme.BadgeDark);
        closeBadge.pivot = new Vector2(1f, 0.5f);
        closeBadge.anchoredPosition = Vector2.zero;
        closeBadge.sizeDelta = new Vector2(140, 36);
        TMP_Text closeHint = Label(closeBadge, "Close Hint", "ESC  Close", 14f, FontStyles.Bold, Vector2.zero, Vector2.one, WildRootUITheme.ColorTextSubtitle);

        // 3. Player Page Container
        RectTransform playerBody = new GameObject("Player Page", typeof(RectTransform)).GetComponent<RectTransform>();
        playerBody.SetParent(frame, false);
        playerBody.anchorMin = Vector2.zero;
        playerBody.anchorMax = Vector2.one;
        playerBody.offsetMin = new Vector2(20, 20);
        playerBody.offsetMax = new Vector2(-20, -74);
        player = playerBody.gameObject;

        // --- Column 1: Net Storage Region (Left) ---
        RectTransform netRegion = Panel(playerBody, "Net Region", new Vector2(0f, 0f), new Vector2(0.29f, 1f), WildRootUITheme.PanelInner);
        netList = CreateAnimalList(netRegion);

        // --- Column 2: Inventory Region (Center) ---
        RectTransform center = Panel(playerBody, "Inventory Region", new Vector2(0.305f, 0f), new Vector2(0.725f, 1f), WildRootUITheme.PanelInner);
        
        // Header Ribbon
        WildRootUITheme.CreateHeaderRibbon(center, "Inventory Header", "BACKPACK INVENTORY", "Quick Slots 1–10  •  Storage 11–20");
        RectTransform invRibbon = center.Find("Inventory Header") as RectTransform;
        if (invRibbon != null)
        {
            invRibbon.anchorMin = new Vector2(0.04f, 1f);
            invRibbon.anchorMax = new Vector2(0.96f, 1f);
            invRibbon.pivot = new Vector2(0.5f, 1f);
            invRibbon.anchoredPosition = new Vector2(0, -12);
            invRibbon.sizeDelta = new Vector2(0, 48);
        }

        // Reparent and position the inventory slots
        inventory.transform.SetParent(center, false);
        RectTransform invRect = (RectTransform)inventory.transform;
        invRect.anchorMin = new Vector2(0.04f, 0.04f);
        invRect.anchorMax = new Vector2(0.96f, 0.88f);
        invRect.offsetMin = Vector2.zero;
        invRect.offsetMax = Vector2.zero;
        inventory.SetActive(true);

        // Hide legacy title inside inventoryPanel if present
        Transform legacyTitle = inventory.transform.Find("Title");
        if (legacyTitle != null) legacyTitle.gameObject.SetActive(false);

        // Configure Grid Layout Group
        GridLayoutGroup grid = inventory.GetComponentInChildren<GridLayoutGroup>(true);
        if (grid != null)
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 5;
            grid.cellSize = new Vector2(72, 72);
            grid.spacing = new Vector2(8, 8);
            grid.padding = new RectOffset(12, 12, 12, 12);
            grid.childAlignment = TextAnchor.MiddleCenter;

            RectTransform gridRect = (RectTransform)grid.transform;
            gridRect.anchorMin = Vector2.zero;
            gridRect.anchorMax = Vector2.one;
            gridRect.offsetMin = Vector2.zero;
            gridRect.offsetMax = Vector2.zero;
        }

        // Style the inventory slots with cohesive WildRoot slot sprites
        InventorySlotUI[] slots = inventory.GetComponentsInChildren<InventorySlotUI>(true);
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            UnityEngine.UI.Image slotBg = slots[i].GetComponent<UnityEngine.UI.Image>();
            if (slotBg != null)
            {
                // First 10 slots are hotbar slots
                slotBg.sprite = (i < 10) ? WildRootUITheme.SlotHotbar : WildRootUITheme.SlotNormal;
                slotBg.type = UnityEngine.UI.Image.Type.Sliced;
                slotBg.color = Color.white;
            }
        }

        // --- Column 3: Player Presentation Region (Right) ---
        RectTransform right = Panel(playerBody, "Player Presentation Region", new Vector2(0.74f, 0f), new Vector2(1f, 1f), WildRootUITheme.PanelInner);
        
        WildRootUITheme.CreateHeaderRibbon(right, "Player Header", "ADVENTURER", "Explorer Profile");
        RectTransform pRibbon = right.Find("Player Header") as RectTransform;
        if (pRibbon != null)
        {
            pRibbon.anchorMin = new Vector2(0.06f, 1f);
            pRibbon.anchorMax = new Vector2(0.94f, 1f);
            pRibbon.pivot = new Vector2(0.5f, 1f);
            pRibbon.anchoredPosition = new Vector2(0, -12);
            pRibbon.sizeDelta = new Vector2(0, 48);
        }

        // Portrait Frame
        RectTransform portraitBox = Panel(right, "Portrait Frame", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), WildRootUITheme.SlotHotbar);
        portraitBox.anchoredPosition = new Vector2(0, 30);
        portraitBox.sizeDelta = new Vector2(190, 190);

        RectTransform portraitRect = new GameObject("Player Portrait", typeof(RectTransform), typeof(UnityEngine.UI.Image)).GetComponent<RectTransform>();
        portraitRect.SetParent(portraitBox, false);
        WildRootUITheme.FitStretch(portraitRect, 12, 12, 12, 12);
        portrait = portraitRect.GetComponent<UnityEngine.UI.Image>();
        portrait.color = Color.white;
        portrait.preserveAspect = true;
        portrait.raycastTarget = false;

        // Explorer Badge / Info
        RectTransform infoBadge = Panel(right, "Explorer Info", new Vector2(0.06f, 0.05f), new Vector2(0.94f, 0.28f), WildRootUITheme.BadgeDark);
        TMP_Text infoText = Label(infoBadge, "Status Text",
            "<b>WildRoot Explorer</b>\n<color=#A8C59A>Terrarium & Field Specialist</color>\n<size=12><color=#D4C2A3>Ready for Exploration</color></size>",
            14f, FontStyles.Normal, Vector2.zero, Vector2.one, WildRootUITheme.ColorTextBody);

        // 4. Crafting Page Container
        craftingPage = HandCraftingMenuUI.CreatePage(frame);
        craftingView = craftingPage.GetComponent<HandCraftingMenuUI>();

        // 5. Settings Page Container
        RectTransform settingsBody = Panel(frame, "Settings Page", Vector2.zero, Vector2.one, WildRootUITheme.PanelInner);
        settingsBody.offsetMin = new Vector2(20, 20);
        settingsBody.offsetMax = new Vector2(-20, -74);
        settings = settingsBody.gameObject;

        WildRootUITheme.CreateHeaderRibbon(settingsBody, "Settings Header", "GAME SETTINGS", "Audio, Video & Controls");
        RectTransform sHeader = settingsBody.Find("Settings Header") as RectTransform;
        if (sHeader != null)
        {
            sHeader.anchorMin = new Vector2(0.25f, 1f);
            sHeader.anchorMax = new Vector2(0.75f, 1f);
            sHeader.pivot = new Vector2(0.5f, 1f);
            sHeader.anchoredPosition = new Vector2(0, -16);
            sHeader.sizeDelta = new Vector2(0, 52);
        }

        RectTransform settingsCard = Panel(settingsBody, "Settings Card", new Vector2(0.2f, 0.2f), new Vector2(0.8f, 0.8f), WildRootUITheme.BadgeDark);
        Label(settingsCard, "Placeholder",
            "<b><size=20><color=#FFF3D6>WildRoot Configuration</color></size></b>\n\n<color=#D4C2A3>Gameplay, Audio, Graphics and Keybinding options\nwill be configurable here.</color>",
            16f, FontStyles.Normal, Vector2.zero, Vector2.one, WildRootUITheme.ColorTextBody);

        settings.SetActive(false);
    }

    public static void Build(GameObject inventory, out GameObject root, out GameObject player,
        out GameObject settings, out Button playerTab, out Button settingsTab,
        out NetStorageListUI netList, out UnityEngine.UI.Image portrait)
    {
        Build(inventory, out root, out player, out settings, out playerTab, out settingsTab,
            out netList, out portrait, out _, out _, out _);
    }

    public static Button CreateTabButton(Transform parent, string name, string caption, bool isActive)
    {
        RectTransform rect = Panel(parent, name, Vector2.zero, Vector2.one, isActive ? WildRootUITheme.TabActive : WildRootUITheme.TabInactive);
        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = rect.GetComponent<UnityEngine.UI.Image>();

        TMP_Text label = Label(rect, "Label", caption, 16f, FontStyles.Bold, Vector2.zero, Vector2.one,
            isActive ? WildRootUITheme.ColorTextTitle : WildRootUITheme.ColorTextMuted);

        return button;
    }

    public static Button Button(Transform parent, string name, string caption)
    {
        return WildRootUITheme.CreateButton(parent, name, caption, 16f, WildRootUITheme.ButtonNormal, WildRootUITheme.ButtonHover);
    }

    public static NetStorageListUI CreateAnimalList(RectTransform netRegion)
    {
        // Header Ribbon
        WildRootUITheme.CreateHeaderRibbon(netRegion, "Net Header", "CREATURE NET", "Stored Companions");
        RectTransform header = netRegion.Find("Net Header") as RectTransform;
        if (header != null)
        {
            header.anchorMin = new Vector2(0.04f, 1f);
            header.anchorMax = new Vector2(0.96f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.anchoredPosition = new Vector2(0, -12);
            header.sizeDelta = new Vector2(0, 48);
        }

        // Capacity & Status Badge (Bottom)
        RectTransform statusBadge = Panel(netRegion, "Net Status Badge", new Vector2(0.04f, 0.03f), new Vector2(0.96f, 0.16f), WildRootUITheme.BadgeDark);
        TMP_Text status = Label(statusBadge, "Net status", "", 13f, FontStyles.Normal, Vector2.zero, Vector2.one, WildRootUITheme.ColorTextSubtitle);

        // Scrollable Animal Viewport
        RectTransform viewport = Panel(netRegion, "Net Viewport", new Vector2(0.04f, 0.18f), new Vector2(0.96f, 0.88f), WildRootUITheme.BadgeDark);
        viewport.gameObject.AddComponent<RectMask2D>();
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();

        RectTransform content = new GameObject("Animals", typeof(RectTransform)).GetComponent<RectTransform>();
        content.SetParent(viewport, false);
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = Vector2.one;
        content.pivot = new Vector2(0.5f, 1);
        content.sizeDelta = Vector2.zero;

        VerticalLayoutGroup layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.spacing = 8;
        layout.padding = new RectOffset(6, 6, 6, 6);
        layout.childControlHeight = true;
        layout.childControlWidth = true;
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;

        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 25;

        NetStorageListUI netList = netRegion.gameObject.AddComponent<NetStorageListUI>();
        netList.Configure(content, status);
        return netList;
    }

    public static RectTransform Panel(Transform parent, string name, Vector2 min, Vector2 max, Sprite sprite = null, Color? color = null)
    {
        RectTransform rect = WildRootUITheme.CreatePanel(parent, name, sprite != null ? sprite : WildRootUITheme.PanelFrame, color);
        Fit(rect, min, max);
        return rect;
    }

    public static TMP_Text Label(Transform parent, string name, string caption, Vector2 min, Vector2 max)
    {
        return Label(parent, name, caption, 18f, FontStyles.Normal, min, max, WildRootUITheme.ColorTextBody);
    }

    public static TMP_Text Label(Transform parent, string name, string caption, float fontSize, FontStyles fontStyle, Vector2 min, Vector2 max, Color color)
    {
        TMP_Text text = WildRootUITheme.CreateText(parent, name, caption, fontSize, fontStyle, TextAlignmentOptions.Center, color);
        Fit((RectTransform)text.transform, min, max);
        return text;
    }

    public static void Fit(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }
}
