using UnityEngine;

/// <summary>
/// Projectile that travels in a straight line and calls HandleHit on the first IDamageable it hits.
/// GuidedShell component can be added at runtime to enable homing behaviour.
/// </summary>
public class Shell : MonoBehaviour
{
    private float       standardDamage;
    private float       durableDamage;
    private int         critChance;
    private float       critCoef;
    private bool        isCrit = false;
    private int         pity;
    private ArmorType   armorPen;
    private int         velocity;
    private float       lifeTime;
    private TypeEffect  typeEffect;
    private Color color;

    private WeaponType  type;
    private Team        owner;

    // Direction is set in Setup so GuidedShell can override it each frame
    private Vector3 direction;

    // Init
    /// <summary>
    /// Called by WeaponManager immediately after instantiation.
    /// Crit values are passed in directly - no GameManager chain needed.
    /// </summary>
    public void Setup(ShellSO shellData, Team team, bool isCrit, int critChance, float critCoef, int pity)
    {
        this.armorPen = shellData.armorPen;
        this.standardDamage = shellData.standardDamage;
        this.durableDamage = shellData.durableDamage;
        this.velocity = shellData.velocity;
        this.lifeTime = shellData.lifeTime;
        this.type = shellData.weaponType;
        this.typeEffect = shellData.typeEffect;
        this.color = shellData.color;

        this.isCrit = isCrit;
        this.critChance = critChance;
        this.critCoef = critCoef;
        this.pity = pity;
        this.owner = team;

        direction = transform.forward;

        // Trail color
        TrailRenderer trail = GetComponent<TrailRenderer>();
        if (trail != null)
        {
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { new GradientColorKey(this.color, 0f), new GradientColorKey(Color.white, 1f) },
                new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
            );

            gameObject.GetComponent<TrailRenderer>().colorGradient = gradient;
        }

        Destroy(gameObject, lifeTime);
    }

    // Movement
    private void FixedUpdate()
    {
        Vector3 nextPos = transform.position + direction * velocity * Time.deltaTime;
        Vector3 move = nextPos - transform.position;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, move.magnitude))
        {
            if (hit.collider.GetComponentInParent<IDamageable>() is IDamageable target)
            {
                target.HandleHit(this, hit);

                Destroy(gameObject);

                return;
            }
        }

        transform.position = nextPos;
    }

    // Damage computation
    /// <summary>
    /// Returns final damage considering durable damage, crit and armor penetration.
    /// </summary>
    public float GetFinalDamage(ArmorType targetArmor, HealthComponent enemyPart)
    {
        if (targetArmor == ArmorType.INDESTRUCTIBLE) return 0f;

        // Base damage formula
        float baseDamage = standardDamage * (1 - enemyPart.GetDurability() / 100f) + (durableDamage * enemyPart.GetDurability() / 100f);
        
        // Crit only for player shells
        if (this.owner == Team.Player && isCrit)
        {
            baseDamage *= critCoef;
        }

        // Armor reduction
        baseDamage *= PenetrationMultiplier(armorPen, targetArmor);

        return baseDamage;
    }

    /// <summary>Legacy overload - called without armor context </summary>
    public float GetFinalDamage()
    {
        float baseDamage = standardDamage * (1 - durableDamage / 100f) + (durableDamage * durableDamage / 100f);
        return this.owner == Team.Player && isCrit ? baseDamage * critCoef : baseDamage;
    }

    // Armor reduction
    /// <summary>
    /// Returns a [0-1] damage multiplier based on shell pen vs target armor.
    /// Full damage if pen >= armor, partial otherwise.
    /// </summary>
    public static float PenetrationMultiplier(ArmorType shellPen, ArmorType targetArmor)
    {
        if (targetArmor == ArmorType.INDESTRUCTIBLE) return 0f;

        int pen     = (int)shellPen;
        int armor   = (int)targetArmor;

        if (pen >= armor)
        {
            return 1f;
        }
        else
        {
            // Example: pen 2 vs armor 4 -> 50% damage
            return 1f - (armor - pen) * 0.25f;
        }
    }

    #region Getter
    public ArmorType GetArmorPenetration()
    {
        return this.armorPen;
    }

    public TypeEffect GetTypeEffect()
    {
        return this.typeEffect;
    }

    public Team GetTeam()
    {
        return this.owner;
    }

    public bool IsCrit()
    {
        return this.isCrit;
    }

    public int GetCritChance()
    {
        return critChance;
    }

    public float GetCritCoef()
    {
        return critCoef;
    }

    public int GetPity()
    {
        return pity;
    }

    public WeaponType GetWeaponType()
    {
        return this.type;
    }
    #endregion

    #region Setter
    public void SetArmorPen(ArmorType newPen)
    {
        this.armorPen = newPen;
    }

    public void SetCritChance(int chance)
    {
        this.critChance = chance;
    }

    public void SetCritCoef(float coef)
    {
        this.critCoef = coef;
    }

    public void SetStandardDamage(float damage)
    {
        this.standardDamage = damage;
    }

    public void SetDurableDamage(float damage)
    {
        this.durableDamage = damage;
    }
    #endregion
}