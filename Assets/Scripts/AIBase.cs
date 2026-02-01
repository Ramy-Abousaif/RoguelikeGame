using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public abstract class AIBase : MonoBehaviour
{
    public enum State { Chase, Windup, Attack, Stunned }

    [Header("References")]
    [SerializeField] protected Character selfCharacter;
    [SerializeField] protected Transform target;
    [HideInInspector] protected Character targetCharacter;
    [SerializeField] protected NavMeshAgent agent;
    [SerializeField] protected Animator anim;

    [Header("Ranges")]
    [SerializeField] protected float chaseRange = 30f;
    [SerializeField] protected float attackRange = 2.2f;

    [Header("Line of Sight")]
    [SerializeField] protected Transform losOrigin;
    [SerializeField] protected LayerMask losBlockMask;
    [SerializeField] protected float losMaxDistance = 50f;

    [Header("Attack Timing")]
    [SerializeField] protected float windupTime = 0.3f;
    [SerializeField] protected float attackCooldown = 1.2f;

    [Header("Rotation")]
    [SerializeField] protected float faceTargetSpeed = 12f;

    protected State state;
    protected float attackTimer;
    protected bool canAttack => attackTimer <= 0f;

    protected virtual void Awake()
    {
        if (selfCharacter == null) selfCharacter = GetComponent<Character>();
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (anim == null) anim = GetComponentInChildren<Animator>();

        if (losOrigin == null) losOrigin = transform;

        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
            {
                target = p.transform;
                targetCharacter = p.GetComponent<Character>();
            }
        }
    }

    public static Vector3 GetLeadDirection(Vector3 shooterPos, Vector3 targetPos, Vector3 targetVelocity, float projectileSpeed)
    {
        Vector3 toTarget = targetPos - shooterPos;

        float a = Vector3.Dot(targetVelocity, targetVelocity) - projectileSpeed * projectileSpeed;
        float b = 2f * Vector3.Dot(targetVelocity, toTarget);
        float c = Vector3.Dot(toTarget, toTarget);

        // If a is ~0 then speeds are similar; avoid divide by zero
        if (Mathf.Abs(a) < 0.0001f)
            return toTarget.normalized;

        float discriminant = b * b - 4f * a * c;

        // No real solution => can't intercept, just shoot at current position
        if (discriminant < 0f)
            return toTarget.normalized;

        float sqrt = Mathf.Sqrt(discriminant);

        // Two possible times
        float t1 = (-b - sqrt) / (2f * a);
        float t2 = (-b + sqrt) / (2f * a);

        // pick the smallest positive time
        float t = Mathf.Min(t1, t2);
        if (t < 0f) t = Mathf.Max(t1, t2);
        if (t < 0f) return toTarget.normalized;

        Vector3 aimPoint = targetPos + targetVelocity * t;
        Debug.DrawRay(shooterPos, aimPoint - shooterPos, Color.green);
        return (aimPoint - shooterPos).normalized;
    }

    protected virtual void Update()
    {
        if (target == null) return;

        attackTimer -= Time.deltaTime;

        if (state == State.Stunned)
            return;

        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > chaseRange)
        {
            // optional: idle here
            agent.isStopped = true;
            if (anim) anim.SetBool("Moving", false);
            return;
        }

        bool inAttackRange = dist <= attackRange;
        bool hasLOS = HasLineOfSight();

        if (inAttackRange && hasLOS && canAttack && state != State.Windup)
        {
            StartCoroutine(WindupRoutine());
            return;
        }

        if (state != State.Windup)
            UpdateChase();
    }

    protected virtual void UpdateChase()
    {
        state = State.Chase;
        agent.isStopped = false;
        agent.SetDestination(target.position);

        if (anim)
            anim.SetBool("Moving", agent.velocity.sqrMagnitude > 0.1f);
    }

    protected virtual IEnumerator WindupRoutine()
    {
        state = State.Windup;
        agent.isStopped = true;

        if (anim)
        {
            anim.SetBool("Moving", false);
            anim.SetTrigger("Windup");
        }

        float t = 0f;
        while (t < windupTime)
        {
            t += Time.deltaTime;
            FaceTarget();
            yield return null;
        }

        // Recheck LOS/range before committing
        float dist = Vector3.Distance(transform.position, target.position);
        if (dist <= attackRange && HasLineOfSight())
        {
            state = State.Attack;
            DoAttack();
            attackTimer = attackCooldown;
        }

        state = State.Chase;
    }

    protected virtual bool HasLineOfSight()
    {
        Vector3 origin = losOrigin.position;
        Vector3 dir = (target.position + Vector3.up * 1f) - origin;

        if (dir.sqrMagnitude < 0.01f) return true;
        if (dir.magnitude > losMaxDistance) return false;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dir.magnitude, losBlockMask))
        {
            // blocked by environment
            return false;
        }

        return true;
    }

    protected virtual void FaceTarget()
    {
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion rot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, rot, Time.deltaTime * faceTargetSpeed);
    }

    public virtual void ApplyStun(float duration)
    {
        if (gameObject.activeInHierarchy)
            StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        state = State.Stunned;
        agent.isStopped = true;

        if (anim)
        {
            anim.SetBool("Moving", false);
            anim.SetTrigger("Stunned");
        }

        yield return new WaitForSeconds(duration);

        state = State.Chase;
    }

    protected abstract void DoAttack();
}