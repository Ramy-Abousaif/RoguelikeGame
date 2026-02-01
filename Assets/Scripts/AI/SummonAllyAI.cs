using UnityEngine;

public class SummonAllyAI : AIBase
{
    [Header("Owner")]
    public PhysicsBasedCharacterController owner;
    public Ally allyComponent;

    private float lifetime = 10f;
    private float timer;
    private bool isDestroyed = false;

    public void Initialize(float summonDuration)
    {
        owner = GameObject.FindGameObjectWithTag("Player").GetComponent<PhysicsBasedCharacterController>();
        allyComponent = GetComponent<Ally>();
        lifetime = summonDuration;
        timer = 0f;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        timer += Time.deltaTime;
        if (timer >= lifetime && !isDestroyed)
        {
            isDestroyed = true;
            allyComponent.Fade(false);
        }
    }

    protected override void DoAttack()
    {
        // Summon ally does not attack
    }
}
