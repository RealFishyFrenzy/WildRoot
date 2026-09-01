using UnityEngine;

public class ToolTarget : MonoBehaviour
{
    [SerializeField] protected ToolType requiredTool;
    [SerializeField] protected int health = 3;

    [Header("Hit Feedback")]
    [SerializeField] private float shakeAmount = 3f;
    [SerializeField] private float shakeDuration = 0.12f;

    private Quaternion originalRotation;
    private Coroutine shakeCoroutine;

    protected virtual void Awake()
    {
        originalRotation = transform.localRotation;
    }

    public virtual bool UseTool(ToolType toolType, int power)
    {
        if (toolType != requiredTool)
            return false;

        health -= power;

        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);

        shakeCoroutine = StartCoroutine(Shake());

        Debug.Log($"{gameObject.name} hit with {toolType}. Health: {health}");

        if (health <= 0)
        {
            OnDestroyed();
        }

        return true;
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