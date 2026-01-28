using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class Projectile : MonoBehaviour
{
    private int abilityIndex;
    private float baseRange = 30;
    private float range;
    private Vector3 startPos;
    private float forceDestroyTimer = 0f;
    private float forceDestroyCapacity = 5f;
    private PhysicsBasedCharacterController player;
    private MaterialPropertyBlock _mpb;
    private Renderer[] _renderers;

    [Header("Impact")]
    [SerializeField] private bool stickable;
    [SerializeField] private float rotXOffset, rotZOffset;
    [SerializeField] private GameObject impactFX;
    [SerializeField, Tooltip("If prefab has an ongoing effect and you want to disable it, assign effect here")] private GameObject ongoingEffect;

    [Header("Dissolve")]
    [SerializeField] private float dissolveDelay = 1.0f;
    [SerializeField] private float dissolveDuration = 0.4f;
    [SerializeField] private VisualEffect[] vfx;

    private bool hasHit = false;
    private float rotYOffset;

    public void Awake()
    {
        startPos = transform.position;
        rotYOffset = transform.eulerAngles.y;
        _renderers = GetComponentsInChildren<Renderer>();
        _mpb = new MaterialPropertyBlock();
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

        if (other.TryGetComponent(out Collider col))
        {
            Debug.Log(col.transform.gameObject.name);
            contactNormal = col.transform.forward;
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
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (impactFX != null)
            Instantiate(impactFX, contactPoint, Quaternion.identity);

        StartCoroutine(DissolveRoutine());
    }

    private IEnumerator DissolveRoutine()
    {
        yield return new WaitForSeconds(dissolveDelay);

        float elapsed = 0f;

        if(vfx.Length > 0)
        {
            for(int i = 0; i < vfx.Length; i++)
            {
                vfx[i].Play();
                vfx[i].SetFloat("Duration", dissolveDuration);
            }
        }

        while (elapsed < dissolveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dissolveDuration);

            foreach(Renderer r in _renderers)
            {
                r.GetPropertyBlock(_mpb);
                _mpb.SetFloat("_DissolveAmount", t);
                r.SetPropertyBlock(_mpb);
            }

            yield return null;
        }

        if(vfx.Length > 0)
        {
            for(int i = 0; i < vfx.Length; i++)
            {
                vfx[i].Stop();
            }
        }

        yield return new WaitForSeconds(0.5f);

        Destroy(gameObject);
    }

    public void SetRange(float rng)
    {
        range = baseRange + (rng * 10);
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