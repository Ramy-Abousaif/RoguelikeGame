using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.VFX;

public class RoarEffect : MonoBehaviour
{
    private CameraShake shake;
    
    [SerializeField]private VisualEffect sparksEffect;

    private void Awake()
    {
        shake = Camera.main.GetComponent<CameraShake>();
    }

    private void StartAOEEffect()
    {
        sparksEffect.Play();
        shake.ShakeCamera(1f, 1f, 0.3f);
    }
}
