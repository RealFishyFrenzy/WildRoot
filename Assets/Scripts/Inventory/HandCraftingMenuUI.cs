using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// High-fidelity WildRoot Hand-Crafting interface.
// Presentation and UI only; crafting rules and availability reside in HandCraftingMenuState.
public class HandCraftingMenuUI : MonoBehaviour
{
    [Header("Catalog / Recipe List")]
    [SerializeField] private RectTransform recipeContent;
    [SerializeField] private ScrollRect recipeScrollRect;
    [SerializeField] private GameObject catalogEmptyState;

    [Header("Details Area")]
    [SerializeField] private GameObject detailsRoot;
    [SerializeField] private GameObject detailsEmptyState;
    [SerializeField] private TMP_Text detailsEmptyText;

    [Header("Details - Output Preview")]
    [SerializeField] private UnityEngine.UI.Image resultIcon;
    [SerializeField] private TMP_Text resultNameText;
    [SerializeField] private TMP_Text resultQuantityText;

    [Header("Details - Ingredients")]
    [SerializeField] private RectTransform ingredientContent;
    [SerializeField] private ScrollRect ingredientScrollRect;

    [Header("Details - Action")]
    [SerializeField] private Button craftButton;
    [SerializeField] private TMP_Text statusHintText;

    private readonly List<RecipeCardWidget> recipeWidgets = new List<RecipeCardWidget>();
    private readonly List<GameObject> ingredientWidgets = new List<GameObject>();
    public HandCraftingMenuState State { get; private set; }

    public void Bind(PlayerInventory inventory, IEnumerable<CraftingRecipe> recipes)
    {
        Detach();
        EnsureWidgets();
        State = new HandCraftingMenuState(inventory, recipes);
        State.Changed += Refresh;

        // Clear existing recipe cards
        foreach (var widget in recipeWidgets)
        {
            if (widget != null && widget.Root != null)
            {
                widget.Root.SetActive(false);
                Destroy(widget.Root);
            }
        }
        recipeWidgets.Clear();

        // Build recipe cards
        if (State.Recipes != null && recipeContent != null)
        {
            foreach (CraftingRecipe recipe in State.Recipes)
            {
                if (recipe == null) continue;
                RecipeCardWidget card = CreateRecipeCard(recipeContent, recipe);
                recipeWidgets.Add(card);
            }
        }

        if (isActiveAndEnabled) State.Observe();
        Refresh();
    }

    private void OnEnable() { State?.Observe(); }
    private void OnDisable() { State?.Dispose(); }
    private void OnDestroy()
    {
        Detach();
        if (craftButton != null) craftButton.onClick.RemoveListener(Craft);
    }

    private void Detach()
    {
        if (State == null) return;
        State.Changed -= Refresh;
        State.Dispose();
    }

    public void ClearSelection() => State?.Select(null);

    public void Craft()
    {
        if (!isActiveAndEnabled || State == null) return;
        ItemData resultItem = State.ResultItem;
        int resultQuantity = State.ResultQuantity;
        if (State.Craft())
        {
            string itemName = resultItem != null ? resultItem.itemName : "Item";
            GameplayNotifications.Show($"Crafted {resultQuantity} × {itemName}");
        }
        else
        {
            GameplayNotifications.Show("Cannot craft: check ingredients and inventory space", "hand-crafting-failure");
        }
    }

