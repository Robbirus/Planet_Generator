using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class WeaponTabUI : MonoBehaviour
{
    [Header("Weapon Manager")]
    [SerializeField] private WeaponManager weaponManager;

    [Header("Dropdown")]
    [SerializeField] private TMP_Dropdown weaponDropdown;

    [Header("Switch Button")]
    [Tooltip("The button that alternates between the Weapon and Shell/Laser view.")]
    [SerializeField] private Button switchButton;

    [Header("Weapon info")]
    [SerializeField] private GameObject weaponInfoGroup;
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private TMP_Text weaponTypeText;
    [SerializeField] private TMP_Text fireRateText;
    [SerializeField] private TMP_Text magazineText;
    [SerializeField] private TMP_Text reloadTimeText;
    [SerializeField] private TMP_Text critText;

    [Header("Shell info")]
    [SerializeField] private GameObject shellInfoGroup;
    [SerializeField] private TMP_Text shellNameText;
    [SerializeField] private TMP_Text shellDamageText;
    [SerializeField] private TMP_Text shellVelocityText;
    [SerializeField] private TMP_Text shellArmorPenText;
    [SerializeField] private TMP_Text shellEffectText;
    [SerializeField] private Image shellColorSwatch;

    [Header("Laser info")]
    [SerializeField] private GameObject laserInfoGroup;
    [SerializeField] private TMP_Text laserEffectText;

    private enum DisplayMode { Weapon, Ammo }
    private DisplayMode currentMode = DisplayMode.Weapon;

    private void Awake()
    {
        if (switchButton != null)
        {
            switchButton.onClick.RemoveAllListeners();
            switchButton.onClick.AddListener(OnSwitchClicked);
        }
    }

    private void OnEnable()
    {
        if (weaponManager == null)
            weaponManager = GameManager.instance.GetSpaceshipController()?.GetWeaponManager();

        if (weaponManager == null) { Debug.LogWarning("[WeaponTabUI] WeaponManager introuvable."); return; }

        RefreshDropdown();
        ApplyMode(weaponManager.GetCurrentWeaponIndex());
    }

    private void OnDisable()
    {
        weaponDropdown?.onValueChanged.RemoveAllListeners();
    }

    private void RefreshDropdown()
    {
        if (weaponDropdown == null) return;

        weaponDropdown.onValueChanged.RemoveAllListeners();

        List<TMP_Dropdown.OptionData> options = new();
        for (int i = 0; i < weaponManager.GetWeaponCount(); i++)
        {
            WeaponSO w = weaponManager.GetWeapon(i);
            options.Add(new TMP_Dropdown.OptionData(
                w != null ? $"[{i + 1}]  {w.weaponName}" : $"[{i + 1}]  - empty -"));
        }

        weaponDropdown.ClearOptions();
        weaponDropdown.AddOptions(options);
        weaponDropdown.SetValueWithoutNotify(weaponManager.GetCurrentWeaponIndex());
        weaponDropdown.onValueChanged.AddListener(OnDropdownChanged);
    }

    private void OnDropdownChanged(int index)
    {
        weaponManager.SwitchWeaponPublic(index);
        ApplyMode(index);
    }

    private void OnSwitchClicked()
    {
        currentMode = currentMode == DisplayMode.Weapon
            ? DisplayMode.Ammo
            : DisplayMode.Weapon;

        ApplyMode(weaponManager != null ? weaponManager.GetCurrentWeaponIndex() : 0);
    }

    /// <summary>
    /// Activates/deactivates the 3 groups according to the mode and type of weapon,
    /// and then fills in the corresponding data.
    /// </summary>
    private void ApplyMode(int weaponIndex)
    {
        WeaponSO weapon = weaponManager?.GetWeapon(weaponIndex);

        if (weapon == null)
        {
            ShowGroups(weapon: false, shell: false, laser: false);
            ClearAll();
            return;
        }

        bool showWeapon = currentMode == DisplayMode.Weapon;
        bool isLaser = weapon.weaponType == WeaponType.LASER;

        ShowGroups(
            weapon: showWeapon,
            shell: !showWeapon && !isLaser,
            laser: !showWeapon && isLaser
        );

        if (showWeapon)
            FillWeapon(weapon);
        else if (isLaser)
            FillLaser(weapon);
        else
            FillShell(weaponManager.GetShell(weaponIndex));
    }

    private void ShowGroups(bool weapon, bool shell, bool laser)
    {
        weaponInfoGroup?.SetActive(weapon);
        shellInfoGroup?.SetActive(shell);
        laserInfoGroup?.SetActive(laser);
    }

    private void FillWeapon(WeaponSO w)
    {
        SetText(weaponNameText, w.weaponName);
        SetText(weaponTypeText, $"Type : {w.weaponType}");
        SetText(fireRateText, $"Fire Rate : {w.fireRate:0.#} shot/s");
        SetText(critText, $"Crit : {w.critChance}%  ×{w.critCoef:0.##}");
        SetText(magazineText, w.hasMagazine ? $"{w.magazineSize} magazine" : "no magazine");
        SetText(reloadTimeText, w.hasMagazine ? $"Reload Time : {w.reloadTime:0.#} s" : "-");
    }

    private void FillShell(ShellSO shell)
    {
        if (shell == null)
        {
            SetText(shellNameText, "No loaded shell");
            SetText(shellDamageText, shellVelocityText, shellArmorPenText, shellEffectText, "-");
            if (shellColorSwatch) shellColorSwatch.color = Color.grey;
            return;
        }

        SetText(shellNameText, shell.name);
        SetText(shellDamageText, $"Damage  STD {shell.standardDamage:0}  /  DUR {shell.durableDamage:0}");
        SetText(shellVelocityText, $"Speed : {shell.velocity} m/s");
        SetText(shellArmorPenText, $"Penetration : {shell.armorPen}");

        TypeEffect fx = shell.GetTypeEffect();
        SetText(shellEffectText, $"Effect : {(fx != TypeEffect.NONE ? fx.ToString() : "none")}");

        if (shellColorSwatch) shellColorSwatch.color = shell.color;
    }

    private void FillLaser(WeaponSO w)
    {
        if (laserEffectText == null) return;

        if (w.laserEffectData != null)
        {
            StatusEffectSO fx = w.laserEffectData;
            laserEffectText.text =
                $"Effect : {fx.effectType}\n" +
                $"Damage / tick : {fx.damagePerTick:0.#}\n" +
                $"Interval : {fx.tickInterval:0.##} s\n" +
                $"Duration : {fx.duration:0.#} s";
        }
        else
        {
            laserEffectText.text = "No effect.";
        }
    }

    private void ClearAll()
    {
        SetText(weaponNameText, "-"); SetText(weaponTypeText, "-");
        SetText(fireRateText, "-"); SetText(magazineText, "-");
        SetText(reloadTimeText, "-"); SetText(critText, "-");
    }

    private static void SetText(TMP_Text t, string v) { if (t) t.text = v; }

    private static void SetText(TMP_Text a, TMP_Text b, TMP_Text c, TMP_Text d, string v)
    {
        SetText(a, v); SetText(b, v); SetText(c, v); SetText(d, v);
    }
}