using UnityEngine;

public class SpiderAI : EnemyAIBase
{
    [Header("Projectile Attack")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;
    [SerializeField] private float projectileSpeed = 25f;
    [SerializeField] private float projectileDamage = 10f;
    [SerializeField] private LayerMask projectileHitMask;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void DoAttack()
    {
        if (anim)
            anim.SetTrigger("Attack");

        if (projectilePrefab == null || shootPoint == null)
            return;

        GameObject obj = PoolManager.Instance.Spawn(projectilePrefab, shootPoint.position, shootPoint.rotation);

        if (obj.TryGetComponent(out Projectile proj))
        {
            proj.SetOwner(selfCharacter);
            proj.SetDamage(projectileDamage);
            proj.SetHitMask(projectileHitMask);
            proj.SetRange(attackRange);
        }

        if (obj.TryGetComponent(out Rigidbody rb))
        {
            rb.linearVelocity = shootPoint.forward * projectileSpeed;
        }
    }
}