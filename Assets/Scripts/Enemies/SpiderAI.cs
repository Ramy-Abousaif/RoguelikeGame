using UnityEngine;

public class SpiderAI : AIBase
{
    [Header("Projectile Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private LayerMask projectileHitMask;

    [Header("Leading")]
    [SerializeField] private bool useLeading = true;
    [SerializeField] private float maxLeadTime = 1.5f;
    [SerializeField] private float aimHeightOffset = 1.0f;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void DoAttack()
    {
        if (projectilePrefab == null || shootPoint == null || target == null)
            return;

        Vector3 shooterPos = shootPoint.position;
        Vector3 targetPos = target.position + Vector3.up * aimHeightOffset;

        Vector3 targetVel = Vector3.zero;
        Rigidbody targetRigidbody = targetCharacter.GetComponent<Rigidbody>();
        targetVel = targetRigidbody.linearVelocity;

        Vector3 dir;

        if (useLeading)
        {
            dir = GetLeadDirection(shooterPos, targetPos, targetVel, projectileSpeed);

            float leadMagnitude = targetVel.magnitude * maxLeadTime;
            Vector3 clampedAimPoint = targetPos + Vector3.ClampMagnitude(targetVel, leadMagnitude) * maxLeadTime;
            dir = (clampedAimPoint - shooterPos).normalized;
        }
        else
        {
            dir = (targetPos - shooterPos).normalized;
        }

        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        GameObject obj = PoolManager.Instance.Spawn(projectilePrefab, shooterPos, rot);

        if (obj.TryGetComponent(out Projectile proj))
        {
            proj.SetOwner(selfCharacter);
            proj.SetDamage(projectileDamage);
            proj.SetHitMask(projectileHitMask);
            proj.SetRange(attackRange);
        }

        if (obj.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = dir * projectileSpeed;
        }
    }
}