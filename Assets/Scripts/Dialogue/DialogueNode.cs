using UnityEngine;

[CreateAssetMenu(fileName = "NewDialogueNode", menuName = "Dialogue/Dialogue Node")]
public class DialogueNode : ScriptableObject
{
    public string speaker;

    [TextArea(3, 10)]
    public string dialogueText;

    public DialogueResponse[] responses;
}