using UnityEngine;

public class Projectile : MonoBehaviour
{
    private int abilityIndex;
    private float baseRange = 30;
    private float damage;
    private float range;
    private Vector3 startPos;
    private float forceDestroyTimer = 0f;
    private float forceDestroyCapacity = 5f;
    private PhysicsBasedCharacterController player;
    [SerializeField] private GameObject impactFX;

    public void Awake()
    {
        startPos = transform.position;
    }

    private void Update()
    {
        forceDestroyTimer += Time.deltaTime;
        if ((Vector3.Distance(startPos, transform.position) >= range) || (forceDestroyTimer >= forceDestroyCapacity))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Instantiate(impactFX, transform.position, Quaternion.identity);
        if (other.transform.TryGetComponent(out Enemy enemy))
        {
            player.abilities.OnHit(enemy, abilityIndex, true);
            player.CallItemOnHit(enemy);
            Destroy(gameObject);
        }
    }

    public void SetDamage(float dmg)
    {
        damage = dmg;
    }

    public void SetRange(float rng)
    {
        range = baseRange + (rng * 10);
    }

    public void SetScale(float scale)
    {
        transform.localScale = new Vector3(scale, scale, scale);
    }

    public void SetAbilityIndex(int i)
    {
        abilityIndex = i;
    }

    public void SetPlayer(PhysicsBasedCharacterController p)
    {
        player = p;
    }
}