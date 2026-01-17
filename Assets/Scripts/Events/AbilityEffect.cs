using System.Collections;
using UnityEngine;

public class AbilitiyEffects : MonoBehaviour
{
    private Abilities.Ability[] abilities;

    void Awake()
    {
        abilities = transform.root.GetComponent<Abilities>().abilities;
    }

    public void PlayEffect(AnimationEvent animationEvent)
    {
        int index = animationEvent.intParameter;
        float disableTime = animationEvent.floatParameter;
        StartCoroutine(SpawnAbilityEffect(index, disableTime));
    }

    IEnumerator SpawnAbilityEffect(int index, float disableTime)
    {
        Abilities.Ability ability = abilities[index];
        Transform spawnPoint = ability.effectSpawnPoint;
        ability.effectPrefab.transform.eulerAngles = new Vector3(spawnPoint.transform.eulerAngles.x, spawnPoint.transform.eulerAngles.y, spawnPoint.transform.eulerAngles.z);
        ability.effectPrefab.transform.position = spawnPoint.transform.position;
        ability.effectPrefab.transform.localScale = new Vector3(ability.currentAbilityRange, ability.currentAbilityRange, ability.currentAbilityRange);
        ability.effectPrefab.SetActive(true);
        yield return new WaitForSeconds(disableTime);
        ability.effectPrefab.SetActive(false);
    }
}