using UnityEngine;

public class TestHitbox : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(10, true);
            }
        }
        if(other.CompareTag("Player"))
        {
            PhysicsBasedCharacterController player = other.GetComponent<PhysicsBasedCharacterController>();
            if(player != null)
            {
                player.TakeDamage(10, true);
            }
        }
    }
}