    private void Refresh()
    {
        if (State == null) return;

        bool hasRecipes = State.Recipes != null && State.Recipes.Count > 0;

        if (catalogEmptyState != null) catalogEmptyState.SetActive(!hasRecipes);
        if (recipeScrollRect != null) recipeScrollRect.gameObject.SetActive(hasRecipes);

        if (!hasRecipes)
        {
            if (detailsRoot != null) detailsRoot.SetActive(false);
            if (detailsEmptyState != null)
            {
                detailsEmptyState.SetActive(true);
                if (detailsEmptyText != null)
                {
                    detailsEmptyText.text =
                        "<b><size=18><color=#FFF3D6>No Crafting Recipes</color></size></b>\n\n" +
                        "<size=14><color=#D4C2A3>There are currently no hand-crafting blueprints in your catalog.\n" +
                        "Explore the wilderness and gather materials to discover new recipes.</color></size>";
                }
            }
            if (craftButton != null) craftButton.interactable = false;
            return;
        }

        // Update Recipe Cards
        for (int i = 0; i < recipeWidgets.Count && i < State.Recipes.Count; i++)
        {
            CraftingRecipe recipe = State.Recipes[i];
            RecipeCardWidget widget = recipeWidgets[i];
            if (widget == null || recipe == null) continue;

            bool canCraft = State.CanCraftRecipe(recipe);
            bool isSelected = State.Selected == recipe;

            widget.SetState(recipe, canCraft, isSelected);
        }

        // Update Details View
        if (State.Selected == null)
        {
            if (detailsRoot != null) detailsRoot.SetActive(false);
            if (detailsEmptyState != null)
            {
                detailsEmptyState.SetActive(true);
                if (detailsEmptyText != null)
                {
                    detailsEmptyText.text =
                        "<b><size=18><color=#FFF3D6>Select a Blueprint</color></size></b>\n\n" +
                        "<size=14><color=#D4C2A3>Choose a recipe from the catalog on the left to view\n" +
                        "required ingredients and craft items.</color></size>";
                }
            }
            if (craftButton != null) craftButton.interactable = false;
        }
        else
        {
            if (detailsEmptyState != null) detailsEmptyState.SetActive(false);
            if (detailsRoot != null) detailsRoot.SetActive(true);

            // 1. Output Preview
            ItemData outputItem = State.ResultItem;
            int outputAmount = State.ResultQuantity;
            string itemName = outputItem != null ? outputItem.itemName : "Unknown Item";

            if (resultIcon != null)
            {
                resultIcon.sprite = outputItem != null ? outputItem.icon : null;
                resultIcon.enabled = resultIcon.sprite != null;
            }
            if (resultNameText != null)
            {
                resultNameText.text = $"<b><color=#FFE8A3>{itemName}</color></b>";
            }
            if (resultQuantityText != null)
            {
                resultQuantityText.text = $"<color=#A8C59A>Batch Output: {outputAmount} × {itemName}</color>";
            }

            // 2. Ingredients List
            RefreshIngredients(State.GetIngredients());

            // 3. Action Box & Status Hint
            bool canCraft = State.CanCraft;
            if (craftButton != null) craftButton.interactable = canCraft;

            if (statusHintText != null)
            {
                if (canCraft)
                {
                    statusHintText.text = "<color=#88C888>✦ All ingredients collected — ready to craft!</color>";
                }
                else
                {
                    statusHintText.text = "<color=#E68369>⚠ Insufficient materials or inventory space.</color>";
                }
            }
        }
    }

    private void RefreshIngredients(IReadOnlyList<HandCraftingIngredientState> ingredients)
    {
        // Clear previous rows
        foreach (var obj in ingredientWidgets)
        {
            if (obj != null)
            {
                obj.SetActive(false);
                Destroy(obj);
            }
        }
        ingredientWidgets.Clear();

        if (ingredients == null || ingredientContent == null) return;

        foreach (var ing in ingredients)
        {
            if (ing == null || ing.Item == null) continue;
            GameObject row = CreateIngredientRow(ingredientContent, ing);
            ingredientWidgets.Add(row);
        }
    }

