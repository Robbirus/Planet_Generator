using UnityEngine;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// Visual representation of a skill node inside the InfiniteCanvas
/// Spawned and configured at runtime by UpgradeMenu.
/// 
/// PREFAB SETUP:
/// Root : -> Button + SkillNodeUI
///     Icon -> Image
///     Name -> TMP_Text
///     Lock Icon -> Image (shown when locked/unavailable)
/// </summary>
[RequireComponent(typeof(Button))]
public class SkillNodeUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameLabel;
    [SerializeField] private Image lockIcon;
    [SerializeField] private Image background;
    [Space(10)]

    [Header("State Colors")]
    [SerializeField] private Color colorUnlocked  = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color colorAvailable = Color.white;
    [SerializeField] private Color colorLocked    = new Color(0.4f, 0.4f, 0.4f);

    private SkillNodeSO data;
    private SkillTreeManager manager;
    private Button btn;

    // Init
    ///<summary>Call this right after instantiation to bind data and manager</summary>
    public void Init(SkillNodeSO nodeData, SkillTreeManager treeManager)
    {
        this.data = nodeData;
        this.manager = treeManager;
        this.btn = GetComponent<Button>();

        // Position the node on the InfiniteCanvas
        GetComponent<RectTransform>().anchoredPosition = nodeData.canvasPosition;

        Refresh();

        // Listen for future unlocks to refresh data
        manager.OnNodeUnlocked += _ => Refresh();
    }

    // Refresh
    ///<summary>Updates visuals to reflect the current unlock state.</summary>
    public void Refresh()
    {
        if (data == null) return;

        bool unlocked = manager.IsUnlocked(data);
        bool available = manager.IsAvailable(data);

        // Name
        if(nameLabel != null)
        {
            nameLabel.text = data.displayName;
        }

        // Icon
        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
        }

        // Lock Overlay
        if(lockIcon != null)
        {
            lockIcon.gameObject.SetActive(!unlocked && !available);
        }

        // Background tint
        if(background != null)
        {
            background.color = unlocked  ? colorUnlocked :
                               available ? colorAvailable : colorLocked;
        }

        // Button
        if(btn != null)
        {
            btn.onClick.AddListener(OnClick);
        }

        // Disable the button if already unlocked or not yet available
        btn.interactable = available;
    }

    // Click
    private void OnClick()
    {
        if(manager.TryUnlock(data))
        {
            Refresh();
        }
        else
        {
            LogOnClick();
        }
    }

    private void LogOnClick()
    {
        Debug.Log($"Clicked on {data.displayName} (ID: {data.id})");
        Debug.Log($"[SkillNodeUI] Cannot unlock {data.displayName} - check prerequisites or resources.");

        if (data.prerequisites.Count == 0)
        {
            Debug.Log("No prerequisites for this node.");
        }
        else
        {
            Debug.Log($"Prerequisites are : {string.Join(", ", data.prerequisites)}");
        }

        foreach (SkillCost cost in data.costs)
        {
            Debug.Log($"Resources required are : {cost.amount} of {cost.resourceType}");
        }
    }

    public SkillNodeSO GetData()
    {
        return data;
    }
}
