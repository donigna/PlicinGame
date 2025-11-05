using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BijiSpawner : MonoBehaviour
{
    public static BijiSpawner Instance;
    public GameObject dotPrefab;
    public float spawnInterval;
    public float spawnRangeX;
    public float spawnY;

    Coroutine dropBallCoroutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool isDroping() => dropBallCoroutine != null;

    public void DropBall(int amount)
    {
        if (dropBallCoroutine != null) return;
        dropBallCoroutine = StartCoroutine(StartDropingBalls(amount));
    }

    IEnumerator StartDropingBalls(int amount)
    {
        int droppedBall = 0;
        while (droppedBall < amount)
        {
            Vector2 spawnPos = new Vector2(Random.Range(transform.position.x + -spawnRangeX, transform.position.x + spawnRangeX), spawnY);
            Instantiate(dotPrefab, spawnPos, Quaternion.identity);
            yield return new WaitForSeconds(spawnInterval);
            droppedBall++;
        }
        dropBallCoroutine = null;
    }

}
