using UnityEngine;

public class Enemy : Character
{
    [Header("Effects")]
    public GameObject deathEffect;

    protected override void OnDamageTaken(float damage, bool direct = false)
    {
        // Hit flash
        Flash();
        if(currentHealth > 0f)
        {
            ShowNumber(damage);            
        }
        // Sound
        if(direct)
        {
            Camera.main.GetComponent<CameraShake>().ShakeCamera(5f, 3f, 0.5f);
        }
    }

    protected override void Die()
    {
        currentHealth = 0f;
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
