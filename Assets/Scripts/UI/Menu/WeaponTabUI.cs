using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
public class WeaponTabUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The WeaponManager on the player ship.")]
    [SerializeField] private WeaponManager weaponManager;

    [Header("Dropdown")]
    [Tooltip("TMP_Dropdown listing the available weapon slots.")]
    [SerializeField] private TMP_Dropdown weaponDropdown;

    [Header("Weapon Info")]
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private TMP_Text weaponTypeText;
    [SerializeField] private TMP_Text fireRateText;
    [SerializeField] private TMP_Text magazineText;
    [SerializeField] private TMP_Text reloadTimeText;
    [SerializeField] private TMP_Text critText;

    [Header("Shell Info (hidden for LASER)")]
    [SerializeField] private GameObject shellInfoGroup;   // Parent GO to show/hide
    [SerializeField] private TMP_Text shellNameText;
    [SerializeField] private TMP_Text shellDamageText;
    [SerializeField] private TMP_Text shellVelocityText;
    [SerializeField] private TMP_Text shellArmorPenText;
    [SerializeField] private TMP_Text shellEffectText;
    [SerializeField] private Image shellColorSwatch;  // Small colored square

    [Header("Laser Info (hidden for BALLISTIC)")]
    [SerializeField] private GameObject laserInfoGroup;  // Parent GO to show/hide
    [SerializeField] private TMP_Text laserEffectText;

    private void OnEnable()
    {
        if (weaponManager == null)
        {
            weaponManager = GameManager.instance.GetSpaceshipController()?.GetWeaponManager();
        }

        RefreshDropdown();
        DisplayWeapon(weaponManager.GetCurrentWeaponIndex());
    }

    // Dropdown 

    private void RefreshDropdown()
    {
        if (weaponDropdown == null) return;

        weaponDropdown.onValueChanged.RemoveAllListeners();

        List<TMP_Dropdown.OptionData> options = new();
        int count = weaponManager.GetWeaponCount();

        for (int i = 0; i < count; i++)
        {
            WeaponSO w = weaponManager.GetWeapon(i);
            string label = w != null
                ? $"[{i + 1}]  {w.weaponName}"
                : $"[{i + 1}]  — empty slot —";

            options.Add(new TMP_Dropdown.OptionData(label));
        }

        weaponDropdown.ClearOptions();
        weaponDropdown.AddOptions(options);
        weaponDropdown.SetValueWithoutNotify(weaponManager.GetCurrentWeaponIndex());

        weaponDropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    private void OnDropdownChanged(int index)
    {
        weaponManager.SwitchWeaponPublic(index);
        DisplayWeapon(index);
    }

    // Info display

    private void DisplayWeapon(int index)
    {
        WeaponSO weapon = weaponManager.GetWeapon(index);

        if (weapon == null)
        {
            ClearWeaponInfo();
            return;
        }

        // Common stats
        SetText(weaponNameText, weapon.weaponName);
        SetText(weaponTypeText, $"Weapon Type : {weapon.weaponType}");
        SetText(fireRateText, $"{weapon.fireRate:0.#} shots/s");
        SetText(critText, $"{weapon.critChance}%  x{weapon.critCoef:0.##}");

        if (weapon.hasMagazine)
            SetText(magazineText, $"{weapon.magazineSize} rounds");
        else
            SetText(magazineText, "(no magazine)");

        SetText(reloadTimeText, weapon.hasMagazine ? $"{weapon.reloadTime:0.#} s" : "-");

        // Panel visibility per weapon type
        bool isLaser = weapon.weaponType == WeaponType.LASER;
        shellInfoGroup?.SetActive(!isLaser);
        laserInfoGroup?.SetActive(isLaser);

        if (isLaser)
        {
            DisplayLaserInfo(weapon);
        }
        else
        {
            ShellSO shell = weaponManager.GetShell(index);
            DisplayShellInfo(shell);
        }
    }

    private void DisplayShellInfo(ShellSO shell)
    {
        if (shell == null)
        {
            SetText(shellNameText, "No shell loaded");
            SetText(shellDamageText, "-");
            SetText(shellVelocityText, "-");
            SetText(shellArmorPenText, "-");
            SetText(shellEffectText, "-");
            if (shellColorSwatch != null) shellColorSwatch.color = Color.grey;
            return;
        }

        SetText(shellNameText, shell.name);
        SetText(shellDamageText, $"STD {shell.standardDamage:0}  /  DUR {shell.durableDamage:0}");
        SetText(shellVelocityText, $"{shell.velocity} m/s");
        SetText(shellArmorPenText, shell.armorPen.ToString());

        TypeEffect fx = shell.GetTypeEffect();
        SetText(shellEffectText, fx != TypeEffect.NONE ? fx.ToString() : "none");

        if (shellColorSwatch != null) shellColorSwatch.color = shell.color;
    }

    private void DisplayLaserInfo(WeaponSO weapon)
    {
        if (laserEffectText == null) return;

        if (weapon.laserEffectData != null)
        {
            StatusEffectSO fx = weapon.laserEffectData;
            laserEffectText.text =
                $"Effect : {fx.effectType}\n" +
                $"Damage / tick : {fx.damagePerTick:0.#}\n" +
                $"Tick interval : {fx.tickInterval:0.##} s\n" +
                $"Duration : {fx.duration:0.#} s";
        }
        else
        {
            laserEffectText.text = "No laser effect assigned.";
        }
    }

    private void ClearWeaponInfo()
    {
        SetText(weaponNameText, "Empty slot");
        SetText(weaponTypeText, "-");
        SetText(fireRateText, "-");
        SetText(magazineText, "-");
        SetText(reloadTimeText, "-");
        SetText(critText, "-");
        shellInfoGroup?.SetActive(false);
        laserInfoGroup?.SetActive(false);
    }

    // Utility
    private static void SetText(TMP_Text label, string value)
    {
        if (label != null) label.text = value;
    }
}