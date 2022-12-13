using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class LogIn : MonoBehaviour
{
    private readonly string _uri = Environment.GetEnvironmentVariable("API_URI") + "/user";

    public Button enter;
    public Canvas mainMenu;
    public Canvas logInPage;
    public TMP_InputField nameInputField, passwordInputField;

    public TextMeshProUGUI passwordError, notExistError;


    void Start()
    {
        enter.onClick.AddListener(() =>
            StartCoroutine(Login(nameInputField.text, passwordInputField.text)));
    }

    private IEnumerator Login(string userName, string password)
    {
        var sampleUpdateInput = new LoginInput { name = userName, password = password };
        using (var request = Utils.CreateApiPostRequest(_uri + "/login", sampleUpdateInput))
        {
            yield return request.SendWebRequest();

            var result = request.result == UnityWebRequest.Result.ConnectionError
                ? request.error
                : request.downloadHandler.text;

            if (result == Utils.ErrorMessage("Invalid credentials"))
            {
                ShowPasswordError();
            }
            else if (result == Utils.ErrorMessage("User not exist"))
            {
                ShowNotExistError();
            }
            else
            {
                PlayerPrefs.SetString("access_token", result);
                PlayerPrefs.Save();
                Click();
            }
        }
    }

    private void Click()
    {
        mainMenu.gameObject.SetActive(true);
        logInPage.gameObject.SetActive(false);
    }

    private void ShowPasswordError()
    {
        passwordError.enabled = true;
        StartCoroutine(WaitPasswordError());
    }

    private void ShowNotExistError()
    {
        notExistError.enabled = true;
        StartCoroutine(WaitNotExistError());
    }

    private IEnumerator WaitPasswordError()
    {
        yield return new WaitForSeconds(3);
        passwordError.enabled = false;
    }

    private IEnumerator WaitNotExistError()
    {
        yield return new WaitForSeconds(3);
        notExistError.enabled = false;
    }
}