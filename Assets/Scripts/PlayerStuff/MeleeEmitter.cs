using Unity.Mathematics;
using UnityEngine;

public class MeleeEmitter : AbilityEmitter
{
    [Header("Melee")]
    [SerializeField] private LayerMask enemyMask;

    protected override void PerformFire(Abilities.Ability ability)
    {
        if (firePoint == null)
            return;

        float radius = ability.currentAbilityRange;
        Vector3 center = firePoint.position;

        if(ability.abilityEmitter.abilityEffect != null)
        {
            GameObject fx = Instantiate(ability.abilityEmitter.abilityEffect, center, quaternion.identity);
            fx.transform.localScale = new Vector3(radius, radius, radius);
        }
        else
        {
            Debug.Log("No Effect Prefab");
        }

        Collider[] hits = Physics.OverlapSphere(center, radius, enemyMask);

        foreach (Collider col in hits)
        {
            if (col.TryGetComponent(out Enemy enemy))
            {
                player.abilities.OnHit(enemy, abilityIndex);
                player.CallItemOnHit(enemy);
            }
        }
    }
}