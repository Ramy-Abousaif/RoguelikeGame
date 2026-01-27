using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private int abilityIndex;
    private float baseRange = 30;
    private float range;
    private Vector3 startPos;
    private float forceDestroyTimer = 0f;
    private float forceDestroyCapacity = 5f;
    private PhysicsBasedCharacterController player;
    [Header("Impact")]
    [SerializeField] private bool stickable;
    [SerializeField] private float rotXOffset, rotZOffset;
    [SerializeField] private GameObject impactFX;
    [SerializeField, Tooltip("If prefab has an ongoing effect and you want to disable it, assign effect here")] private GameObject ongoingEffect;

    [Header("Stick Fade")]
    [SerializeField] private float stickLifetime = 1.2f;
    [SerializeField] private float shrinkDuration = 0.35f;
    [SerializeField] private float wiggleAmplitude = 6f;
    [SerializeField] private float wiggleFrequency = 18f;
    [SerializeField] private AnimationCurve shrinkCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);
    private bool hasHit = false;
    private float rotYOffset;

    public void Awake()
    {
        startPos = transform.position;
        rotYOffset = transform.eulerAngles.y;
    }

    private void Update()
    {
        if(hasHit)
            return;
            
        forceDestroyTimer += Time.deltaTime;
        if ((Vector3.Distance(startPos, transform.position) >= range) || (forceDestroyTimer >= forceDestroyCapacity))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(hasHit)
            return;
        
        Vector3 contactPoint = transform.position;
        Vector3 contactNormal = Vector3.up;

        if (other.TryGetComponent(out MeshCollider meshCol) && meshCol.sharedMesh != null)
        {
            contactNormal = meshCol.transform.up;
        }

        StickIntoSurface(other, contactPoint, contactNormal);

        if (other.CompareTag("Enemy"))
        {
            if (other.TryGetComponent(out Enemy enemy))
            {
                player.abilities.OnHit(enemy, abilityIndex, true);
                player.CallItemOnHit(enemy);
            }
        }

        hasHit = true;
    }

    private void StickIntoSurface(Collider other, Vector3 contactPoint, Vector3 contactNormal)
    {
        ongoingEffect.SetActive(false);

        transform.position = contactPoint;

        Quaternion alignNormal = Quaternion.FromToRotation(Vector3.up, contactNormal);

        Quaternion prefabOffset = Quaternion.Euler(rotXOffset, rotYOffset, rotZOffset);

        transform.rotation = alignNormal * prefabOffset;

        transform.SetParent(other.transform);

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (impactFX != null)
            Instantiate(impactFX, contactPoint, Quaternion.identity);

        StartCoroutine(StickDecayRoutine());
    }

    private IEnumerator StickDecayRoutine()
    {
        Vector3 startScale = transform.localScale;
        Quaternion baseRotation = transform.rotation;

        // Optional pause before decay
        yield return new WaitForSeconds(stickLifetime);

        float elapsed = 0f;

        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / shrinkDuration);

            // Shrink
            float scale = shrinkCurve.Evaluate(t);
            transform.localScale = startScale * scale;

            // Wiggle (small local rotation noise)
            float wiggle =
                Mathf.Sin(Time.time * wiggleFrequency) * wiggleAmplitude * (1f - t);

            transform.rotation = baseRotation * Quaternion.Euler(0f, 0f, wiggle);

            yield return null;
        }

        Destroy(gameObject);
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