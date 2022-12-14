using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class CreateBtn : MonoBehaviour
{
    private readonly string _uri = Environment.GetEnvironmentVariable("API_URI") + "/blackjack";

    public Button create;
    public Canvas createMenu;
    public Canvas waitScreen;
    public TMP_InputField nameInputField;


    void Start()
    {
        create.onClick.AddListener(() => StartCoroutine(CreateGame(nameInputField.text)));
    }

    private IEnumerator CreateGame(string gameName)
    {
        using (var request = UnityWebRequest.Post(_uri + "/create", gameName))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);

            PlayerPrefs.SetString("access_token", result);
            PlayerPrefs.Save();
            Click();
        }
    }

    private void Click()
    {
        waitScreen.gameObject.SetActive(true);
        createMenu.gameObject.SetActive(false);
    }
}