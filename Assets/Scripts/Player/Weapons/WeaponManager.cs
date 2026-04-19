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
        [Tooltip("Ammo currently loaded defaults to weapon.defaultShell.")]
        public ShellSO loadedShell;
        [Tooltip("Left Transform from which the shell is spawned.")]
        public Transform leftSpawnPoint;
        [Tooltip("Right Transform from which the shell is spawned.")]
        public Transform rightSpawnPoint;
        [Tooltip("If false, weapon never consumes ammo.")]
        public bool hasMagasine;

        [HideInInspector] public int    currentAmmo;
        [HideInInspector] public bool   isReloading;
        [HideInInspector] public float  fireTimer;
    }

    [Header("Weapon Slots")]
    [SerializeField] private WeaponSlot[] weaponSlots = new WeaponSlot[4];
    [Space(10)]

    [Header("Projectile")]
    [SerializeField] private GameObject shellPrefab;
    [Space(10)]

    [Header("VFX")]
    [SerializeField] private ParticleSystem shootParticle;
    [Space(10)]

    [Header("Pity (Debug)")]
    [SerializeField] private int pity = 0;
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
    public event Action<AudioClip> OnPlayFireSound;
    #endregion

    private int currentWeaponIndex = 0;
    private int consecutiveMisses = 0;

    private System.Random soundRng;
    private System.Random critRng;

    private WeaponSlot GetCurrentWeapon()
    {
        return weaponSlots[currentWeaponIndex];
    }

    private void Awake()
    {
        soundRng = SeedManager.GetRNG("WeaponSound");
        critRng = SeedManager.GetRNG("Crit");

        foreach (WeaponSlot slot in weaponSlots)
        {
            if (slot.weapon == null) continue;

            // Load default ammo if none assigned
            if(slot.loadedShell == null) slot.loadedShell = slot.weapon.defaultShell;

            slot.currentAmmo = slot.weapon.magazineSize;
            slot.hasMagasine = slot.weapon.hasMagazine;
            slot.isReloading = false;
            slot.fireTimer   = 0;
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
        bool isShooting = shootActionReference != null && shootActionReference.action.IsPressed();

        // Tick fire timers for all slots
        // Tick fire timers for all slots
        for (int i = 0; i < weaponSlots.Length; i++)
        {
            if (weaponSlots[i].fireTimer > 0)
            {
                weaponSlots[i].fireTimer -= Time.deltaTime;
            }
        }

        // Shoot
        if (isShooting)
        {
            TryShoot();
        }
    }

    // Shooting
    private void TryShoot()
    {
        WeaponSlot slot = GetCurrentWeapon();

        if (slot.weapon         == null) return;
        if (slot.loadedShell    == null) return;
        if (slot.isReloading)            return;
        if (slot.fireTimer > 0f)         return;

        if (slot.currentAmmo <= 0 && slot.hasMagasine)
        {
            StartCoroutine(Reloading(slot));
            return;
        }

        bool isCrit = RollCrit(slot.weapon);

        if (slot.leftSpawnPoint != null) Fire(slot, slot.leftSpawnPoint, isCrit);
        if (slot.rightSpawnPoint != null) Fire(slot, slot.rightSpawnPoint, isCrit);

        slot.fireTimer = 1f / slot.weapon.fireRate;
        if (slot.hasMagasine) slot.currentAmmo--;

        if(shootParticle != null)
        {
            shootParticle?.Play();
        }

        OnShellChanged?.Invoke(slot.loadedShell);

        if(slot.currentAmmo <= 0 && slot.hasMagasine)
        {
            StartCoroutine(Reloading(slot));
        }
    }

    private void Fire(WeaponSlot slot, Transform spawnPoint, bool isCrit)
    {
        if(shellPrefab == null)
        {
            Debug.LogWarning("[WeaponManager] shellPrefab not assigned.", this);
            return;
        }

        // SFX shooting
        if(slot.weapon.fireSounds != null && slot.weapon.fireSounds.Count > 0)
        {
            int idx = soundRng.Next(0, slot.weapon.fireSounds.Count);
            OnPlayFireSound?.Invoke(slot.weapon.fireSounds[idx]);
        }

        GameObject shellGO = Instantiate(shellPrefab, spawnPoint.position, spawnPoint.rotation);
        Shell shell = shellGO.GetComponent<Shell>();

        if(shell != null)
        {
            shell.Setup(slot.loadedShell, Team.Player, isCrit, slot.weapon.critChance, slot.weapon.critCoef, pity);
        }

        if (slot.weapon.isGuided)
        {
            /*
            GuidedShell guided = shellGO.AddComponent<GuidedShell>();
            guided.Init(slot.weapon.guidedTurnRate, slot.weapon.guidedDetectionRadius, Team.Player);
            */
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
        pity = 0; // Resets pity on switch
        OnShellChanged?.Invoke(GetCurrentWeapon().loadedShell);

        Debug.Log($"[WeaponManager] Switched to weapon slot {index}: {GetCurrentWeapon().weapon.weaponName}");
    }

    // Crit
    /// <summary>
    /// Rolls a crit using critChance + pity accumulation.
    /// Pity increases after each non-crit and resets on a crit.
    /// </summary>
    /// <returns>True if the shot is a crit</returns>
    private bool RollCrit(WeaponSO weapon)
    {
        int effectiveChance = Mathf.Clamp(weapon.critChance + pity, 0, 100);
        bool isCrit = SeedManager.Range(0, 100, critRng) < effectiveChance;

        if (isCrit) { consecutiveMisses = 0; pity = 0; }
        else        { consecutiveMisses++;   pity = Mathf.Clamp(consecutiveMisses, 0, 98); }

        return isCrit;
    }

    /// <summary>Loads a different shell into the current weapon slot at runtime</summary>
    public void SetShell(int slotIndex, ShellSO shell)
    {
        if (slotIndex < 0 || slotIndex >= weaponSlots.Length) return;
        weaponSlots[slotIndex].loadedShell = shell;
        OnShellChanged?.Invoke(shell);
    }

    /// <summary>Returns the chance of making a critical hit.</summary>
    public int GetCritChance() { return GetCurrentWeapon().weapon != null ? GetCurrentWeapon().weapon.critChance : 0; }
    /// <summary>Returns the crit coefficient.</summary>
    public float GetCritCoef() { return GetCurrentWeapon().weapon != null ? GetCurrentWeapon().weapon.critCoef : 1f; }
    public int GetPity() { return pity; }
    public int GetCurrentAmmo() { return GetCurrentWeapon().currentAmmo; }
    public int GetMagazineSize() { return GetCurrentWeapon().weapon != null ? GetCurrentWeapon().weapon.magazineSize : 0; }
    public bool IsReloading() { return GetCurrentWeapon().isReloading; }
    public WeaponSO GetWeaponSO() { return GetCurrentWeapon().weapon; }
    public ShellSO GetCurrentShell() { return GetCurrentWeapon().loadedShell; }
    public int GetCurrentWeaponIndex() { return currentWeaponIndex; }

}
