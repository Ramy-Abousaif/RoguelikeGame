using UnityEngine;

public class TransformEmitter : AbilityEmitter
{
    [SerializeField] private CharacterForm targetForm;
    [SerializeField] private float blendSpeed = 0.25f;
    [SerializeField] private float duration = 8f;

    public CharacterForm TargetForm => targetForm;
    public float BlendSpeed => blendSpeed;

    protected override void PerformFire(Abilities.Ability ability)
    {
        var abilities = player.GetComponent<Abilities>();
        abilities.ApplyTimedForm(targetForm, blendSpeed, duration, blendSpeed);
    }
}