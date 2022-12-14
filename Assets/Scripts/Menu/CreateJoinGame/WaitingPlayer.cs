using System;
using System.Collections;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WaitingPlayer : MonoBehaviour
{
    private readonly string _uri = Environment.GetEnvironmentVariable("API_URI") + "/blackjack";

    public TextMeshProUGUI playerCount;
    public Button back;
    public Button ready;
    public Button undo;
    public Canvas join;
    public Canvas wait;

    private float _timePassed;


    private int _readyCount, _count;

    void Start()
    {
        StartCoroutine(GetGame(PlayerPrefs.GetString("gameId")));

        ready.onClick.AddListener(() =>
        {
            ready.enabled = false;
            undo.enabled = true;
            StartCoroutine(ReadyGame(PlayerPrefs.GetString("gameId")));
        });
        undo.onClick.AddListener(() =>
        {
            ready.enabled = true;
            undo.enabled = false;
            StartCoroutine(UndoReadyGame(PlayerPrefs.GetString("gameId")));
        });
        back.onClick.AddListener(() => { StartCoroutine(LeaveGame(PlayerPrefs.GetString("gameId"))); });
    }


    void Update()
    {
        _timePassed += Time.deltaTime;
        if (_timePassed >= 5f)
        {
            _timePassed = 0f;
            StartCoroutine(GetGame(PlayerPrefs.GetString("gameId")));
        }
        playerCount.SetText("(" + _readyCount + "/" + _count + " Players)");
    }

    private static void GameStart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    private IEnumerator LeaveGame(string gameId)
    {
        using (var request = Utils.AuthorizedPostUnityWebRequest(_uri + "/game/leave", gameId))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);
            if (result == Utils.ErrorMessage("game already started"))
            {
                GameStart();
            }

            wait.gameObject.SetActive(false);
            join.gameObject.SetActive(true);
        }
    }

    private IEnumerator ReadyGame(string gameId)
    {
        using (var request = Utils.AuthorizedPostUnityWebRequest(_uri + "/ready", gameId))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);
            if (result == Utils.ErrorMessage("already started"))
            {
                GameStart();
            }
            _readyCount += 1;
        }
    }

    private IEnumerator UndoReadyGame(string gameId)
    {
        using (var request = Utils.AuthorizedPostUnityWebRequest(_uri + "/ready/undo", gameId))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);
            if (result == Utils.ErrorMessage("already started"))
            {
                GameStart();
            }
            _readyCount -= 1;
        }
    }

    private IEnumerator GetGame(string gameId)
    {
        using (var request = UnityWebRequest.Get(_uri + "/game?game_id=" + gameId))
        {
            Debug.Log("get");
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);
            var blackjackGame = JsonConvert.DeserializeObject<BlackjackGame>(result);
            if (blackjackGame == null) yield break;
            _readyCount = 0;
            _count = blackjackGame.players.Count;
            blackjackGame.players.ForEach(player =>
            {
                if (player.status == "ready")
                {
                    _readyCount++;
                }
            });
            if (blackjackGame.status == "waiting_bet")
            {
                GameStart();
            }
        }
    }
}