    private RecipeCardWidget CreateRecipeCard(RectTransform parent, CraftingRecipe recipe)
    {
        RectTransform cardRT = WildRootUITheme.CreatePanel(parent, "Recipe Card", WildRootUITheme.CardAnimal);
        Button button = cardRT.gameObject.AddComponent<Button>();
        button.targetGraphic = cardRT.GetComponent<UnityEngine.UI.Image>();

        ColorBlock cb = button.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
        cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        cb.selectedColor = Color.white;
        cb.disabledColor = new Color(0.6f, 0.6f, 0.6f, 0.7f);
        button.colors = cb;

        LayoutElement le = cardRT.gameObject.AddComponent<LayoutElement>();
        le.minHeight = le.preferredHeight = 68;

        // Icon Box (Left)
        RectTransform iconBox = WildRootUITheme.CreatePanel(cardRT, "Icon Box", WildRootUITheme.SlotNormal);
        iconBox.anchorMin = new Vector2(0f, 0.5f);
        iconBox.anchorMax = new Vector2(0f, 0.5f);
        iconBox.pivot = new Vector2(0f, 0.5f);
        iconBox.anchoredPosition = new Vector2(10, 0);
        iconBox.sizeDelta = new Vector2(48, 48);

        GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        iconGO.transform.SetParent(iconBox, false);
        RectTransform iconRT = iconGO.GetComponent<RectTransform>();
        WildRootUITheme.FitStretch(iconRT, 4, 4, 4, 4);
        UnityEngine.UI.Image iconImg = iconGO.GetComponent<UnityEngine.UI.Image>();
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;

        // Text Info Container (Center & Right)
        RectTransform infoRT = new GameObject("Info", typeof(RectTransform)).GetComponent<RectTransform>();
        infoRT.SetParent(cardRT, false);
        infoRT.anchorMin = Vector2.zero;
        infoRT.anchorMax = Vector2.one;
        infoRT.offsetMin = new Vector2(68, 6);
        infoRT.offsetMax = new Vector2(-10, -6);

        TMP_Text nameTxt = WildRootUITheme.CreateText(infoRT, "Name", "", 15f, FontStyles.Bold, TextAlignmentOptions.TopLeft, WildRootUITheme.ColorTextTitle);
        nameTxt.rectTransform.anchorMin = new Vector2(0f, 0.45f);
        nameTxt.rectTransform.anchorMax = new Vector2(0.65f, 1f);
        nameTxt.rectTransform.offsetMin = Vector2.zero;
        nameTxt.rectTransform.offsetMax = Vector2.zero;

        TMP_Text qtyTxt = WildRootUITheme.CreateText(infoRT, "Qty", "", 12f, FontStyles.Normal, TextAlignmentOptions.BottomLeft, WildRootUITheme.ColorGoldMuted);
        qtyTxt.rectTransform.anchorMin = new Vector2(0f, 0f);
        qtyTxt.rectTransform.anchorMax = new Vector2(0.65f, 0.48f);
        qtyTxt.rectTransform.offsetMin = Vector2.zero;
        qtyTxt.rectTransform.offsetMax = Vector2.zero;

        TMP_Text statusTxt = WildRootUITheme.CreateText(infoRT, "Status", "", 12f, FontStyles.Bold, TextAlignmentOptions.MidlineRight, WildRootUITheme.ColorTextGreen);
        statusTxt.rectTransform.anchorMin = new Vector2(0.66f, 0f);
        statusTxt.rectTransform.anchorMax = new Vector2(1f, 1f);
        statusTxt.rectTransform.offsetMin = Vector2.zero;
        statusTxt.rectTransform.offsetMax = Vector2.zero;

        Outline outline = cardRT.gameObject.AddComponent<Outline>();
        outline.effectColor = WildRootUITheme.ColorGoldAmber;
        outline.effectDistance = new Vector2(2f, -2f);
        outline.enabled = false;

        button.onClick.AddListener(() => State.Select(recipe));

        return new RecipeCardWidget(cardRT.gameObject, cardRT.GetComponent<UnityEngine.UI.Image>(), outline, iconImg, nameTxt, qtyTxt, statusTxt);
    }

