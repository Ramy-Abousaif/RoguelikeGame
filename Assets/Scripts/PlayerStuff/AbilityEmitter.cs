using UnityEngine;

public abstract class AbilityEmitter : MonoBehaviour
{
    [SerializeField] protected Transform firePoint;
    [SerializeField] public GameObject optionalHeldItem;
    [SerializeField] protected int abilityIndex = 0;
    protected PhysicsBasedCharacterController player;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PhysicsBasedCharacterController>();
    }

    public virtual void Fire(Abilities.Ability ability)
    {
        PerformFire(ability);
    }

    protected abstract void PerformFire(Abilities.Ability ability);
}
