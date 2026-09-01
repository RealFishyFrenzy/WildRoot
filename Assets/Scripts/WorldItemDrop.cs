using System.Collections;
using UnityEngine;

public class WorldItemDrop : MonoBehaviour
{
    [SerializeField] private ItemData item;
    [SerializeField] private int quantity = 1;

    [Header("Drop Movement")]
    [SerializeField] private float burstDistance = 0.6f;
    [SerializeField] private float burstDuration = 0.25f;

    private bool canPickup = false;

    public void Setup(ItemData newItem, int newQuantity)
    {
        item = newItem;
        quantity = newQuantity;

        if (item != null && item.icon != null)
        {
            SpriteRenderer spriteRenderer =
                GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = item.icon;
            }
        }

        StartCoroutine(BurstOut());
    }

    private IEnumerator BurstOut()
    {
        Vector3 startPosition = transform.position;

        Vector2 randomDirection =
            Random.insideUnitCircle.normalized;

        float randomDistance =
            Random.Range(burstDistance * 0.6f, burstDistance);

        Vector3 targetPosition =
            startPosition +
            (Vector3)(randomDirection * randomDistance);

        float elapsed = 0f;

        while (elapsed < burstDuration)
        {
            elapsed += Time.deltaTime;

            float t =
                Mathf.Clamp01(elapsed / burstDuration);

            transform.position =
                Vector3.Lerp(
                    startPosition,
                    targetPosition,
                    t
                );

            yield return null;
        }

        transform.position = targetPosition;

        canPickup = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryPickup(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryPickup(other);
    }

    private void TryPickup(Collider2D other)
    {
        if (!canPickup)
            return;

        PlayerInventory inventory = other.GetComponent<PlayerInventory>();

        if (inventory == null || item == null)
            return;

        inventory.AddItem(item, quantity);

        HotbarUI hotbarUI = FindAnyObjectByType<HotbarUI>();

        if (hotbarUI != null)
            hotbarUI.Refresh();

        Debug.Log($"Picked up {quantity} {item.itemName}.");

        Destroy(gameObject);
    }
}