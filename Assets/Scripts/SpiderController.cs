using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpiderController : MonoBehaviour
{
    public GameObject root;
    public Transform BRFoot, BLFoot, FRFoot, FLFoot;
    public float offset = 1.38f;
    public float smoothness = 5f;
    public float speed = 12f;
    public float rotSpeed = 12f;
    private Vector3 lastBodyUp;
    
    [Header("AI Movement")]
    public float wanderChangeInterval = 2f;
    public float wanderRadius = 8f;
    public float obstacleDetectDistance = 1.2f;
    public LayerMask obstacleMask = ~0;

    private float wanderTimer = 0f;
    private Vector3 aiMoveDir;

    private void Start()
    {
        lastBodyUp = root.transform.parent.forward;
        wanderTimer = Random.Range(0f, wanderChangeInterval);
        aiMoveDir = transform.forward;
    }

    // Update is called once per frame
    void Update()
    {

        float avgHeight = (BRFoot.position.y + BLFoot.position.y + FRFoot.position.y + FLFoot.position.y) / 4;
        root.transform.parent.position = new Vector3(root.transform.parent.position.x, avgHeight + offset, root.transform.parent.position.z);

        Vector3 v1 = FRFoot.position - BLFoot.position;
        Vector3 v2 = BRFoot.position - FLFoot.position;
        Vector3 normal = Vector3.Cross(v1, v2).normalized;
        Vector3 up = Vector3.Lerp(lastBodyUp, normal, 1f / (float)(smoothness + 1));
        root.transform.parent.forward = up;
        root.transform.parent.rotation = Quaternion.LookRotation(transform.forward, up);
        Debug.DrawRay(root.transform.parent.position, up * 100, Color.red);
        lastBodyUp = root.transform.parent.forward;

        // Simple AI wandering: change direction on a timer and avoid obstacles
        wanderTimer -= Time.deltaTime;
        if (wanderTimer <= 0f)
        {
            wanderTimer = wanderChangeInterval;
            Vector2 rnd = Random.insideUnitCircle.normalized * Random.Range(0.3f, 1f);
            aiMoveDir = new Vector3(rnd.x, 0f, rnd.y);
        }

        // Obstacle avoidance: if something is in front, pick a new random direction
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;
        if (Physics.Raycast(rayOrigin, transform.forward, out hit, obstacleDetectDistance, obstacleMask))
        {
            Vector2 rnd = Random.insideUnitCircle.normalized;
            aiMoveDir = new Vector3(rnd.x, 0f, rnd.y);
            wanderTimer = wanderChangeInterval; // reset timer after avoidance
        }

        Vector3 moveDir = aiMoveDir;
        moveDir.y = 0f;
        if (moveDir.sqrMagnitude > 0.0001f)
        {
            moveDir.Normalize();
            Quaternion rot = Quaternion.LookRotation(moveDir, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, rotSpeed * Time.deltaTime);
            transform.position += moveDir * speed * Time.deltaTime;
        }
    }
}
