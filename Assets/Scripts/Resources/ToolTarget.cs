using UnityEngine;

public class ToolTarget : MonoBehaviour
{
    [SerializeField] protected ToolType requiredTool;
    [Header("Legacy baseline (used when Minimum Hits is zero)")]
    [SerializeField] protected int health = 3;
    [Header("Node requirements (zero hits preserves legacy baseline)")]
    [SerializeField, Min(1)] private int minimumToolPower = 1;
    // Zero preserves the serialized legacy health baseline until explicitly configured.
    [SerializeField, Min(0)] private int minimumHitsToBreak;
    [SerializeField, Min(0)] private int maximumHitsToBreak;

    private double remainingHits;
    private bool initialized;
    private bool destroyed;
    public double RemainingHitPoints => remainingHits;
    public int MinimumToolPower => System.Math.Max(1, minimumToolPower);
    public bool IsUnderpowered(ToolType type, int power) =>
        type == requiredTool && power < MinimumToolPower;

    [Header("Hit Feedback")]
    [SerializeField] private float shakeAmount = 3f;
    [SerializeField] private float shakeDuration = 0.12f;

    private Quaternion originalRotation;
    private Coroutine shakeCoroutine;

    protected virtual void Awake()
    {
        originalRotation = transform.localRotation;
        InitializeHits();
    }

    private void InitializeHits()
    {
        if (initialized) return;
        int minimum = minimumHitsToBreak > 0 ? minimumHitsToBreak : System.Math.Max(1, health);
        int maximum = maximumHitsToBreak > 0 ? System.Math.Max(minimum, maximumHitsToBreak) : minimum;
        // Long arithmetic avoids max + 1 overflow; Random.value includes both endpoints.
        long count = (long)maximum - minimum + 1;
        long offset = minimum == maximum ? 0 : System.Math.Min(count - 1,
            (long)(UnityEngine.Random.value * (double)count));
        remainingHits = minimum + offset;
        initialized = true;
    }

    public virtual bool UseTool(ToolType toolType, int power)
    {
        if (destroyed || toolType != requiredTool)
            return false;

        InitializeHits();
        PlayHitFeedback();
        if (IsUnderpowered(toolType, power))
        {
            GameplayNotifications.Show($"Need a stronger {requiredTool}", $"tool-power-{requiredTool}");
            return false;
        }

        // Bound the exponent before Pow: extreme powers still safely destroy in one hit.
        double damage = System.Math.Pow(1.75, System.Math.Min(64L, (long)power - MinimumToolPower));
        remainingHits = System.Math.Max(0, remainingHits - damage);
        Debug.Log($"{gameObject.name} hit with {toolType}. Remaining: {remainingHits}");

        if (remainingHits <= 0)
        {
            destroyed = true; // Unity destruction is deferred; never award drops twice.
            OnDestroyed();
        }
        return true;
    }

    protected virtual void PlayHitFeedback()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(Shake());

    }

    protected virtual void OnDestroyed()
    {
        Destroy(gameObject);
    }

    private System.Collections.IEnumerator Shake()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float rotation = Mathf.Sin(elapsed * 80f) * shakeAmount;

            transform.localRotation =
                originalRotation * Quaternion.Euler(0f, 0f, rotation);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = originalRotation;
        shakeCoroutine = null;
    }
}
