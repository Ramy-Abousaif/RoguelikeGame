using UnityEngine;

public class SpiderController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform bodyPivot;
    [SerializeField] private Transform BRFoot;
    [SerializeField] private Transform BLFoot;
    [SerializeField] private Transform FRFoot;
    [SerializeField] private Transform FLFoot;

    [Header("Body Settings")]
    [SerializeField] private float bodyHeightOffset = 1.4f;
    [SerializeField] private float bodyPositionSpring = 25f;
    [SerializeField] private float bodyPositionDamping = 6f;
    [SerializeField] private float bodyRotationSpring = 20f;
    [SerializeField] private float bodyRotationDamping = 5f;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotationSpeed = 180f;

    [Header("AI Wander")]
    [SerializeField] private float wanderInterval = 2f;
    [SerializeField] private float obstacleCheckDistance = 1.2f;
    [SerializeField] private LayerMask obstacleMask = ~0;

    private Vector3 moveDirection;
    private float wanderTimer;

    // Second-order dynamics state
    private Vector3 bodyPosVelocity;
    private Vector3 bodyUpVelocity;
    private Vector3 smoothedUp = Vector3.up;

    private void Start()
    {
        wanderTimer = Random.Range(0f, wanderInterval);
        PickNewDirection();
    }

    private void Update()
    {
        UpdateMovement();
        UpdateBodyPosition();
        UpdateBodyRotation();
    }

    // ---------------- MOVEMENT ----------------

    private void UpdateMovement()
    {
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            wanderTimer = wanderInterval;
            PickNewDirection();
        }

        // Obstacle avoidance
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f,
            -transform.forward, obstacleCheckDistance, obstacleMask))
        {
            PickNewDirection();
        }

        // Rotate
        if (moveDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        // Move
        transform.position += -transform.forward * moveSpeed * Time.deltaTime;
    }

    private void PickNewDirection()
    {
        Vector2 rnd = Random.insideUnitCircle.normalized;
        moveDirection = new Vector3(rnd.x, 0f, rnd.y);
    }

    // ---------------- BODY POSITION ----------------

    private void UpdateBodyPosition()
    {
        float avgFootHeight =
            (BRFoot.position.y +
             BLFoot.position.y +
             FRFoot.position.y +
             FLFoot.position.y) * 0.25f;

        Vector3 targetLocalPos = bodyPivot.localPosition;
        targetLocalPos.y = avgFootHeight + bodyHeightOffset;

        bodyPivot.localPosition = Vector3.SmoothDamp(
            bodyPivot.localPosition,
            targetLocalPos,
            ref bodyPosVelocity,
            1f / bodyPositionSpring,
            Mathf.Infinity,
            Time.deltaTime
        );
    }

    // ---------------- BODY ROTATION ----------------

    private void UpdateBodyRotation()
    {
        Vector3 v1 = FRFoot.position - BLFoot.position;
        Vector3 v2 = BRFoot.position - FLFoot.position;

        Vector3 groundNormal = Vector3.Cross(v1, v2).normalized;
        if (groundNormal.sqrMagnitude < 0.001f)
            groundNormal = Vector3.up;

        smoothedUp = Vector3.SmoothDamp(
            smoothedUp,
            groundNormal,
            ref bodyUpVelocity,
            1f / bodyRotationSpring,
            Mathf.Infinity,
            Time.deltaTime
        );

        Quaternion targetRotation =
            Quaternion.LookRotation(transform.forward, smoothedUp);

        bodyPivot.rotation = Quaternion.Slerp(
            bodyPivot.rotation,
            targetRotation,
            Time.deltaTime * bodyRotationSpring
        );

        Debug.DrawRay(bodyPivot.position, smoothedUp, Color.red);
    }
}