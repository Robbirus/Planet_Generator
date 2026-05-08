using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    [Header("Laser")]
    [SerializeField] private Transform spawpoint;
    [SerializeField] private float maxRange = 500f;
    [SerializeField] private float damagePerSec = 15f;
    [SerializeField] private LayerMask hitLayers;
    [Space(10)]

    [Header("Effect applied on contact")]
    [Tooltip("Must be LASER TypeEffect StatusEffectSO.")]
    [SerializeField] private StatusEffectSO effectData;
    [Space(10)]

    [Header("Visuals")]
    [SerializeField] private Color beamColor = new Color(1f, 0.15f, 0.15f, 1f);
    [SerializeField] private float beamWidth = 0.2f;
    [SerializeField] private float impactWidth = 0.6f;

    private LineRenderer beam;
    private ParticleSystem impactParticles;

    private void Awake()
    {
        beam = GetComponent<LineRenderer>();
        beam.positionCount = 2;
        beam.useWorldSpace = true;
        beam.startWidth = beamWidth;
        beam.endWidth = beamWidth;
        beam.startColor = beamColor;
        beam.endColor = new Color(beamColor.r, beamColor.g, beamColor.b, 0.3f);
        beam.enabled = false;

        impactParticles = GetComponentInChildren<ParticleSystem>();
    }

    public void Fire()
    {
        beam.enabled = true;

        Vector3 origin = spawpoint.position;
        Vector3 direction = spawpoint.forward;

        beam.SetPosition(0, origin);

        if(Physics.Raycast(origin, direction, out RaycastHit hit, maxRange, hitLayers))
        {
            beam.SetPosition(1, hit.point);

            // Damage over time
            IDamageable target = hit.collider.GetComponentInParent<IDamageable>();
            if(target != null)
            {
                target.TakeDamage(damagePerSec * Time.deltaTime, Color.yellow, true, false);
            }

            // Apply LASER status effect (DoT fire) via StatusEffectHandler
            HealthComponent hc = hit.collider.GetComponentInParent<HealthComponent>();
            if(hc != null && effectData != null)
            {
                StatusEffectHandler handler = hc.GetComponent<StatusEffectHandler>();
                handler?.Apply(TypeEffect.LASER, Team.Player, effectData);
            }

            // Impact particles
            if(impactParticles != null)
            {
                impactParticles.transform.position = hit.point;
                if (!impactParticles.isPlaying) impactParticles.Play();
            }

            // Widen beam at impact point
            beam.endWidth = impactWidth;
        }
        else
        {
            beam.SetPosition(1, origin + direction * maxRange);
            beam.endWidth = beamWidth;
            impactParticles?.Stop();
        }
    }

    public void StopFire()
    {
        beam.enabled = false;
        impactParticles?.Stop();
    }

    public void SetEffectData(StatusEffectSO data)
    {
        this.effectData = data;
    }
}
