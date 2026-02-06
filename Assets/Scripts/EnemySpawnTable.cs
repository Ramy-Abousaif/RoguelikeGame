using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Spawning/Enemy Spawn Table")]
public class EnemySpawnTable : ScriptableObject
{
    public List<EnemySpawnEntry> enemies = new();

    public GameObject GetRandom(float elapsedTime, int playerLevel)
    {
        float totalWeight = 0f;

        foreach (var e in enemies)
        {
            if (!IsValid(e, elapsedTime, playerLevel))
                continue;

            totalWeight += e.weight;
        }

        if (totalWeight <= 0f)
            return null;

        float roll = Random.value * totalWeight;

        foreach (var e in enemies)
        {
            if (!IsValid(e, elapsedTime, playerLevel))
                continue;

            roll -= e.weight;
            if (roll <= 0f)
                return e.prefab;
        }

        return null;
    }

    private bool IsValid(EnemySpawnEntry e, float time, int level)
    {
        if (e.prefab == null) return false;
        if (time < e.minTime || time > e.maxTime) return false;
        if (level < e.minPlayerLevel) return false;
        return true;
    }
}