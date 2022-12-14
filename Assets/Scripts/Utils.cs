using System;
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

    public static UnityWebRequest AuthorizedGetRequest(string uri)
    {
        var request = UnityWebRequest.Get(uri);
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));
        return request;
    }

    public static UnityWebRequest AuthorizedDeleteRequest(string uri)
    {
        var request = UnityWebRequest.Delete(uri);
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));
        return request;
    }

    public static UnityWebRequest AuthorizedPostRequest(string uri, object body = null)
    {
        var request = CreateApiPostRequest(uri, body);
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));
        return request;
    }
    
    public static UnityWebRequest AuthorizedPostUnityWebRequest(string uri, string postData)
    {
        var request = UnityWebRequest.Post(uri, postData);
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));
        return request;
    }

    public static UnityWebRequest AuthorizedPutRequest(string uri, object body = null)
    {
        var request = CreateApiPutRequest(uri, body);
        request.SetRequestHeader("Authorization", "Bearer " + PlayerPrefs.GetString("access_token"));
        return request;
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

    public static string ErrorMessage(string message)
    {
        return "{\"detail\":\"" + message + "\"}";
    }

    public static string RequestResult(UnityWebRequest request)
    {
        return request.result == UnityWebRequest.Result.ConnectionError
            ? throw new Exception(request.error)
            : request.downloadHandler.text;
    }
}