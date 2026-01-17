using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using System.Collections;

public abstract class Character : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected float maxHealth = 100f;
    [SerializeField] protected float currentHealth;

    [Header("Defense / Stats")]
    [SerializeField] protected float armor = 0f;

    [Header("Flash Settings")]
    [SerializeField] private float flashDuration = 0.1f;
    [SerializeField] private float flashIntensity = 2f;

    private Renderer[] renderers;
    private MaterialPropertyBlock mpb;
    private Coroutine flashCoroutine;

    private static readonly int FlashColorID  = Shader.PropertyToID("_FlashColor");
    private static readonly int FlashAmountID = Shader.PropertyToID("_FlashAmount");

    [Header("Effects")]
    [SerializeField] protected GameObject numberEffect;

    protected Dictionary<System.Type, StatusEffect> activeEffects = new();

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
        renderers = GetComponentsInChildren<Renderer>(true);
    }

    public void Flash()
    {
        if (flashCoroutine != null)
            StopCoroutine(flashCoroutine);

        flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        float timer = flashDuration;

        // Initialize flash (set color + amount = 1)
        foreach (var rend in renderers)
        {
            int matCount = rend.sharedMaterials.Length;
            for (int i = 0; i < matCount; i++)
            {
                MaterialPropertyBlock mpb = new();
                rend.GetPropertyBlock(mpb, i);

                mpb.SetFloat(FlashAmountID, 1f);

                rend.SetPropertyBlock(mpb, i);
            }
        }

        // Fade flash out
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            float t = Mathf.Clamp01(timer / flashDuration); // 1 → 0

            foreach (var rend in renderers)
            {
                int matCount = rend.sharedMaterials.Length;
                for (int i = 0; i < matCount; i++)
                {
                    MaterialPropertyBlock mpb = new();
                    rend.GetPropertyBlock(mpb, i);

                    mpb.SetFloat(FlashAmountID, t);
                    rend.SetPropertyBlock(mpb, i);
                }
            }

            yield return null;
        }

        // Ensure fully reset
        foreach (var rend in renderers)
        {
            int matCount = rend.sharedMaterials.Length;
            for (int i = 0; i < matCount; i++)
            {
                MaterialPropertyBlock mpb = new();
                rend.GetPropertyBlock(mpb, i);

                mpb.SetFloat(FlashAmountID, 0f);
                rend.SetPropertyBlock(mpb, i);
            }
        }

        flashCoroutine = null;
    }

    protected virtual void Update()
    {
        float dt = Time.deltaTime;

        foreach (var effect in activeEffects.Values.ToList())
        {
            effect.Update(dt);

            if (effect.IsExpired)
                activeEffects.Remove(effect.GetType());
        }
    }

    public virtual void ApplyStatusEffect(StatusEffect effect, int stacks, float duration)
    {
        var type = effect.GetType();

        if (activeEffects.TryGetValue(type, out var existing))
        {
            existing.Refresh(stacks, duration);
        }
        else
        {
            effect.Initialize(this, stacks, duration);
            activeEffects.Add(type, effect);
        }
    }

    /// <summary>
    /// Calculate and apply damage to this character
    /// </summary>
    /// <param name="damage">Raw damage</param>
    /// <param name="direct">True if bypasses effects like camera shake</param>
    public virtual void TakeDamage(float damage, bool direct = false)
    {
        float finalDamage = CalculateDamage(damage);

        currentHealth -= finalDamage;

        OnDamageTaken(finalDamage, direct);

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public virtual void Heal(float amount)
    {
        if(currentHealth < maxHealth)
        {
            onHeal(amount);            
        }
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    protected virtual void onHeal(float amount)
    {
        // Optional: override to show heal numbers, effects, sound, etc.
    }

    /// <summary>
    /// Override this in subclasses for custom damage behavior
    /// </summary>
    protected virtual void OnDamageTaken(float damage, bool direct = false)
    {
        // Optional: override to show damage numbers, hit flashes, sound, etc.
    }

    /// <summary>
    /// Override this in subclasses for custom death behavior
    /// </summary>
    protected abstract void Die();

    protected virtual float CalculateDamage(float damage)
    {
        return damage * (100f / (100f + armor));
    }

    protected virtual void ShowNumber(float value, bool isHeal = false)
    {
        if (numberEffect != null)
        {
            GameObject dmgNumber = Instantiate(numberEffect, transform.position, Quaternion.identity);
            var text = dmgNumber.GetComponentInChildren<TextMeshPro>();
            if (text != null)
            {
                text.color = isHeal ? Color.green : Color.red;
                text.text = Mathf.RoundToInt(value).ToString();
            }
        }
    }

    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;
}
