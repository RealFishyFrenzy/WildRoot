using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerText;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Responses")]
    [SerializeField] private GameObject responseContainer;
    [SerializeField] private Button responseButtonPrefab;

    private DialogueNode currentNode;
    private bool showingResponses;
    private int dialogueOpenedFrame;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        dialoguePanel.SetActive(false);
        responseContainer.SetActive(false);
    }

    private void Update()
    {
        if (!dialoguePanel.activeSelf)
            return;

        if (Time.frameCount == dialogueOpenedFrame)
            return;

        if (!showingResponses &&
            (Input.GetKeyDown(KeyCode.E) || Input.GetMouseButtonDown(0)))
        {
            ShowResponses();
        }
    }

    public void StartDialogue(DialogueNode startingNode)
    {
        if (startingNode == null)
            return;

        PlayerController.Instance.SetControlsEnabled(false);

        dialogueOpenedFrame = Time.frameCount;

        dialoguePanel.SetActive(true);
        ShowNode(startingNode);
    }

    private void ShowNode(DialogueNode node)
    {
        currentNode = node;
        showingResponses = false;

        speakerText.text = node.speaker;
        dialogueText.text = node.dialogueText;

        dialogueText.gameObject.SetActive(true);
        responseContainer.SetActive(false);

        ClearResponses();
    }

    private void ShowResponses()
    {
        if (currentNode == null)
            return;

        if (currentNode.responses == null || currentNode.responses.Length == 0)
        {
            CloseDialogue();
            return;
        }

        showingResponses = true;

        dialogueText.gameObject.SetActive(false);
        responseContainer.SetActive(true);

        ClearResponses();

        foreach (DialogueResponse response in currentNode.responses)
        {
            CreateResponseButton(response);
        }
    }

    private void CreateResponseButton(DialogueResponse response)
    {
        Button button = Instantiate(
            responseButtonPrefab,
            responseContainer.transform
        );

        TMP_Text buttonText = button.GetComponentInChildren<TMP_Text>();
        buttonText.text = response.responseText;

        button.onClick.AddListener(() =>
        {
            SelectResponse(response);
        });
    }

    private void SelectResponse(DialogueResponse response)
    {
        if (response.nextNode == null)
        {
            CloseDialogue();
            return;
        }

        ShowNode(response.nextNode);
    }

    private void ClearResponses()
    {
        foreach (Transform child in responseContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void CloseDialogue()
    {
        dialoguePanel.SetActive(false);

        currentNode = null;
        showingResponses = false;

        ClearResponses();

        PlayerController.Instance.SetControlsEnabled(true);
    }
}