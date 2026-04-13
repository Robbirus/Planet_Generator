using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeMenu : MonoBehaviour
{
    [Header("Skill Tree")]
    [SerializeField] private SkillTreeSO skillTree;
    [SerializeField] private SkillTreeManager skillTreeManager;
    [SerializeField] private RectTransform nodeCanvas;
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private RectTransform lineParent;
    [Space(10)]

    [Header("Inventory display")]
    [SerializeField] private ShipInventory inventory;
    [SerializeField] private TMP_Text resourceListText;
    [Space(10)]

    [Header("Line style")]
    [SerializeField] private Color lineColor = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private float lineThickness = 3f;
    [Space(10)]

    private readonly Dictionary<SkillNodeSO, SkillNodeUI> nodeUIs = new();
    private bool initialized = false;

    public void SetCurrentUpgrade()
    {
        Debug.Log("Upgrade selected");
    }

    public void PurchaseUpgrade()
    {
        Debug.Log("Skill purchased");
    }

    private void OnEnable()
    {
        inventory.OnInventoryChanged += RefreshInventoryText;
        RefreshInventoryText();
    }

    private void OnDisable()
    {
        inventory.OnInventoryChanged -= RefreshInventoryText;
    }

    // „Ÿ„Ÿ Inventory display „Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ„Ÿ

    private void RefreshInventoryText()
    {
        if (resourceListText == null || inventory == null) return;

        System.Text.StringBuilder sb = new();
        sb.AppendLine("Resources :");

        foreach (var pair in inventory.GetAll())
            sb.AppendLine($"  {pair.Key} : {pair.Value:0}");

        resourceListText.text = sb.ToString();
    }
}
