using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class WildRootUITheme
{
    // Color Palette - Warm, rustic, adventure & nature tones
    public static readonly Color ColorWoodDark = new Color(0.14f, 0.09f, 0.06f, 0.98f);
    public static readonly Color ColorWoodMedium = new Color(0.24f, 0.17f, 0.11f, 1.0f);
    public static readonly Color ColorWoodLight = new Color(0.38f, 0.28f, 0.19f, 1.0f);
    public static readonly Color ColorParchmentDark = new Color(0.18f, 0.13f, 0.09f, 0.96f);
    public static readonly Color ColorParchmentInner = new Color(0.13f, 0.09f, 0.06f, 0.95f);
    
    // Accents
    public static readonly Color ColorNatureGreen = new Color(0.24f, 0.42f, 0.25f, 1.0f);
    public static readonly Color ColorNatureGreenLight = new Color(0.45f, 0.68f, 0.42f, 1.0f);
    public static readonly Color ColorNatureGreenMuted = new Color(0.32f, 0.48f, 0.30f, 0.9f);
    public static readonly Color ColorGoldAmber = new Color(0.95f, 0.75f, 0.22f, 1.0f);
    public static readonly Color ColorGoldLight = new Color(1.00f, 0.90f, 0.55f, 1.0f);
    public static readonly Color ColorGoldMuted = new Color(0.80f, 0.62f, 0.25f, 1.0f);

    // Typography Colors
    public static readonly Color ColorTextTitle = new Color(1.00f, 0.95f, 0.84f, 1.0f);     // Soft cream ivory
    public static readonly Color ColorTextSubtitle = new Color(0.85f, 0.78f, 0.66f, 1.0f);  // Warm tan
    public static readonly Color ColorTextBody = new Color(0.96f, 0.92f, 0.85f, 1.0f);      // Light parchment
    public static readonly Color ColorTextMuted = new Color(0.68f, 0.62f, 0.52f, 1.0f);     // Muted earthy grey
    public static readonly Color ColorTextGold = new Color(1.00f, 0.88f, 0.45f, 1.0f);      // Bright gold
    public static readonly Color ColorTextGreen = new Color(0.60f, 0.85f, 0.55f, 1.0f);     // Soft leaf green
    public static readonly Color ColorTextWarning = new Color(0.95f, 0.55f, 0.40f, 1.0f);   // Coral orange

    // Sprite Caching
    private static readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    public static Sprite GetSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName)) return null;

        if (spriteCache.TryGetValue(spriteName, out Sprite cached) && cached != null)
            return cached;

        Sprite loaded = Resources.Load<Sprite>("UI/WildRoot/" + spriteName);
#if UNITY_EDITOR
        if (loaded == null)
        {
            string path = "Assets/Art/UI/WildRoot/" + spriteName + ".png";
            loaded = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }
#endif
        if (loaded != null)
            spriteCache[spriteName] = loaded;

        return loaded;
    }

    // Sprite Getters
    public static Sprite PanelFrame => GetSprite("Panel_Frame");
    public static Sprite PanelInner => GetSprite("Panel_Inner");
    public static Sprite SlotNormal => GetSprite("Slot_Normal");
    public static Sprite SlotHotbar => GetSprite("Slot_Hotbar");
    public static Sprite SlotSelected => GetSprite("Slot_Selected");
    public static Sprite SlotHover => GetSprite("Slot_Hover");
    public static Sprite HeaderRibbon => GetSprite("Header_Ribbon");
    public static Sprite TabActive => GetSprite("Tab_Active");
    public static Sprite TabInactive => GetSprite("Tab_Inactive");
    public static Sprite ButtonNormal => GetSprite("Button_Normal");
    public static Sprite ButtonHover => GetSprite("Button_Hover");
    public static Sprite ButtonClose => GetSprite("Button_Close");
    public static Sprite CardAnimal => GetSprite("Card_Animal");
    public static Sprite CardAnimalHover => GetSprite("Card_Animal_Hover");
    public static Sprite BadgeDark => GetSprite("Badge_Dark");
    public static Sprite SolidWhite => GetSprite("Solid_White");
    public static Sprite HotbarTray => GetSprite("Hotbar_Tray");
    public static Sprite ScrollbarBG => GetSprite("Scrollbar_BG");
    public static Sprite ScrollbarHandle => GetSprite("Scrollbar_Handle");
    public static Sprite Divider => GetSprite("Divider_Wood");

    // UI Construction Helpers
    public static RectTransform CreatePanel(Transform parent, string name, Sprite sprite = null, Color? color = null)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);

        UnityEngine.UI.Image img = go.GetComponent<UnityEngine.UI.Image>();
        img.sprite = sprite != null ? sprite : PanelFrame;
        img.type = UnityEngine.UI.Image.Type.Sliced;
        img.color = color ?? Color.white;
        img.raycastTarget = true;

        return rt;
    }

    public static TMP_Text CreateText(Transform parent, string name, string text, float fontSize = 18f,
        FontStyles fontStyle = FontStyles.Normal, TextAlignmentOptions alignment = TextAlignmentOptions.Center,
        Color? color = null)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        TMP_Text tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.font = TMP_Settings.defaultFontAsset;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.fontStyle = fontStyle;
        tmp.alignment = alignment;
        tmp.color = color ?? ColorTextBody;
        tmp.raycastTarget = false;
        tmp.richText = true;

        return tmp;
    }

    public static Button CreateButton(Transform parent, string name, string caption, float fontSize = 18f,
        Sprite normalSprite = null, Sprite hoverSprite = null)
    {
        RectTransform rt = CreatePanel(parent, name, normalSprite != null ? normalSprite : ButtonNormal);
        Button btn = rt.gameObject.AddComponent<Button>();
        btn.targetGraphic = rt.GetComponent<UnityEngine.UI.Image>();

        ColorBlock colors = btn.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.1f, 1.1f, 1.1f, 1f);
        colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
        btn.colors = colors;

        TMP_Text label = CreateText(rt.transform, "Label", caption, fontSize, FontStyles.Bold, TextAlignmentOptions.Center, ColorTextTitle);
        FitStretch(label.rectTransform);

        return btn;
    }

    public static RectTransform CreateHeaderRibbon(Transform parent, string name, string title, string subtitle = null)
    {
        RectTransform ribbon = CreatePanel(parent, name, HeaderRibbon);
        
        if (string.IsNullOrEmpty(subtitle))
        {
            TMP_Text titleText = CreateText(ribbon, "Title", title, 20f, FontStyles.Bold, TextAlignmentOptions.Center, ColorTextTitle);
            FitStretch(titleText.rectTransform, 8, 4, 8, 4);
        }
        else
        {
            TMP_Text titleText = CreateText(ribbon, "Title", title, 18f, FontStyles.Bold, TextAlignmentOptions.Center, ColorTextTitle);
            titleText.rectTransform.anchorMin = new Vector2(0f, 0.42f);
            titleText.rectTransform.anchorMax = new Vector2(1f, 1f);
            titleText.rectTransform.offsetMin = new Vector2(8, 0);
            titleText.rectTransform.offsetMax = new Vector2(-8, -2);

            TMP_Text subText = CreateText(ribbon, "Subtitle", subtitle, 13f, FontStyles.Italic, TextAlignmentOptions.Center, ColorTextSubtitle);
            subText.rectTransform.anchorMin = new Vector2(0f, 0f);
            subText.rectTransform.anchorMax = new Vector2(1f, 0.45f);
            subText.rectTransform.offsetMin = new Vector2(8, 2);
            subText.rectTransform.offsetMax = new Vector2(-8, 0);
        }

        return ribbon;
    }

    // Durability Bar Palette & Logic
    public static readonly Color ColorDurabilityGreen = new Color(0.30f, 0.85f, 0.30f, 1f);
    public static readonly Color ColorDurabilityYellowOrange = new Color(0.95f, 0.75f, 0.18f, 1f);
    public static readonly Color ColorDurabilityRed = new Color(0.92f, 0.22f, 0.18f, 1f);
    public static readonly Color ColorDurabilityBg = new Color(0.08f, 0.05f, 0.03f, 0.92f);

    public static bool ShouldShowDurability(InventorySlot slot, out float ratio)
    {
        ratio = 1f;
        if (slot == null || slot.IsEmpty || !(slot.item is ToolItem))
            return false;

        ToolInstance instance = slot.ToolState;
        if (instance == null || instance.Unbreakable)
            return false;

        int max = instance.MaximumDurability;
        if (max <= 0)
            return false;

        int current = instance.CurrentDurability;
        if (current >= max || current <= 0)
            return false;

        ratio = Mathf.Clamp01((float)current / max);
        return true;
    }

    public static Color GetDurabilityColor(float ratio)
    {
        if (ratio >= 0.5f)
        {
            float t = (ratio - 0.5f) / 0.5f;
            return Color.Lerp(ColorDurabilityYellowOrange, ColorDurabilityGreen, t);
        }
        else
        {
            float t = ratio / 0.5f;
            return Color.Lerp(ColorDurabilityRed, ColorDurabilityYellowOrange, t);
        }
    }

    public static void UpdateDurabilityBar(GameObject barRoot, UnityEngine.UI.Image fillImage, InventorySlot slot)
    {
        if (barRoot == null) return;

        if (ShouldShowDurability(slot, out float ratio))
        {
            barRoot.SetActive(true);
            if (fillImage != null)
            {
                if (fillImage.sprite == null) fillImage.sprite = SolidWhite;
                fillImage.fillAmount = ratio;
                fillImage.color = GetDurabilityColor(ratio);
            }
        }
        else
        {
            barRoot.SetActive(false);
        }
    }

    public static GameObject CreateDurabilityBar(Transform slotTransform, Vector2 size, Vector2 anchoredPos)
    {
        Transform existing = slotTransform.Find("DurabilityBar");
        if (existing != null)
            return existing.gameObject;

        GameObject barGO = new GameObject("DurabilityBar", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform barRT = barGO.GetComponent<RectTransform>();
        barRT.SetParent(slotTransform, false);
        barRT.anchorMin = new Vector2(0.5f, 0f);
        barRT.anchorMax = new Vector2(0.5f, 0f);
        barRT.pivot = new Vector2(0.5f, 0f);
        barRT.anchoredPosition = anchoredPos;
        barRT.sizeDelta = size;

        UnityEngine.UI.Image barBg = barGO.GetComponent<UnityEngine.UI.Image>();
        barBg.sprite = BadgeDark;
        barBg.type = UnityEngine.UI.Image.Type.Sliced;
        barBg.color = ColorDurabilityBg;
        barBg.raycastTarget = false;

        GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        RectTransform fillRT = fillGO.GetComponent<RectTransform>();
        fillRT.SetParent(barRT, false);
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = new Vector2(1f, 1f);
        fillRT.offsetMax = new Vector2(-1f, -1f);

        UnityEngine.UI.Image fillImg = fillGO.GetComponent<UnityEngine.UI.Image>();
        fillImg.sprite = SolidWhite;
        fillImg.type = UnityEngine.UI.Image.Type.Filled;
        fillImg.fillMethod = UnityEngine.UI.Image.FillMethod.Horizontal;
        fillImg.fillOrigin = (int)UnityEngine.UI.Image.OriginHorizontal.Left;
        fillImg.fillAmount = 1f;
        fillImg.color = ColorDurabilityGreen;
        fillImg.raycastTarget = false;

        barGO.SetActive(false);
        return barGO;
    }

    public static void FitStretch(RectTransform rt, float left = 0, float bottom = 0, float right = 0, float top = 0)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
        rt.localScale = Vector3.one;
    }

    public static void FitAnchors(RectTransform rt, Vector2 min, Vector2 max, float left = 0, float bottom = 0, float right = 0, float top = 0)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = new Vector2(left, bottom);
        rt.offsetMax = new Vector2(-right, -top);
        rt.localScale = Vector3.one;
    }
}
