using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the ship's 4 weapons slots : firing, reload, weapon switching, and crit computations.
/// </summary>
public class WeaponManager : MonoBehaviour
{
    // Weapon slots
    [Serializable]
    public class WeaponSlot
    {
        public WeaponSO weapon;
        [Tooltip("Ammo currently loaded — defaults to weapon.defaultShell.")]
        public ShellSO loadedShell;
        [Tooltip("Transform from which the shell is spawned.")]
        public Transform spawnPoint;
        public bool hasMagasine; // If false, weapon doesn't consume ammo and doesn't reload (e.g. energy weapons)
        [HideInInspector] public int currentAmmo;
        [HideInInspector] public bool isReloading;
        [HideInInspector] public float fireTimer;
    }

    [Header("Weapon Slots")]
    [SerializeField] private WeaponSlot[] weaponSlots = new WeaponSlot[4];
    [Space(10)]

    [Header("Projectile")]
    [SerializeField] private GameObject shellPrefab;
    [Space(10)]

    [Header("Critical Hits")]
    [Tooltip("The base chance of doing a critical damage")]
    [SerializeField] private int critChance = 2;
    [Tooltip("The base critical damage coefficient")]
    [SerializeField] private float critCoef = 1.1f;
    [Tooltip("The pity allows to temporarily up the crit chance")]
    [Range(0, 100)]
    [SerializeField] private int pity = 0;
    [Space(10)]

    [Header("VFX")]
    [SerializeField] private ParticleSystem shootParticle;
    [Space(10)]

    [Header("Input Action Reference")]
    [SerializeField] private InputActionReference shootActionReference;
    [SerializeField] private InputActionReference firstWeaponReference;
    [SerializeField] private InputActionReference secondWeaponReference;
    [SerializeField] private InputActionReference thirdWeaponReference;
    [SerializeField] private InputActionReference fourthWeaponReference;

    #region Events
    public event Action<ShellSO> OnShellChanged;
    public event Action<float, float> OnReloadProgress;
    public event Action<bool> OnReloadStateChanged;
    #endregion

    private int currentWeaponIndex = 0;
    private int consecutiveMisses = 0;

    private WeaponSlot GetCurrentWeapon()
    {
        return weaponSlots[currentWeaponIndex];
    }

    private void Awake()
    {
        foreach(WeaponSlot slot in weaponSlots)
        {
            if (slot.weapon == null) continue;

            // Load default ammo if none assigned
            if(slot.loadedShell == null)
                slot.loadedShell = slot.weapon.defaultShell;

            slot.currentAmmo = slot.weapon.magazineSize;
            slot.isReloading = false;
            slot.fireTimer = 0f;
        }
    }

    private void OnEnable()
    {
        shootActionReference?.action.Enable();
        firstWeaponReference?.action.Enable();
        secondWeaponReference?.action.Enable();
        thirdWeaponReference?.action.Enable();
        fourthWeaponReference?.action.Enable();

        if(firstWeaponReference != null)
        {
            firstWeaponReference.action.performed += ctx => SwitchWeapon(0);
        }

        if (secondWeaponReference != null)
        {
            secondWeaponReference.action.performed += ctx => SwitchWeapon(1);
        }

        if (thirdWeaponReference != null)
        {
            thirdWeaponReference.action.performed += ctx => SwitchWeapon(2);
        }

        if (fourthWeaponReference != null)
        {
            fourthWeaponReference.action.performed += ctx => SwitchWeapon(3);
        }
    }

    private void OnDisable()
    {
        shootActionReference?.action.Disable();
        firstWeaponReference?.action.Disable();
        secondWeaponReference?.action.Disable();
        thirdWeaponReference?.action.Disable();
        fourthWeaponReference?.action.Disable();

        if (firstWeaponReference != null)
        {
            firstWeaponReference.action.performed -= ctx => SwitchWeapon(0);
        }

        if (secondWeaponReference != null)
        {
            secondWeaponReference.action.performed -= ctx => SwitchWeapon(1);
        }

        if (thirdWeaponReference != null)
        {
            thirdWeaponReference.action.performed -= ctx => SwitchWeapon(2);
        }

        if (fourthWeaponReference != null)
        {
            fourthWeaponReference.action.performed -= ctx => SwitchWeapon(3);
        }
    }

