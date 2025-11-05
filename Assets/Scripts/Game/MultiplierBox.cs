using TMPro;
using UnityEngine;

public class MultiplierBox : MonoBehaviour
{
    public float multiplier;
    private TextMeshPro text;

    private void Start()
    {
        GameObject textObj = new GameObject("MultiplierText");
        textObj.transform.SetParent(transform);
        textObj.transform.localPosition = Vector3.zero + Vector3.up * 0.1f;

        text = textObj.AddComponent<TextMeshPro>();
        text.text = multiplier.ToString("0.##");
        text.fontSize = 3f;
        text.alignment = TextAlignmentOptions.Center;
        text.color = Color.black;

        var renderer = text.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 5;
    }

}