    private GameObject CreateIngredientRow(RectTransform parent, HandCraftingIngredientState ing)
    {
        RectTransform rowRT = WildRootUITheme.CreatePanel(parent, "Ingredient Row", WildRootUITheme.CardAnimal);
        LayoutElement le = rowRT.gameObject.AddComponent<LayoutElement>();
        le.minHeight = le.preferredHeight = 44;

        // Icon Box
        RectTransform iconBox = WildRootUITheme.CreatePanel(rowRT, "Icon Box", WildRootUITheme.SlotNormal);
        iconBox.anchorMin = new Vector2(0f, 0.5f);
        iconBox.anchorMax = new Vector2(0f, 0.5f);
        iconBox.pivot = new Vector2(0f, 0.5f);
        iconBox.anchoredPosition = new Vector2(6, 0);
        iconBox.sizeDelta = new Vector2(34, 34);

        GameObject iconGO = new GameObject("Icon", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        iconGO.transform.SetParent(iconBox, false);
        RectTransform iconRT = iconGO.GetComponent<RectTransform>();
        WildRootUITheme.FitStretch(iconRT, 3, 3, 3, 3);
        UnityEngine.UI.Image iconImg = iconGO.GetComponent<UnityEngine.UI.Image>();
        iconImg.sprite = ing.Item.icon;
        iconImg.preserveAspect = true;
        iconImg.raycastTarget = false;

        bool sufficient = ing.Owned >= ing.Required;

        // Name Text
        TMP_Text nameTxt = WildRootUITheme.CreateText(rowRT, "Name", ing.Item.itemName, 14f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, WildRootUITheme.ColorTextBody);
        nameTxt.rectTransform.anchorMin = new Vector2(0f, 0f);
        nameTxt.rectTransform.anchorMax = new Vector2(0.55f, 1f);
        nameTxt.rectTransform.offsetMin = new Vector2(48, 0);
        nameTxt.rectTransform.offsetMax = Vector2.zero;

        // Count / Status Text
        string countColor = sufficient ? "#88C888" : "#E68369";
        string statusLabel = sufficient ? "<size=11><color=#A8C59A>✓ Ready</color></size>" : "<size=11><color=#E68369>✗ Needed</color></size>";
        string countString = $"<color={countColor}><b>{ing.Owned} / {ing.Required}</b></color>  {statusLabel}";

        TMP_Text countTxt = WildRootUITheme.CreateText(rowRT, "Count", countString, 14f, FontStyles.Normal, TextAlignmentOptions.MidlineRight, WildRootUITheme.ColorTextBody);
        countTxt.rectTransform.anchorMin = new Vector2(0.56f, 0f);
        countTxt.rectTransform.anchorMax = new Vector2(1f, 1f);
        countTxt.rectTransform.offsetMin = Vector2.zero;
        countTxt.rectTransform.offsetMax = new Vector2(-12, 0);

        return rowRT.gameObject;
    }

    public static GameObject CreatePage(Transform parent)
    {
        // Root Crafting Page
        RectTransform page = WildRootUITheme.CreatePanel(parent, "Crafting Page", WildRootUITheme.PanelFrame);
        page.anchorMin = Vector2.zero;
        page.anchorMax = Vector2.one;
        page.offsetMin = new Vector2(20, 20);
        page.offsetMax = new Vector2(-20, -74);

        HandCraftingMenuUI ui = page.gameObject.AddComponent<HandCraftingMenuUI>();

        // ========================
        // 1. LEFT: RECIPE CATALOG
        // ========================
        RectTransform leftPanel = WildRootUITheme.CreatePanel(page, "Catalog Region", WildRootUITheme.PanelInner);
        leftPanel.anchorMin = new Vector2(0f, 0f);
        leftPanel.anchorMax = new Vector2(0.38f, 1f);
        leftPanel.offsetMin = Vector2.zero;
        leftPanel.offsetMax = Vector2.zero;

        WildRootUITheme.CreateHeaderRibbon(leftPanel, "Catalog Header", "HAND CRAFTING", "Recipe Catalog");
        RectTransform catHeader = leftPanel.Find("Catalog Header") as RectTransform;
        if (catHeader != null)
        {
            catHeader.anchorMin = new Vector2(0.04f, 1f);
            catHeader.anchorMax = new Vector2(0.96f, 1f);
            catHeader.pivot = new Vector2(0.5f, 1f);
            catHeader.anchoredPosition = new Vector2(0, -12);
            catHeader.sizeDelta = new Vector2(0, 48);
        }

        // Scroll View
        RectTransform viewport = WildRootUITheme.CreatePanel(leftPanel, "Recipe Viewport", WildRootUITheme.BadgeDark);
        viewport.anchorMin = new Vector2(0.04f, 0.04f);
        viewport.anchorMax = new Vector2(0.96f, 0.88f);
        viewport.offsetMin = Vector2.zero;
        viewport.offsetMax = Vector2.zero;
        viewport.gameObject.AddComponent<RectMask2D>();
        ScrollRect scroll = viewport.gameObject.AddComponent<ScrollRect>();

        RectTransform content = new GameObject("Recipe Content", typeof(RectTransform)).GetComponent<RectTransform>();
        content.SetParent(viewport, false);
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = Vector2.one;
        content.pivot = new Vector2(0.5f, 1);
        content.sizeDelta = Vector2.zero;

        VerticalLayoutGroup vlg = content.gameObject.AddComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(6, 6, 6, 6);
        vlg.spacing = 8;
        vlg.childControlHeight = true;
        vlg.childControlWidth = true;
        vlg.childForceExpandHeight = false;
        vlg.childForceExpandWidth = true;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 25;

        // Catalog Empty State Card
        RectTransform catEmpty = WildRootUITheme.CreatePanel(leftPanel, "Catalog Empty Card", WildRootUITheme.BadgeDark);
        catEmpty.anchorMin = new Vector2(0.08f, 0.28f);
        catEmpty.anchorMax = new Vector2(0.92f, 0.72f);
        catEmpty.offsetMin = Vector2.zero;
        catEmpty.offsetMax = Vector2.zero;
        TMP_Text catEmptyText = WildRootUITheme.CreateText(catEmpty, "Empty Label",
            "<b><size=16><color=#FFF3D6>No Crafting Recipes</color></size></b>\n\n" +
            "<size=13><color=#A39482>Hand-crafting blueprints will appear here once discovered or unlocked.</color></size>",
            14f, FontStyles.Normal, TextAlignmentOptions.Center, WildRootUITheme.ColorTextSubtitle);
        WildRootUITheme.FitStretch(catEmptyText.rectTransform, 14, 14, 14, 14);

        // ========================
        // 2. RIGHT: RECIPE DETAILS
        // ========================
        RectTransform rightPanel = WildRootUITheme.CreatePanel(page, "Details Region", WildRootUITheme.PanelInner);
        rightPanel.anchorMin = new Vector2(0.395f, 0f);
        rightPanel.anchorMax = new Vector2(1f, 1f);
        rightPanel.offsetMin = Vector2.zero;
        rightPanel.offsetMax = Vector2.zero;

        WildRootUITheme.CreateHeaderRibbon(rightPanel, "Details Header", "RECIPE DETAILS", "Assembly & Materials");
        RectTransform detHeader = rightPanel.Find("Details Header") as RectTransform;
        if (detHeader != null)
        {
            detHeader.anchorMin = new Vector2(0.03f, 1f);
            detHeader.anchorMax = new Vector2(0.97f, 1f);
            detHeader.pivot = new Vector2(0.5f, 1f);
            detHeader.anchoredPosition = new Vector2(0, -12);
            detHeader.sizeDelta = new Vector2(0, 48);
        }

        // Details Empty State Card
        RectTransform detEmpty = WildRootUITheme.CreatePanel(rightPanel, "Details Empty Card", WildRootUITheme.BadgeDark);
        detEmpty.anchorMin = new Vector2(0.06f, 0.28f);
        detEmpty.anchorMax = new Vector2(0.94f, 0.72f);
        detEmpty.offsetMin = Vector2.zero;
        detEmpty.offsetMax = Vector2.zero;
        TMP_Text detEmptyText = WildRootUITheme.CreateText(detEmpty, "Empty Label",
            "<b><size=18><color=#FFF3D6>Select a Blueprint</color></size></b>\n\n" +
            "<size=14><color=#D4C2A3>Choose a recipe from the catalog on the left to view required materials and craft items.</color></size>",
            15f, FontStyles.Normal, TextAlignmentOptions.Center, WildRootUITheme.ColorTextSubtitle);
        WildRootUITheme.FitStretch(detEmptyText.rectTransform, 20, 20, 20, 20);

        // Details Active Container
        RectTransform detContainer = new GameObject("Details Container", typeof(RectTransform)).GetComponent<RectTransform>();
        detContainer.SetParent(rightPanel, false);
        detContainer.anchorMin = Vector2.zero;
        detContainer.anchorMax = Vector2.one;
        detContainer.offsetMin = new Vector2(16, 16);
        detContainer.offsetMax = new Vector2(-16, -68);

        // 2.1 Output Preview Box
        RectTransform outBox = WildRootUITheme.CreatePanel(detContainer, "Output Preview Box", WildRootUITheme.BadgeDark);
        outBox.anchorMin = new Vector2(0f, 0.74f);
        outBox.anchorMax = new Vector2(1f, 1f);
        outBox.offsetMin = Vector2.zero;
        outBox.offsetMax = Vector2.zero;

        RectTransform outIconBox = WildRootUITheme.CreatePanel(outBox, "Icon Box", WildRootUITheme.SlotHotbar);
        outIconBox.anchorMin = new Vector2(0f, 0.5f);
        outIconBox.anchorMax = new Vector2(0f, 0.5f);
        outIconBox.pivot = new Vector2(0f, 0.5f);
        outIconBox.anchoredPosition = new Vector2(16, 0);
        outIconBox.sizeDelta = new Vector2(68, 68);

        GameObject outIconGO = new GameObject("Result Icon", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        outIconGO.transform.SetParent(outIconBox, false);
        RectTransform outIconRT = outIconGO.GetComponent<RectTransform>();
        WildRootUITheme.FitStretch(outIconRT, 7, 7, 7, 7);
        UnityEngine.UI.Image outIconImg = outIconGO.GetComponent<UnityEngine.UI.Image>();
        outIconImg.preserveAspect = true;
        outIconImg.raycastTarget = false;

        TMP_Text outName = WildRootUITheme.CreateText(outBox, "Result Name", "Recipe Output", 20f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, WildRootUITheme.ColorTextGold);
        outName.rectTransform.anchorMin = new Vector2(0f, 0.48f);
        outName.rectTransform.anchorMax = new Vector2(1f, 1f);
        outName.rectTransform.offsetMin = new Vector2(98, 0);
        outName.rectTransform.offsetMax = new Vector2(-12, 0);

        TMP_Text outQty = WildRootUITheme.CreateText(outBox, "Result Quantity", "Batch Output: 1 × Item", 14f, FontStyles.Normal, TextAlignmentOptions.MidlineLeft, WildRootUITheme.ColorTextGreen);
        outQty.rectTransform.anchorMin = new Vector2(0f, 0f);
        outQty.rectTransform.anchorMax = new Vector2(1f, 0.52f);
        outQty.rectTransform.offsetMin = new Vector2(98, 0);
        outQty.rectTransform.offsetMax = new Vector2(-12, 0);

        // 2.2 Required Ingredients Box
        RectTransform ingBox = WildRootUITheme.CreatePanel(detContainer, "Required Ingredients Box", WildRootUITheme.BadgeDark);
        ingBox.anchorMin = new Vector2(0f, 0.23f);
        ingBox.anchorMax = new Vector2(1f, 0.72f);
        ingBox.offsetMin = Vector2.zero;
        ingBox.offsetMax = Vector2.zero;

        TMP_Text ingTitle = WildRootUITheme.CreateText(ingBox, "Section Title", "<b>REQUIRED INGREDIENTS  (OWNED / NEEDED)</b>", 13f, FontStyles.Bold, TextAlignmentOptions.MidlineLeft, WildRootUITheme.ColorTextSubtitle);
        ingTitle.rectTransform.anchorMin = new Vector2(0f, 1f);
        ingTitle.rectTransform.anchorMax = new Vector2(1f, 1f);
        ingTitle.rectTransform.pivot = new Vector2(0f, 1f);
        ingTitle.rectTransform.anchoredPosition = new Vector2(14, -8);
        ingTitle.rectTransform.sizeDelta = new Vector2(-28, 24);

        RectTransform ingViewport = WildRootUITheme.CreatePanel(ingBox, "Ingredients Viewport", WildRootUITheme.BadgeDark);
        ingViewport.anchorMin = new Vector2(0.02f, 0.04f);
        ingViewport.anchorMax = new Vector2(0.98f, 0.84f);
        ingViewport.offsetMin = Vector2.zero;
        ingViewport.offsetMax = Vector2.zero;
        ingViewport.gameObject.AddComponent<RectMask2D>();
        ScrollRect ingScroll = ingViewport.gameObject.AddComponent<ScrollRect>();

        RectTransform ingContent = new GameObject("Ingredient Content", typeof(RectTransform)).GetComponent<RectTransform>();
        ingContent.SetParent(ingViewport, false);
        ingContent.anchorMin = new Vector2(0, 1);
        ingContent.anchorMax = Vector2.one;
        ingContent.pivot = new Vector2(0.5f, 1);
        ingContent.sizeDelta = Vector2.zero;

        VerticalLayoutGroup ingVlg = ingContent.gameObject.AddComponent<VerticalLayoutGroup>();
        ingVlg.padding = new RectOffset(4, 4, 4, 4);
        ingVlg.spacing = 6;
        ingVlg.childControlHeight = true;
        ingVlg.childControlWidth = true;
        ingVlg.childForceExpandHeight = false;
        ingVlg.childForceExpandWidth = true;
        ingContent.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        ingScroll.viewport = ingViewport;
        ingScroll.content = ingContent;
        ingScroll.horizontal = false;
        ingScroll.vertical = true;
        ingScroll.movementType = ScrollRect.MovementType.Clamped;
        ingScroll.scrollSensitivity = 25;

        // 2.3 Crafting Action Box
        RectTransform actionBox = WildRootUITheme.CreatePanel(detContainer, "Crafting Action Box", WildRootUITheme.BadgeDark);
        actionBox.anchorMin = new Vector2(0f, 0f);
        actionBox.anchorMax = new Vector2(1f, 0.21f);
        actionBox.offsetMin = Vector2.zero;
        actionBox.offsetMax = Vector2.zero;

        TMP_Text hintText = WildRootUITheme.CreateText(actionBox, "Status Hint", "", 13f, FontStyles.Normal, TextAlignmentOptions.Center, WildRootUITheme.ColorTextSubtitle);
        hintText.rectTransform.anchorMin = new Vector2(0f, 1f);
        hintText.rectTransform.anchorMax = new Vector2(1f, 1f);
        hintText.rectTransform.pivot = new Vector2(0.5f, 1f);
        hintText.rectTransform.anchoredPosition = new Vector2(0, -6);
        hintText.rectTransform.sizeDelta = new Vector2(-20, 24);

        Button craftBtn = WildRootUITheme.CreateButton(actionBox, "Craft Button", "⚒ CRAFT ITEM", 17f, WildRootUITheme.ButtonNormal, WildRootUITheme.ButtonHover);
        RectTransform cbRT = (RectTransform)craftBtn.transform;
        cbRT.anchorMin = new Vector2(0.5f, 0f);
        cbRT.anchorMax = new Vector2(0.5f, 0f);
        cbRT.pivot = new Vector2(0.5f, 0f);
        cbRT.anchoredPosition = new Vector2(0, 8);
        cbRT.sizeDelta = new Vector2(260, 46);

        // Assign serialized fields on UI component
        ui.recipeContent = content;
        ui.recipeScrollRect = scroll;
        ui.catalogEmptyState = catEmpty.gameObject;

        ui.detailsRoot = detContainer.gameObject;
        ui.detailsEmptyState = detEmpty.gameObject;
        ui.detailsEmptyText = detEmptyText;

        ui.resultIcon = outIconImg;
        ui.resultNameText = outName;
        ui.resultQuantityText = outQty;

        ui.ingredientContent = ingContent;
        ui.ingredientScrollRect = ingScroll;

        ui.craftButton = craftBtn;
        ui.statusHintText = hintText;

        craftBtn.onClick.AddListener(ui.Craft);
        detContainer.gameObject.SetActive(false);
        page.gameObject.SetActive(false);

        return page.gameObject;
    }

    private void EnsureWidgets()
    {
        if (craftButton != null)
        {
            craftButton.onClick.RemoveListener(Craft);
            craftButton.onClick.AddListener(Craft);
        }
    }

    private class RecipeCardWidget
    {
        public GameObject Root { get; }
        private readonly UnityEngine.UI.Image bgImage;
        private readonly Outline outline;
        private readonly UnityEngine.UI.Image icon;
        private readonly TMP_Text nameText;
        private readonly TMP_Text qtyText;
        private readonly TMP_Text statusText;

        public RecipeCardWidget(GameObject root, UnityEngine.UI.Image bgImage, Outline outline,
            UnityEngine.UI.Image icon, TMP_Text nameText, TMP_Text qtyText, TMP_Text statusText)
        {
            Root = root;
            this.bgImage = bgImage;
            this.outline = outline;
            this.icon = icon;
            this.nameText = nameText;
            this.qtyText = qtyText;
            this.statusText = statusText;
        }

        public void SetState(CraftingRecipe recipe, bool canCraft, bool isSelected)
        {
            ItemData output = recipe.outputItem;
            string itemName = output != null ? output.itemName : "Unknown";

            if (icon != null)
            {
                icon.sprite = output != null ? output.icon : null;
                icon.enabled = icon.sprite != null;
            }
            if (nameText != null)
            {
                nameText.text = $"<b>{(isSelected ? "<color=#FFE8A3>" : "<color=#FFF3D6>")}{itemName}</color></b>";
            }
            if (qtyText != null)
            {
                qtyText.text = recipe.outputAmount > 1
                    ? $"<color=#F4C453>Produces ×{recipe.outputAmount}</color>"
                    : "<color=#A39482>Single Item</color>";
            }
            if (statusText != null)
            {
                statusText.text = canCraft
                    ? "<color=#88C888>✦ Ready</color>"
                    : "<color=#A39482>Missing items</color>";
            }
            if (outline != null)
            {
                outline.enabled = isSelected;
            }
            if (bgImage != null)
            {
                bgImage.sprite = isSelected ? WildRootUITheme.CardAnimalHover : WildRootUITheme.CardAnimal;
            }
        }
    }
}

