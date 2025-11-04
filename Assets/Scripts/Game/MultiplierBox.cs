using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MultiplierBox : MonoBehaviour
{
    public float multiplier;
    private TextMeshPro text;

    private void Start()
    {
        // Membuat teks dinamis di atas kotak
        GameObject textObj = new GameObject("MultiplierText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = Vector3.zero + Vector3.up * 0.1f; // sedikit di atas tengah

        text = textObj.AddComponent<TextMeshPro>();
        text.text = "×" + multiplier.ToString("0.##");
        text.fontSize = 3f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;

        // pastikan teks muncul di atas sprite
        var renderer = text.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 5;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Dot"))
        {
            Debug.Log($"Dot menyentuh multiplier ×{multiplier}");
            ScoreManager.Instance.AddScore(multiplier);
            Destroy(other.gameObject); // hilangkan dot
        }
    }

}
