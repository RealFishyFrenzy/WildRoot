using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    private bool controlsEnabled = true;
    private int blockedInputFrame = -1;

    public bool ControlsEnabled => controlsEnabled &&
        blockedInputFrame != Time.frameCount &&
        !(InventoryUI.Instance != null && InventoryUI.Instance.IsOpen);

    public void BlockControlsForCurrentFrame()
    {
        blockedInputFrame = Time.frameCount;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetControlsEnabled(bool enabled)
    {
        controlsEnabled = enabled;
    }
}
