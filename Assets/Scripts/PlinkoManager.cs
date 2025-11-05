using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlinkoManager : MonoBehaviour
{
    public GameObject pinPrefab;
    public GameObject ballPrefab;
    public GameObject slotPrefab;
    public Transform pinsParent;
    public Transform ballsParent;
    public Transform slotsParent;
    public int rows = 8;
    public float horizontalSpacing = 1f;
    public float verticalSpacing = 1f;

    public List<List<Transform>> pinsByRow = new List<List<Transform>>();
    public List<PlinkoSlot> slots = new List<PlinkoSlot>();

    [Space]
    [Header("Ball Spawn")]
    float spawnInterval = 1;

    Coroutine dropBallCoroutine;

    void Start()
    {
        GeneratePins();
        GenerateSlots();
    }

    public bool isDroping() => ballsParent.childCount > 0;

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
            SpawnBall();
            yield return new WaitForSeconds(spawnInterval);
            droppedBall++;
            Time.timeScale += 0.01f;
        }
        Time.timeScale = 1;
        dropBallCoroutine = null;
    }

    void GeneratePins()
    {
        pinsByRow.Clear();

        for (int row = 0; row < rows; row++)
        {
            int pinsInRow = row + 1;
            float offsetX = -horizontalSpacing * row / 2f;
            List<Transform> rowList = new List<Transform>();

            for (int i = 0; i < pinsInRow; i++)
            {
                Vector3 pos = new Vector3(transform.position.x + i * horizontalSpacing + offsetX, transform.position.y - row * verticalSpacing, 0);
                GameObject pinGO = Instantiate(pinPrefab, pos, Quaternion.identity, pinsParent);
                rowList.Add(pinGO.transform);
            }

            pinsByRow.Add(rowList);
        }

    }

    void GenerateSlots()
    {
        if (slots != null)
            slots.Clear();

        var lastRow = pinsByRow[pinsByRow.Count - 1];
        int row = lastRow.Count;
        for (int i = 0; i < lastRow.Count + 1; i++)
        {
            float offsetX = -horizontalSpacing * row / 2f;
            Vector3 slotPos = new Vector3(transform.position.x + i * horizontalSpacing + offsetX, transform.position.y - row * verticalSpacing, 0);
            GameObject slotGO = Instantiate(slotPrefab, slotPos, Quaternion.identity, slotsParent);

            PlinkoSlot ps = slotGO.GetComponent<PlinkoSlot>();
            if (i == 0 || i == lastRow.Count)
            {
                ps.multiplier = 2.5f;
                ps.sprite.color = Color.red;
            }
            if (i == 1 || i == lastRow.Count - 1)
            {
                ps.multiplier = 1.5f;
                ps.sprite.color = Color.yellow;
            }
            if (i == 2 || i == lastRow.Count - 2)
            {
                ps.multiplier = 1f;
                ps.sprite.color = Color.green;
            }
            if (i == 3 || i == lastRow.Count - 3)
            {
                ps.multiplier = 0.5f;
                ps.sprite.color = Color.blue;
            }

            slots.Add(ps);
        }
    }
    void SpawnBall()
    {
        Vector3 startPos = pinsByRow[0][0].position + Vector3.up * 1f; // sedikit di atas pin pertama
        GameObject ballGO = Instantiate(ballPrefab, startPos, Quaternion.identity, ballsParent);

        // Build path nodes for this ball: pick one pin per row using plinko logic
        List<Transform> path = BuildRandomPath();

        // Call SetPathNodes pada PlinkoBall
        PlinkoBall pb = ballGO.GetComponent<PlinkoBall>();
        if (pb != null)
        {
            pb.SetPathNodes(path);
        }
        else
        {
            Debug.LogWarning("Spawned ball prefab has no PlinkoBall component.");
        }
    }

    private List<Transform> BuildRandomPath()
    {
        List<Transform> path = new List<Transform>();

        if (pinsByRow.Count == 0) return path;

        // Start index: choose nearest pin in row 0.
        int currentIndex = 0; // row 0 has 1 pin => index 0
        for (int row = 0; row < pinsByRow.Count; row++)
        {
            var rowList = pinsByRow[row];

            // clamp just in case:
            currentIndex = Mathf.Clamp(currentIndex, 0, rowList.Count - 1);
            path.Add(rowList[currentIndex]);

            // decide next index for next row (if exists)
            if (row + 1 < pinsByRow.Count)
            {
                bool goRight = Random.value < 0.5f;
                // next row has one more pin, choices are currentIndex (left) or currentIndex+1 (right)
                currentIndex = goRight ? currentIndex + 1 : currentIndex;
            }
        }

        return path;
    }
}
