using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


public class RestApiSample : MonoBehaviour
{
    private readonly string _uri = Environment.GetEnvironmentVariable("API_URI");
    public Button sampleGetAllButton, sampleGetButton, sampleCreateButton, sampleUpdateButton, sampleDeleteButton;
    public TMP_InputField idInputField, nameInputField;

    void Start()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("No internet connection");
        }
        else
        {
            Debug.Log(_uri);
            StartCoroutine(GetRequest());
            sampleGetAllButton.onClick.AddListener(() => StartCoroutine(SampleGetAllRequest()));
            sampleGetButton.onClick.AddListener(() => StartCoroutine(SampleGetRequest(idInputField.text)));
            sampleCreateButton.onClick.AddListener(() =>
                StartCoroutine(SampleCreateRequest(nameInputField.text)));
            sampleUpdateButton.onClick.AddListener(() =>
                StartCoroutine(SampleUpdateRequest(idInputField.text, nameInputField.text)));
            sampleDeleteButton.onClick.AddListener(() => StartCoroutine(SampleDeleteRequest(idInputField.text)));
        }
    }

    IEnumerator GetRequest()
    {
        using (var request = UnityWebRequest.Get(_uri))
        {
            yield return request.SendWebRequest();

            Debug.Log(request.result == UnityWebRequest.Result.ConnectionError
                ? request.error
                : request.downloadHandler.text);
        }
    }

    IEnumerator SampleGetAllRequest()
    {
        using (var request = UnityWebRequest.Get(_uri + "/sample/all"))
        {
            yield return request.SendWebRequest();

            Debug.Log(request.result == UnityWebRequest.Result.ConnectionError
                ? request.error
                : request.downloadHandler.text);
        }
    }

    IEnumerator SampleGetRequest(string id)
    {
        using (
            var request = UnityWebRequest.Get(_uri + "/sample?_id=" + id))
        {
            yield return request.SendWebRequest();

            Debug.Log(request.result == UnityWebRequest.Result.ConnectionError
                ? request.error
                : request.downloadHandler.text);
        }
    }

    IEnumerator SampleCreateRequest(string sampleName)
    {
        using (var request = UnityWebRequest.Post(_uri + "/sample", sampleName))
        {
            yield return request.SendWebRequest();

            Debug.Log(request.result == UnityWebRequest.Result.ConnectionError
                ? request.error
                : request.downloadHandler.text);

        }
    }

    IEnumerator SampleUpdateRequest(string id, string sampleName)
    {
        var sampleUpdateInput = new SampleUpdateInput { id = id, name = sampleName };
        var body = JsonUtility.ToJson(sampleUpdateInput);
        using (var request = UnityWebRequest.Put(_uri + "/sample", body))
        {
            request.SetRequestHeader("Content-Type", "application/json");
            yield return request.SendWebRequest();

            Debug.Log(request.result == UnityWebRequest.Result.ConnectionError
                ? request.error
                : request.downloadHandler.text);
        }
    }

    IEnumerator SampleDeleteRequest(string id)
    {
        using (var request = UnityWebRequest.Delete(_uri + "/sample?_id=" + id))
        {
            yield return request.SendWebRequest();

            // NullReferenceException occurs when using request.downloadHandler.text
            // Debug.Log(request.result == UnityWebRequest.Result.ConnectionError
            //     ? request.error
            //     : request.downloadHandler.text);
            
            Debug.Log(request.result == UnityWebRequest.Result.ConnectionError
                ? request.error
                : "Successfully deleted");
        }
    }
}

internal class SampleUpdateInput
{
    public string id;
    public string name;
}