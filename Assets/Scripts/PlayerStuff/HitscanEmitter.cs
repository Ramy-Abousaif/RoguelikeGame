using UnityEngine;

public class HitscanEmitter : AbilityEmitter
{
    [Header("Hitscan")]
    [SerializeField] private LayerMask hitMask;
    [SerializeField] private float baseRange = 100f;

    protected override void PerformFire(Abilities.Ability ability)
    {
        if (firePoint == null)
            return;

        float range = baseRange * ability.currentAbilityRange;
        Ray ray = new Ray(firePoint.position, firePoint.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, range, hitMask))
        {
            if (hit.collider.TryGetComponent(out Enemy enemy))
            {
                player.abilities.OnHit(enemy, abilityIndex);
                player.CallItemOnHit(enemy);
            }

            // Optional: impact VFX
            // Instantiate(impactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }

        // Optional: tracer / muzzle flash
    }
}