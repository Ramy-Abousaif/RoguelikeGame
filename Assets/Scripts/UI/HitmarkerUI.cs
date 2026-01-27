using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HitmarkerUI : MonoBehaviour
{
    [SerializeField] private Image hitmarkerImage;
    [SerializeField] private float popScale = 1.3f;
    [SerializeField] private float popTime = 0.05f;
    [SerializeField] private float fadeTime = 0.1f;

    private Coroutine _routine;

    void Start()
    {
        Color c = hitmarkerImage.color;
        hitmarkerImage.color = new Color(c.r, c.g, c.b, 0f);
    }

    public void ShowHit()
    {
        if (_routine != null)
            StopCoroutine(_routine);

        _routine = StartCoroutine(HitRoutine());
    }

    private IEnumerator HitRoutine()
    {
        RectTransform rt = hitmarkerImage.rectTransform;
        Color c = hitmarkerImage.color;

        // Reset
        rt.localScale = Vector3.one;
        hitmarkerImage.color = new Color(c.r, c.g, c.b, 1f);

        // Pop
        float t = 0f;
        while (t < popTime)
        {
            t += Time.deltaTime;
            float s = Mathf.Lerp(1f, popScale, t / popTime);
            rt.localScale = Vector3.one * s;
            yield return null;
        }

        // Settle
        rt.localScale = Vector3.one;

        // Fade out
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(1f, 0f, t / fadeTime);
            hitmarkerImage.color = new Color(c.r, c.g, c.b, a);
            yield return null;
        }

        hitmarkerImage.color = new Color(c.r, c.g, c.b, 0f);
    }
}