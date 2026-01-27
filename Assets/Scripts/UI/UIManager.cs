using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Abilities")]
    [SerializeField] private Image[] abilityBGIcons;
    [SerializeField] private Image[] abilityIcons;
    [SerializeField] private TMP_Text[] abilityCDTexts;

    [Header("Hitmarker")]
    [SerializeField] private HitmarkerUI hitmarker;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void SetupAbilityIcons(int i, float timer, Sprite icon)
    {
        float cdTimer = timer;
        abilityBGIcons[i].sprite = icon;
        abilityIcons[i].sprite = icon;
        if(cdTimer > 0)
        {
            abilityCDTexts[i].text = cdTimer.ToString("N1");   
        }
        else
        {
            abilityCDTexts[i].transform.gameObject.SetActive(false);
        }
    }

    public void UpdateCooldown(int index, float timer, float duration)
    {
        float fill = CooldownToFill(timer, duration);
        abilityIcons[index].fillAmount = fill;

        abilityCDTexts[index].gameObject.SetActive(timer > 0f);
        abilityCDTexts[index].text = timer.ToString("N1");
    }

    public static float CooldownToFill(float cooldownTimer, float cooldownDuration)
    {
        if (cooldownDuration <= 0f)
            return 1f;

        return Mathf.Clamp01(1f - (cooldownTimer / cooldownDuration));
    }

    public void ShowHitmarker()
    {
        hitmarker?.ShowHit();
    }
}