    private void Update()
    {
        // Tick fire timers for all slots
        for(int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i].fireTimer > 0)
            {
                weaponSlots[i].fireTimer -= Time.deltaTime;
            }
        }

        // Shoot
        if(shootActionReference != null && shootActionReference.action.IsPressed())
        {
            TryShoot();
        }
    }

    // Shooting
    private void TryShoot()
    {
        WeaponSlot slot = GetCurrentWeapon();

        if (slot.weapon == null) return;
        if (slot.loadedShell == null) return;
        if (slot.isReloading) return;
        if (slot.fireTimer > 0f) return;

        if (slot.currentAmmo <= 0)
        {
            StartCoroutine(Reloading(slot));
            return;
        }

        Fire(slot);
    }

    private void Fire(WeaponSlot slot)
    {
        if (shellPrefab == null ||slot.spawnPoint == null)
        {
            Debug.LogWarning("[WeaponManager] shellPrefab or spawnPoint is not assigned.", this);
            return;
        }

        bool isCrit = RollCrit();

        GameObject shellGO = Instantiate(shellPrefab, slot.spawnPoint.position, slot.spawnPoint.rotation);
        Shell shell = shellGO.GetComponent<Shell>();

        if (shell != null)
        {
            shell.Setup(slot.loadedShell, Team.Player, isCrit, critChance, critCoef, pity);
        }

        // Add guidance if the weapon requires it
        if (slot.weapon.isGuided)
        {
            Debug.LogWarning("[WeaponManager] Guided weapons are not yet implemented.", this);
        }

        // Fire rate - seconds per shot
        slot.fireTimer = 1f / slot.weapon.fireRate;

        if (slot.hasMagasine)
        {
            slot.currentAmmo--;
        }


        shootParticle?.Play();

        OnShellChanged?.Invoke(slot.loadedShell);

        if(slot.currentAmmo <= 0)
        {
            StartCoroutine(Reloading(slot));
        }
    }

    // Reloading
    private IEnumerator Reloading(WeaponSlot slot)
    {
        if(slot.isReloading) yield break;

        slot.isReloading = true;
        OnReloadStateChanged?.Invoke(true);

        float elapsed = 0f;
        float duration = slot.weapon.reloadTime;

        while(elapsed < duration)
        {
            elapsed += Time.deltaTime;
            OnReloadProgress?.Invoke(elapsed, duration);
            yield return null;
        }

        slot.currentAmmo = slot.weapon.magazineSize;
        slot.isReloading = false;

        OnReloadStateChanged?.Invoke(false);
        OnReloadProgress?.Invoke(duration, duration);
    }

    // Weapon Switching
    private void SwitchWeapon(int index)
    {
        if(index < 0 ||index >= weaponSlots.Length) return;
        if (weaponSlots[index].weapon == null) return;
        if(index == currentWeaponIndex) return;

        currentWeaponIndex = index;
        OnShellChanged?.Invoke(GetCurrentWeapon().loadedShell);

        Debug.Log($"[WeaponManager] Switched to weapon slot {index}: {GetCurrentWeapon().weapon.weaponName}");
    }

    // Crit
    /// <summary>
    /// Rolls a crit using critChance + pity accumulation.
    /// Pity increases after each non-crit and resets on a crit.
    /// </summary>
    /// <returns>True if the shot is a crit</returns>
    private bool RollCrit()
    {
        int effectiveChance = Mathf.Clamp(critChance + pity, 0, 100);
        bool isCrit = UnityEngine.Random.Range(0, 100) < effectiveChance;

        if (isCrit)
        {
            consecutiveMisses = 0;
            pity = 0;
        }
        else
        {
            consecutiveMisses++;
            // Pity grows by 1%  per consecutive non-crit, capped à 98% so there's always a small chance to miss
            pity = Mathf.Clamp(consecutiveMisses, 0, 98);
        }

        return isCrit;
    }

    /// <summary>Loads a different shell into the current weapon slot at runtime</summary>
    public void SetShell(int slotIndex, ShellSO shell)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return;
        weaponSlots[slotIndex].loadedShell = shell;
        OnShellChanged?.Invoke(shell);
    }

    public int GetCritChance() { return critChance; }
    public float GetCritCoef() { return critCoef; }
    public int GetPity() { return pity; }
    public int GetCurrentAmmo() { return GetCurrentWeapon().currentAmmo; }
    public int GetMagazineSize() { return GetCurrentWeapon().weapon != null ? GetCurrentWeapon().weapon.magazineSize : 0; }
    public bool IsReloading() { return GetCurrentWeapon().isReloading; }
    public WeaponSO GetWeaponSO() { return GetCurrentWeapon().weapon; }
    public ShellSO GetCurrentShell() { return GetCurrentWeapon().loadedShell; }
    public int GetCurrentWeaponIndex() { return currentWeaponIndex; }

}
