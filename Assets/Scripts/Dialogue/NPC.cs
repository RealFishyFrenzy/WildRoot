using UnityEngine;

public class NPC : MonoBehaviour, IInteractable, IInteractionPrompt
{
    public string InteractionLabel => "Talk";
    [SerializeField] private DialogueNode startingDialogue;

    public void Interact()
    {
        DialogueManager.Instance.StartDialogue(startingDialogue);
    }
}
