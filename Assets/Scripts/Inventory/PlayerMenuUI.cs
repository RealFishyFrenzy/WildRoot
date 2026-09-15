using UnityEngine;
using UnityEngine.UI;

// Navigation/presentation only. InventoryUI remains the open/close/input authority.
public class PlayerMenuUI : MonoBehaviour
{
    [Header("Optional authored layout (leave empty for temporary runtime layout)")]
    [SerializeField] private GameObject menuRoot;
    [SerializeField] private GameObject playerPage;
    [SerializeField] private GameObject settingsPage;
    [SerializeField] private Button playerTab;
    [SerializeField] private Button settingsTab;
    [SerializeField] private NetStorageListUI netList;
    [SerializeField] private Image playerPortrait;
    [Header("Hand crafting (explicit catalog; station recipes are filtered out)")]
    [SerializeField] private CraftingRecipe[] handCraftingRecipes;
    [SerializeField] private GameObject craftingPage;
    [SerializeField] private Button craftingTab;
    [SerializeField] private HandCraftingMenuUI craftingView;

    private InventoryUI owner;
    private bool initialized;
    private bool generatedLayout;
    public bool IsOpen => menuRoot != null && menuRoot.activeInHierarchy;
    public bool IsPlayerTab => playerPage != null && playerPage.activeSelf;

    public void Initialize(InventoryUI inventory, GameObject existingInventoryPanel)
    {
        if (initialized)
            return;
        owner = inventory;
        if (menuRoot == null)
        {
            generatedLayout = true;
            PlayerMenuLayout.Build(existingInventoryPanel, out menuRoot, out playerPage,
                out settingsPage, out playerTab, out settingsTab, out netList, out playerPortrait,
                out craftingPage, out craftingTab, out craftingView);
        }
        existingInventoryPanel.SetActive(true);

        if (craftingPage == null)
            craftingPage = HandCraftingMenuUI.CreatePage(menuRoot.transform);
        if (craftingView == null)
            craftingView = craftingPage.GetComponent<HandCraftingMenuUI>();
        if (craftingView == null)
            craftingView = craftingPage.AddComponent<HandCraftingMenuUI>();
        if ((handCraftingRecipes == null || handCraftingRecipes.Length == 0))
        {
#if UNITY_EDITOR
            string[] recipeGuids = UnityEditor.AssetDatabase.FindAssets("t:CraftingRecipe");
            var recipeList = new System.Collections.Generic.List<CraftingRecipe>();
            foreach (var guid in recipeGuids)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var recipe = UnityEditor.AssetDatabase.LoadAssetAtPath<CraftingRecipe>(path);
                if (recipe != null && recipe.context == RecipeContext.HandCrafting)
                    recipeList.Add(recipe);
            }
            if (recipeList.Count > 0)
                handCraftingRecipes = recipeList.ToArray();
#endif
        }

        craftingView.Bind(inventory.Inventory, handCraftingRecipes);
        if (craftingTab == null)
        {
            craftingTab = PlayerMenuLayout.CreateTabButton(
                playerTab != null ? playerTab.transform.parent : menuRoot.transform,
                "Crafting Tab", "⚒ Crafting", false);
            var rect = (RectTransform)craftingTab.transform;
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 0.5f);
            rect.anchoredPosition = new Vector2(178, 0);
            rect.sizeDelta = new Vector2(160, 0);
            if (settingsTab != null)
                ((RectTransform)settingsTab.transform).anchoredPosition = new Vector2(346, 0);
        }
        craftingPage.SetActive(false);

        if (playerTab != null) playerTab.onClick.AddListener(ShowPlayer);
        if (settingsTab != null) settingsTab.onClick.AddListener(ShowSettings);
        if (craftingTab != null) craftingTab.onClick.AddListener(ShowCrafting);
        NetUI net = FindAnyObjectByType<NetUI>();
        if (netList != null) netList.Bind(net, false);
        if (playerPortrait != null && playerPortrait.sprite == null)
        {
            PlayerMovement player = FindAnyObjectByType<PlayerMovement>();
            SpriteRenderer sprite = player != null ? player.GetComponent<SpriteRenderer>() : null;
            if (sprite != null) playerPortrait.sprite = sprite.sprite;
            playerPortrait.enabled = playerPortrait.sprite != null;
        }
        initialized = true;
    }

    public void Show()
    {
        if (menuRoot == null) return;
        menuRoot.SetActive(true);
        menuRoot.transform.SetAsLastSibling();
        ShowPlayer();
    }

    public void Hide()
    {
        AnimalInfoUI.Instance?.CloseStored();
        if (menuRoot != null) menuRoot.SetActive(false);
    }

    public void ShowPlayer() => SelectPage(0);
    public void ShowCrafting() => SelectPage(1);
    public void ShowSettings() => SelectPage(2);

    private void SelectPage(int page)
    {
        bool player = page == 0;
        AnimalInfoUI.Instance?.CloseStored();
        owner?.CancelSlotSelection();
        if (playerPage != null) playerPage.SetActive(player);
        if (craftingPage != null) craftingPage.SetActive(page == 1);
        if (settingsPage != null) settingsPage.SetActive(page == 2);

        if (playerTab != null)
        {
            playerTab.interactable = page != 0;
            UnityEngine.UI.Image pImg = playerTab.GetComponent<UnityEngine.UI.Image>();
            if (pImg != null) pImg.sprite = page == 0 ? WildRootUITheme.TabActive : WildRootUITheme.TabInactive;
            TMPro.TMP_Text pTxt = playerTab.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (pTxt != null) pTxt.color = page == 0 ? WildRootUITheme.ColorTextTitle : WildRootUITheme.ColorTextMuted;
        }
        if (craftingTab != null)
        {
            craftingTab.interactable = page != 1;
            UnityEngine.UI.Image cImg = craftingTab.GetComponent<UnityEngine.UI.Image>();
            if (cImg != null) cImg.sprite = page == 1 ? WildRootUITheme.TabActive : WildRootUITheme.TabInactive;
            TMPro.TMP_Text cTxt = craftingTab.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (cTxt != null) cTxt.color = page == 1 ? WildRootUITheme.ColorTextTitle : WildRootUITheme.ColorTextMuted;
        }
        if (settingsTab != null)
        {
            settingsTab.interactable = page != 2;
            UnityEngine.UI.Image sImg = settingsTab.GetComponent<UnityEngine.UI.Image>();
            if (sImg != null) sImg.sprite = page == 2 ? WildRootUITheme.TabActive : WildRootUITheme.TabInactive;
            TMPro.TMP_Text sTxt = settingsTab.GetComponentInChildren<TMPro.TMP_Text>(true);
            if (sTxt != null) sTxt.color = page == 2 ? WildRootUITheme.ColorTextTitle : WildRootUITheme.ColorTextMuted;
        }
    }

    private void OnDestroy()
    {
        if (playerTab != null) playerTab.onClick.RemoveListener(ShowPlayer);
        if (settingsTab != null) settingsTab.onClick.RemoveListener(ShowSettings);
        if (craftingTab != null) craftingTab.onClick.RemoveListener(ShowCrafting);
        if (generatedLayout && menuRoot != null) Destroy(menuRoot);
    }
}
