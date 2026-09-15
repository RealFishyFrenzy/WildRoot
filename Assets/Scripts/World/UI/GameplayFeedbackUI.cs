using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The overlay cannot intercept pointer input and does not modify TimeHUD.
public class GameplayFeedbackUI : MonoBehaviour
{
    [SerializeField] private PlayerInteraction interaction;
    [Header("Authored Layout")]
    [SerializeField] private GameObject overlay;
    [SerializeField] private TMP_Text promptText;
    [SerializeField] private TMP_Text notificationText;
    private readonly NotificationFeed feed = new NotificationFeed();
    private bool generatedOverlay;

    public void SetInteraction(PlayerInteraction source) => interaction = source;

    private void Awake()
    {
        if (overlay == null)
        {
            generatedOverlay = true;
            overlay = new GameObject("Gameplay Feedback (Runtime)", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            overlay.transform.SetParent(transform, false);
            Canvas canvas = overlay.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            CanvasScaler scaler = overlay.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }

        if (promptText == null)
            promptText = CreateText("Interaction Prompt", new Vector2(0.5f, 0f),
                new Vector2(0, 150), new Vector2(900, 45), TextAlignmentOptions.Center);
        if (notificationText == null)
            notificationText = CreateText("Action Notifications", new Vector2(1f, 0.5f),
                new Vector2(-30, 0), new Vector2(620, 150), TextAlignmentOptions.Right);
    }

    private TMP_Text CreateText(string label, Vector2 anchor, Vector2 position, Vector2 size, TextAlignmentOptions alignment)
    {
        GameObject child = new GameObject(label, typeof(RectTransform), typeof(TextMeshProUGUI));
        child.transform.SetParent(overlay.transform, false);
        RectTransform rect = child.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        TMP_Text text = child.GetComponent<TMP_Text>();
        text.font = TMP_Settings.defaultFontAsset;
        text.fontSize = 24;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.richText = false;
        text.text = "";
        return text;
    }

    private void OnEnable()
    {
        GameplayNotifications.Posted += OnPosted;
        if (overlay != null)
            overlay.SetActive(true);
    }

    private void OnDisable()
    {
        GameplayNotifications.Posted -= OnPosted;
        feed.Clear();
        if (overlay != null)
            overlay.SetActive(false);
    }

    private void OnDestroy()
    {
        if (generatedOverlay && overlay != null)
            Destroy(overlay);
    }

    private void OnPosted(string message, string repeatKey)
    {
        feed.Post(message, repeatKey, Time.unscaledTimeAsDouble);
    }

    private void LateUpdate()
    {
        IInteractable target = interaction != null ? interaction.ResolveTarget() : null;
        string label = target is IInteractionPrompt provider ? provider.InteractionLabel : "Interact";
        promptText.text = target == null ? "" : "E — " + label;
        notificationText.text = feed.GetText(Time.unscaledTimeAsDouble);
    }
}
