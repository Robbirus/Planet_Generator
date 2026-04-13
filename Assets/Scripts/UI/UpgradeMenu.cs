using TMPro;
using UnityEngine;

public class UpgradeMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text resourceList;

    public void SetCurrentUpgrade()
    {
        Debug.Log("Upgrade selected");
    }

    public void PurchaseUpgrade()
    {
        Debug.Log("Skill purchased");
    }
}
