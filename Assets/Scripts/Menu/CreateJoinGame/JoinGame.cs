using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class JoinGame : MonoBehaviour
{
    private readonly string _uri = Environment.GetEnvironmentVariable("API_URI") + "/blackjack";

    public TextMeshProUGUI select, full;
    public Canvas join;
    public Canvas wait;
    public Button joinBtn;
    public Button room1, room2, room3, room4;
    private bool _isRoom1Exists, _isRoom2Exists, _isRoom3Exists, _isRoom4Exists;
    private BlackjackGame _game1, _game2, _game3, _game4;
    private BlackjackGame _selectedGame;

    void Start()
    {
        StartCoroutine(GetAllGame());

        // if (_isRoom1Exists) works before the coroutine is done

        joinBtn.onClick.AddListener(() => JoinClick(_selectedGame));
    }

    private void JoinClick(BlackjackGame game)
    {
        if (game.players.Count == 4) // if room is full, error sign shows
        {
            select.enabled = false;
            full.enabled = true;
            StartCoroutine(WaitRoomIsFull());
        }
        else // if room is not full, head to waiting screen
        {
            StartCoroutine(EnterGame(game._id));
        }
    }

    private IEnumerator WaitRoomIsFull()
    {
        yield return new WaitForSeconds(3);
        full.enabled = false;
        select.enabled = true;
    }

    private void AfterGetAllGame()
    {
        if (_isRoom1Exists)
        {
            room1.GetComponentInChildren<TextMeshProUGUI>().text = _game1.name;
            room1.onClick.AddListener(() => _selectedGame = _game1);
        }

        if (_isRoom2Exists)
        {
            room2.GetComponentInChildren<TextMeshProUGUI>().text = _game2.name;
            room2.onClick.AddListener(() => _selectedGame = _game2);
        }

        if (_isRoom3Exists)
        {
            room3.GetComponentInChildren<TextMeshProUGUI>().text = _game3.name;
            room3.onClick.AddListener(() => _selectedGame = _game3);
        }

        if (_isRoom4Exists)
        {
            room4.GetComponentInChildren<TextMeshProUGUI>().text = _game4.name;
            room4.onClick.AddListener(() => _selectedGame = _game4);
        }
    }

    private IEnumerator GetAllGame()
    {
        using (var request = UnityWebRequest.Get(_uri + "/game/all"))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);

            var blackjackGames = JsonConvert.DeserializeObject<List<BlackjackGame>>(result);
            if (blackjackGames == null) yield break;

            foreach (var blackjackGame in blackjackGames)
            {
                if (!_isRoom1Exists)
                {
                    _isRoom1Exists = true;
                    _game1 = blackjackGame;
                }
                else if (!_isRoom2Exists)
                {
                    _isRoom2Exists = true;
                    _game2 = blackjackGame;
                }
                else if (!_isRoom3Exists)
                {
                    _isRoom3Exists = true;
                    _game3 = blackjackGame;
                }
                else if (!_isRoom4Exists)
                {
                    _isRoom4Exists = true;
                    _game4 = blackjackGame;
                }
                else break;
            }

            AfterGetAllGame();
        }
    }

    private IEnumerator EnterGame(string gameId)
    {
        using (var request = Utils.AuthorizedPostUnityWebRequest(_uri + "/enter", gameId))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);
            if (result == Utils.ErrorMessage("already entered")) Debug.Log("already entered");
            wait.gameObject.SetActive(true);
            join.gameObject.SetActive(false);
        }
    }
}