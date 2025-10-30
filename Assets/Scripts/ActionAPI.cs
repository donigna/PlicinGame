using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class ActionAPI : MonoBehaviour
{
    public TMP_InputField inputURL;
    public TMP_Text textResult;

    public void RunRequest()
    {
        string url = inputURL.text;
        UnityWebRequest request = UnityWebRequest.Get(url);
        StartCoroutine(Execute());

        IEnumerator Execute()
        {
            yield return request.SendWebRequest();
            textResult.text = (request.result == UnityWebRequest.Result.Success) ? request.downloadHandler.text : $"Error: {request.error}";
        }
    }
}
