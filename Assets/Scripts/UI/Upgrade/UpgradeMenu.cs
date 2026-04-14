using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SkillDescriptionPanel descriptionPanel;
    [Space(5)]

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

    private void OnEnable()
    {
        inventory.OnInventoryChanged += RefreshInventoryText;
        RefreshInventoryText();
    }

    private void OnDisable()
    {
        inventory.OnInventoryChanged -= RefreshInventoryText;
    }

    private void Start()
    {
        GenerateNodes();
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

    private void GenerateNodes()
    {
        GameObject previousNodeGO = null;

        foreach (SkillNodeSO node in skillTree.nodes)
        {
            GameObject nodeGO = Instantiate(nodePrefab, nodeCanvas);
            SkillNodeUI nodeUI = nodeGO.GetComponent<SkillNodeUI>();

            nodeUI.Init(node, skillTreeManager, descriptionPanel);

            nodeUIs[node] = nodeUI;

            // Position the node
            nodeGO.GetComponent<RectTransform>().anchoredPosition = node.canvasPosition;
            // Draw lines to prerequisites
            foreach (SkillNodeSO prereq in node.prerequisites)
            {
                if (nodeUIs.TryGetValue(prereq, out SkillNodeUI prereqUI))
                {
                    DrawLine(prereqUI.GetComponent<RectTransform>(), nodeGO.GetComponent<RectTransform>());
                }
            }
            previousNodeGO = nodeGO;
        }
    }

    private void DrawLine(RectTransform rectTransform1, RectTransform rectTransform2)
    {
        // Create a new GameObject with Image component for the line
        GameObject lineGO = new GameObject("Line", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));

        // Set the line GameObject as a child of the line parent
        lineGO.transform.SetParent(lineParent, false);
        Image lineImage = lineGO.GetComponent<Image>();
        lineImage.color = lineColor;
        RectTransform lineRect = lineGO.GetComponent<RectTransform>();

        // Calculate the direction and distance between the two nodes
        Vector2 dir = (rectTransform2.anchoredPosition - rectTransform1.anchoredPosition).normalized;
        float distance = Vector2.Distance(rectTransform1.anchoredPosition, rectTransform2.anchoredPosition);

        // Set the size and position of the line
        lineRect.sizeDelta = new Vector2(distance, lineThickness);
        lineRect.pivot = new Vector2(0, 0.5f);
        lineRect.anchoredPosition = rectTransform1.anchoredPosition;

        // Rotate the line to point from the first node to the second
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        lineRect.rotation = Quaternion.Euler(0, 0, angle);
    }
}
