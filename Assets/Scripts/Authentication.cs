using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Authentication : MonoBehaviour
{
    private readonly string _uri = Environment.GetEnvironmentVariable("API_URI") + "/user";
    public Button signupButton, loginButton;
    public TMP_InputField nameInputField, passwordInputField;

    // Start is called before the first frame update
    void Start()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.Log("No internet connection");
        }
        else
        {
            signupButton.onClick.AddListener(() =>
                StartCoroutine(Signup(nameInputField.text, passwordInputField.text)));
            loginButton.onClick.AddListener(() =>
                StartCoroutine(Login(nameInputField.text, passwordInputField.text)));
        }
    }

    IEnumerator Signup(string userName, string password)
    {
        var sampleUpdateInput = new LoginInput { name = userName, password = password };
        using (var request = Utils.CreateApiPostRequest(_uri + "/signup", sampleUpdateInput))
        {
            yield return request.SendWebRequest();

            Debug.Log(request.result == UnityWebRequest.Result.ConnectionError
                ? request.error
                : request.downloadHandler.text);
        }
    }

    IEnumerator Login(string userName, string password)
    {
        var sampleUpdateInput = new LoginInput { name = userName, password = password };
        using (var request = Utils.CreateApiPostRequest(_uri + "/login", sampleUpdateInput))
        {
            yield return request.SendWebRequest();

            Debug.Log(request.result == UnityWebRequest.Result.ConnectionError
                ? request.error
                : request.downloadHandler.text);
            PlayerPrefs.SetString("access_token", request.downloadHandler.text);
            PlayerPrefs.Save();
        }
    }
}

internal class LoginInput
{
    public string name;
    public string password;
}