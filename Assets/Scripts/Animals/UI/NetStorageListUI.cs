using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A view of NetUI's existing storage. Never copies or mutates animal records.
public class NetStorageListUI : MonoBehaviour
{
    [SerializeField] private RectTransform content;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button animalRowTemplate;
    private NetUI net;
    private AnimalInventory storage;
    private bool subscribed;
    private bool placementView;
    private readonly List<Button> rows = new List<Button>();

    public void Configure(RectTransform listContent, TMP_Text status)
    {
        content = listContent;
        statusText = status;
    }

    public void Bind(NetUI source, bool allowPlacement = false)
    {
        Unsubscribe();
        net = source;
        placementView = allowPlacement;
        storage = net != null ? net.Storage : null;
        Subscribe();
        Refresh();
    }

    private void OnEnable() { Subscribe(); Refresh(); }
    private void OnDisable() => Unsubscribe();
    private void Subscribe()
    {
        if (!subscribed && isActiveAndEnabled && storage != null)
        {
            storage.Changed += Refresh;
            subscribed = true;
        }
    }
    private void Unsubscribe()
    {
        if (subscribed && storage != null) storage.Changed -= Refresh;
        subscribed = false;
    }

    private void Refresh()
    {
        foreach (Button row in rows)
            if (row != null) { row.gameObject.SetActive(false); Destroy(row.gameObject); }
        rows.Clear();
        if (storage != null && content != null)
            foreach (AnimalInstance animal in storage.Animals)
            {
                if (animal == null) continue;
                string animalName = string.IsNullOrWhiteSpace(animal.animalName) ? "Unnamed" : animal.animalName;
                string species = animal.SpeciesName;
                string maxFood = animal.animalData != null ? AnimalPresentation.Number(animal.animalData.baseMaxHunger) : "?";
                string caption = $"<b><size=15><color=#FFF3D6>{animalName}</color></size></b>\n" +
                                 $"<size=12><color=#88C888>{species} ({animal.sex})</color></size>\n" +
                                 $"<size=11><color=#D4C2A3>Food: {AnimalPresentation.Number(animal.foodLevel)}/{maxFood}</color></size>";

                Button row;
                if (animalRowTemplate != null)
                {
                    row = Instantiate(animalRowTemplate, content);
                    TMP_Text label = row.GetComponentInChildren<TMP_Text>(true);
                    if (label != null) { label.richText = true; label.text = caption; }
                    row.gameObject.SetActive(true);
                }
                else
                {
                    RectTransform cardRect = WildRootUITheme.CreatePanel(content, "Stored Animal", WildRootUITheme.CardAnimal);
                    row = cardRect.gameObject.AddComponent<Button>();
                    row.targetGraphic = cardRect.GetComponent<UnityEngine.UI.Image>();

                    ColorBlock cb = row.colors;
                    cb.normalColor = Color.white;
                    cb.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
                    cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
                    cb.selectedColor = Color.white;
                    cb.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.6f);
                    row.colors = cb;

                    TMP_Text label = WildRootUITheme.CreateText(cardRect, "Label", caption, 14f, FontStyles.Normal, TextAlignmentOptions.Left, WildRootUITheme.ColorTextBody);
                    WildRootUITheme.FitStretch(label.rectTransform, 14, 6, 14, 6);

                    LayoutElement layout = row.gameObject.AddComponent<LayoutElement>();
                    layout.minHeight = layout.preferredHeight = 72;
                }
                AnimalInstance selected = animal;
                AnimalHoverTooltip hover = row.GetComponent<AnimalHoverTooltip>();
                if (hover == null) hover = row.gameObject.AddComponent<AnimalHoverTooltip>();
                hover.Bind(selected);
                row.onClick.AddListener(() =>
                {
                    hover.Hide();
                    if (storage == null || !storage.Contains(selected)) return;
                    if (placementView) { if (net != null) net.TrySelectAnimal(selected); }
                    else AnimalInfoUI.Instance?.OpenStored(selected, storage);
                });
                rows.Add(row);
            }
        UpdateAvailability();
    }

    private void Update() => UpdateAvailability();
    private void UpdateAvailability()
    {
        bool canPlace = !placementView || (net != null && net.CanSelectAnimal);
        foreach (Button row in rows) if (row != null) row.interactable = canPlace;
        if (statusText != null)
        {
            if (storage == null)
            {
                statusText.text = "<color=#E68369>Net storage unavailable</color>";
            }
            else
            {
                string countStr = $"<b><color=#FFF3D6>Capacity: {storage.Count} / {storage.Capacity}</color></b>";
                string hintStr;
                if (rows.Count == 0)
                    hintStr = "<color=#A39482>Net is currently empty</color>";
                else if (!placementView)
                    hintStr = "<color=#88C888>Click animal to view stats</color>";
                else if (canPlace)
                    hintStr = "<color=#F4C453>Click animal to place in world/tank</color>";
                else
                    hintStr = "<color=#E68369>Hold Net tool to place animal</color>";

                statusText.text = $"{countStr}\n<size=12>{hintStr}</size>";
            }
        }
    }
}
