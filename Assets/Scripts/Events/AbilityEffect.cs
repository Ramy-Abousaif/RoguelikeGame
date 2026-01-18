using System.Collections;
using UnityEngine;

public class AbilitiyEffects : MonoBehaviour
{
    private Abilities.Ability[] _abilities;

    void Awake()
    {
        _abilities = transform.root.GetComponent<Abilities>().abilities;
    }

    public void FireAbility(int index)
    {
        if (_abilities == null || index < 0 || index >= _abilities.Length)
            return;

        Abilities.Ability a = _abilities[index];
        if (a.abilityEmitter == null)
            return;

        a.abilityEmitter.Fire(a);
    }

    public void OnThrow(int index)
    {
        _abilities[index].abilityEmitter.optionalHeldItem.SetActive(false);
    }

    public void Restore(int index)
    {
        _abilities[index].abilityEmitter.optionalHeldItem.SetActive(true);
    }
}