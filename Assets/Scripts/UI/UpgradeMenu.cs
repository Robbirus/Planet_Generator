using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeMenu : MonoBehaviour
{
    [Header("Inventory")]
    [SerializeField] private ShipInventory inventory;
    [Space(5)]

    [Header("UI References")]
    [SerializeField] private TMP_Text resourceList;

    private Dictionary<ResourceType, float> stock = new Dictionary<ResourceType, float>();

    public void SetCurrentUpgrade()
    {
        Debug.Log("Upgrade selected");
    }

    public void PurchaseUpgrade()
    {
        Debug.Log("Skill purchased");
    }

    public void Update()
    {
        if (inventory != null && gameObject.activeSelf == true)
        {
            stock = inventory.GetAll();
            resourceList.text = "Resource :\n";

            foreach(ResourceType key in stock.Keys)
            {
                resourceList.text += key.ToString() + " : " + inventory.Get(key).ToString("0") + "\n";
            }
        }
    }
}
