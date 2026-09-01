using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueNode startingDialogue;

    public void Interact()
    {
        DialogueManager.Instance.StartDialogue(startingDialogue);
    }
}