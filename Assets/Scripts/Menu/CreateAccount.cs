using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateAccount : MonoBehaviour
{
    private readonly string _uri = Environment.GetEnvironmentVariable("API_URI") + "/user";

    public Button create;
    public Canvas accountCreated;
    public Canvas register;
    public TextMeshProUGUI userExistError;
    public TMP_InputField nameInputField, passwordInputField;


    void Start()
    {
        create.onClick.AddListener(() =>
            StartCoroutine(Signup(nameInputField.text, passwordInputField.text)));
    }

    private IEnumerator Signup(string userName, string password)
    {
        var sampleUpdateInput = new LoginInput { name = userName, password = password };
        using (var request = Utils.CreateApiPostRequest(_uri + "/signup", sampleUpdateInput))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);

            if (result == Utils.ErrorMessage("User already exists"))
            {
                ShowUserExistError();
            }
            else
            {
                Click();
            }
        }
    }

    private void Click()
    {
        accountCreated.gameObject.SetActive(true);
        register.gameObject.SetActive(false);
    }

    private void ShowUserExistError()
    {
        userExistError.enabled = true;
        StartCoroutine(Wait());
    }

    private IEnumerator Wait()
    {
        yield return new WaitForSeconds(3);
        userExistError.enabled = false;
    }
}