using System.Collections.Generic;
using UnityEngine;

// Runtime-only debug baseline. Never saves or changes the authored scene/assets.
public sealed class DebugResourceRefresh : MonoBehaviour
{
    private sealed class Placement
    {
        public GameObject template;
        public Transform parent;
        public UnityEngine.SceneManagement.Scene scene;
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 localScale;
        public bool active;
    }

    private readonly List<Placement> placements = new List<Placement>();
    private GameObject templates;
    private int refreshedFrame = -1;

    private void Awake()
    {
        if (!Debug.isDebugBuild) return;
        templates = new GameObject("Debug Resource Baselines");
        templates.SetActive(false); // Clones must not run Awake until restored.
        templates.hideFlags = HideFlags.HideAndDontSave;
        foreach (GameObject node in FindNodes())
        {
            placements.Add(new Placement
            {
                template = Instantiate(node, templates.transform),
                parent = node.transform.parent,
                scene = node.scene,
                position = node.transform.position,
                rotation = node.transform.rotation,
                localScale = node.transform.localScale,
                active = node.activeSelf
            });
        }
    }

    private HashSet<GameObject> FindNodes()
    {
        var nodes = new HashSet<GameObject>();
        foreach (ToolTarget target in FindObjectsByType<ToolTarget>(FindObjectsInactive.Include))
        {
            // CatchableAnimal also derives from ToolTarget; it is not a resource.
            if (!(target is ResourceNode) && !(target is Tree) && !(target is Rock)) continue;
            if (templates != null && target.transform.IsChildOf(templates.transform)) continue;
            if (target.gameObject.scene.IsValid()) nodes.Add(target.gameObject);
        }
        return nodes;
    }

    public void Refresh()
    {
        if (!Debug.isDebugBuild || templates == null || refreshedFrame == Time.frameCount) return;
        refreshedFrame = Time.frameCount;
        foreach (GameObject node in FindNodes())
        {
            node.SetActive(false); // Disable colliders immediately; Destroy is deferred.
            Destroy(node); // Bypass gathering/drop callbacks.
        }
        foreach (Placement placement in placements)
        {
            if (placement.template == null || !placement.scene.isLoaded) continue;
            GameObject node = Instantiate(placement.template, templates.transform);
            node.SetActive(false);
            node.transform.SetParent(null, false);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(node, placement.scene);
            node.transform.SetParent(placement.parent, false);
            node.transform.SetPositionAndRotation(placement.position, placement.rotation);
            node.transform.localScale = placement.localScale;
            node.name = placement.template.name.Replace("(Clone)", "");
            node.SetActive(placement.active); // Fresh Awake resets node hit budget.
        }
        foreach (CaveResourceSpawner spawner in FindObjectsByType<CaveResourceSpawner>(FindObjectsInactive.Include))
            spawner.DebugRespawnNodes();
        Debug.Log("Debug: Resource nodes refreshed.");
    }

    private void OnDestroy()
    {
        if (templates != null) Destroy(templates);
    }
}
