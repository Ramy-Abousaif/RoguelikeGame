using UnityEngine;

public class PlayerHitBox : MonoBehaviour
{
    public int abilityIndex = 0;
    public PhysicsBasedCharacterController player;
    public GameObject hitEffect;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PhysicsBasedCharacterController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                player.abilities.OnHit(enemy, abilityIndex);
                Vector3 hitPoint = other.ClosestPoint(transform.position);
                Instantiate(hitEffect, hitPoint, Quaternion.identity);
                player.CallItemOnHit(enemy);
            }
        }
    }
}
