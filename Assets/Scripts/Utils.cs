using System.Text;
using UnityEngine;
using UnityEngine.Networking;

public static class Utils
{
    public static UnityWebRequest CreateApiPostRequest(string uri, object body = null)
    {
        return CreateApiRequest(uri, UnityWebRequest.kHttpVerbPOST, body);
    }
    
    public static UnityWebRequest CreateApiPutRequest(string uri, object body = null)
    {
        return CreateApiRequest(uri, UnityWebRequest.kHttpVerbPUT, body);
    }

    static UnityWebRequest CreateApiRequest(string url, string method, object body)
    {
        string bodyString = null;
        if (body is string)
        {
            bodyString = (string)body;
        }
        else if (body != null)
        {
            bodyString = JsonUtility.ToJson(body);
        }

        var request = new UnityWebRequest();
        request.url = url;
        request.method = method;
        request.downloadHandler = new DownloadHandlerBuffer();
        request.uploadHandler =
            new UploadHandlerRaw(string.IsNullOrEmpty(bodyString) ? null : Encoding.UTF8.GetBytes(bodyString));
        request.SetRequestHeader("Accept", "application/json");
        request.SetRequestHeader("Content-Type", "application/json");
        request.timeout = 60;
        return request;
    }
}