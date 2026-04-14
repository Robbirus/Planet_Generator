using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Text;

public class SkillDescriptionPanel : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costsText;
    [SerializeField] private TMP_Text effectsText;
    [SerializeField] private TMP_Text statusText;
    [Space(5)]

    [Header("Status Colors")]
    [SerializeField] private Color colorUnlocked = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color colorAvailable = Color.white;
    [SerializeField] private Color colorLocked = new Color(0.8f, 0.3f, 0.3f);

    /// <summary>
    /// Populates and shows the panel for the given node.
    /// Called by SkillNodeUI on single click.
    /// </summary>
    public void Show(SkillNodeSO node, SkillTreeManager manager)
    {
        if (node == null) return;

        gameObject.SetActive(true);

        bool unlocked  = manager.IsUnlocked(node);
        bool available = manager.IsAvailable(node);

        // Icon
        if (iconImage != null)
            iconImage.sprite = node.icon;

        // Name
        if (nameText != null)
            nameText.text = node.displayName;

        // Description
        if (descriptionText != null)
            descriptionText.text = node.description;

        // Costs
        if (costsText != null)
            costsText.text = BuildCostText(node);

        // Effects
        if (effectsText != null)
            effectsText.text = BuildEffectText(node);

        // Status
        if (statusText != null)
        {
            if (unlocked)
            {
                statusText.text = "Unlocked";
                statusText.color = colorUnlocked;
            }
            else if (available)
            {
                statusText.text = "Double-click to unlock";
                statusText.color = colorAvailable;
            }
            else
            {
                statusText.text = "Locked - prerequisites not met";
                statusText.color = colorLocked;
            }
        }
    }

    /// <summary>Hides the panel.</summary>
    public void Hide()
    {
        gameObject.SetActive(false);
    }

    private string BuildCostText(SkillNodeSO node)
    {
        if(node.costs.Count == 0)
        {
            return "Cost : Free";
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Cost:");
        foreach (SkillCost cost in node.costs)
        {
            sb.AppendLine($"- {cost.resourceType} {cost.amount:0}");
        }

        return sb.ToString();
    }

    private string BuildEffectText(SkillNodeSO node)
    {
        if (node.effects.Count == 0)
            return "No effects.";

        StringBuilder sb = new();
        sb.AppendLine("Effects:");
        foreach (SkillEffect effect in node.effects)
        {
            string sign = effect.value >= 0 ? "+" : "";
            string unit = effect.mode == SkillModifierMode.Ratio ? "%" : "";
            float display = effect.mode == SkillModifierMode.Ratio
                             ? effect.value * 100f
                             : effect.value;

            sb.AppendLine($"- {effect.type}  {sign}{display:0.##}{unit}");
        }

        return sb.ToString();
    }
}
