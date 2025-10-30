using System;
using UnityEngine;

public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogError("JSON kosong atau null");
            return Array.Empty<T>();
        }

        if (!json.TrimStart().StartsWith("["))
        {
            Debug.LogError("JSON bukan array: " + json);
            return Array.Empty<T>();
        }

        string newJson = "{\"Items\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(newJson);
        return wrapper.Items ?? Array.Empty<T>();
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] Items;
    }
}
