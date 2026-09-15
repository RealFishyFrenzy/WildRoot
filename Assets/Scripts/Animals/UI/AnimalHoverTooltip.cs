using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Concise, non-interactive tooltip styled to match WildRoot adventure UI.
public class AnimalHoverTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private AnimalInstance animal;
    private GameObject tooltip;
    private TMP_Text text;
    public void Bind(AnimalInstance record) => animal = record;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (animal == null) return;
        if (tooltip == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            RectTransform rect = WildRootUITheme.CreatePanel(canvas.transform, "Animal Tooltip (Runtime)", WildRootUITheme.PanelFrame);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0f, 0.5f);
            rect.sizeDelta = new Vector2(280, 130);

            tooltip = rect.gameObject;
            tooltip.AddComponent<CanvasGroup>().blocksRaycasts = false;

            text = WildRootUITheme.CreateText(rect, "Animal Info Text", "", 13f, FontStyles.Normal, TextAlignmentOptions.Left, WildRootUITheme.ColorTextBody);
            WildRootUITheme.FitStretch(text.rectTransform, 14, 10, 14, 10);
        }

        string animalName = string.IsNullOrWhiteSpace(animal.animalName) ? "Unnamed" : animal.animalName;
        string maxFood = animal.animalData != null ? AnimalPresentation.Number(animal.animalData.baseMaxHunger) : "?";
        string maxHealth = animal.animalData != null ? AnimalPresentation.Number(animal.animalData.baseMaxHealth) : "?";

        text.text = $"<b><size=16><color=#FFF3D6>{animalName}</color></size></b>\n" +
                    $"<color=#88C888>{animal.SpeciesName} ({animal.sex})</color>\n" +
                    $"<color=#D4C2A3>Food:</color> {AnimalPresentation.Number(animal.foodLevel)} / {maxFood}\n" +
                    $"<color=#D4C2A3>Health:</color> {AnimalPresentation.Number(animal.health)} / {maxHealth}";

        // Position tooltip near mouse pointer
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltip.transform.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint))
        {
            RectTransform rt = tooltip.transform as RectTransform;
            rt.anchoredPosition = localPoint + new Vector2(25, -20);
        }

        tooltip.SetActive(true);
        tooltip.transform.SetAsLastSibling();
    }

    public void OnPointerExit(PointerEventData eventData) => Hide();
    public void Hide() { if (tooltip != null) tooltip.SetActive(false); }
    private void OnDisable() => Hide();
    private void OnDestroy() { if (tooltip != null) Destroy(tooltip); }
}

