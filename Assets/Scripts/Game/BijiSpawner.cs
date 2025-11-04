using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BijiSpawner : MonoBehaviour
{
     public GameObject dotPrefab;
    public float spawnInterval;
    private float timer;
    public float spawnRangeX;
    public float spawnY;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            Vector2 spawnPos = new Vector2(Random.Range(-spawnRangeX, spawnRangeX), spawnY);
            Instantiate(dotPrefab, spawnPos, Quaternion.identity);
            timer = 0f;
        }
    }
}
