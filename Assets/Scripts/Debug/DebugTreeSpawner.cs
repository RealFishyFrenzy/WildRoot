using UnityEngine;

public class DebugTreeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject treePrefab;

    private void Update()
    {
        if (PlayerController.Instance != null && !PlayerController.Instance.ControlsEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnTreeAtMouse();
        }
    }

    private void SpawnTreeAtMouse()
    {
        if (treePrefab == null)
            return;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = 0f;

        Instantiate(treePrefab, mousePosition, Quaternion.identity);
    }
}
