using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class User : MonoBehaviour
{
    private readonly string _uri = Environment.GetEnvironmentVariable("API_URI") + "/user";
    public Button getMeButton, loggedInButton;

    // Start is called before the first frame update
    void Start()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("No internet connection");
        }
        else
        {
            getMeButton.onClick.AddListener(() =>
                StartCoroutine(GetMeRequest()));
            loggedInButton.onClick.AddListener(() =>
                StartCoroutine(LoggedInRequest()));
        }
    }

    IEnumerator GetMeRequest()
    {
        using (var request = Utils.AuthorizedGetRequest(_uri + "/me"))
        {
            yield return request.SendWebRequest();

            Debug.Log(request.result == UnityWebRequest.Result.ConnectionError
                ? request.error
                : request.downloadHandler.text);
        }
    }

    IEnumerator LoggedInRequest()
    {
        using (var request = Utils.AuthorizedGetRequest(_uri + "/logged_in"))
        {
            yield return request.SendWebRequest();

            Debug.Log(request.result == UnityWebRequest.Result.ConnectionError
                ? request.error
                : request.downloadHandler.text);
        }
    }
}