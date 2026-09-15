using UnityEngine;

[DefaultExecutionOrder(-1000)]
public class DebugTreeSpawner : MonoBehaviour
{
    [SerializeField] private GameObject treePrefab;
    private DebugResourceRefresh resourceRefresh;

    private void Awake()
    {
        if (Debug.isDebugBuild)
        {
            resourceRefresh = GetComponent<DebugResourceRefresh>();
            if (resourceRefresh == null)
                resourceRefresh = gameObject.AddComponent<DebugResourceRefresh>();
        }
    }

    private void Update()
    {
        if (PlayerController.Instance != null && !PlayerController.Instance.ControlsEnabled)
            return;

        if (Input.GetKeyDown(KeyCode.T))
        {
            SpawnTreeAtMouse();
        }
        if (Debug.isDebugBuild && Input.GetKeyDown(KeyCode.L))
            resourceRefresh?.Refresh();
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
