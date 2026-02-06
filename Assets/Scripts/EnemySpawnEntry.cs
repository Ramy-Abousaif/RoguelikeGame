using UnityEngine;

[System.Serializable]
public class EnemySpawnEntry
{
    public GameObject prefab;

    [Min(0f)]
    public float weight = 1f;

    [Header("Optional Gating")]
    public float minTime = 0f;
    public float maxTime = Mathf.Infinity;

    public int minPlayerLevel = 0;
}