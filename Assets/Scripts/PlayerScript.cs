using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class PlayerScript : MonoBehaviour
{
    private readonly string _userUri = Environment.GetEnvironmentVariable("API_URI") + "/user";
    private readonly string _blackjackUri = Environment.GetEnvironmentVariable("API_URI") + "/blackjack";

    public CardScript cardScript;
    public DeckScript deckScript;
    public TextMeshProUGUI notEnoughMoneyError, badRequestError;
    public TextMeshProUGUI leftMoney, currentBet;

    private string _myName;
    private int _money = 1000; // current money
    private int _bet = 0; // current bet

    // Array of card objects on table
    public GameObject[] hand;

    // Index of next card to be turned over
    public int cardIndex = 0;

    private BlackjackGame _currentGame;
    private float _timePassed;
    private List<Card> _dealerHand, _user0Hand, _user1Hand, _user2Hand, _user3Hand;
    private string _user0Name, _user1Name, _user2Name, _user3Name;

    void Start()
    {
        StartCoroutine(GetMe());
        StartCoroutine(GetGame(PlayerPrefs.GetString("gameId")));
    }

    void Update()
    {
        _timePassed += Time.deltaTime;
        if (_timePassed >= 5f)
        {
            _timePassed = 0f;
            StartCoroutine(GetGame(PlayerPrefs.GetString("gameId")));
        }
    }

    public void StartHand()
    {
        GetCard();
        GetCard();
    }
    
    // Recieve card from Server.
    public void GetCard()
    {
        int recieved_card_value = 0;
        deckScript.AddCard(hand[cardIndex].GetComponent<CardScript>(), recieved_card_value);
        hand[cardIndex].GetComponent<Renderer>().enabled = true;
        // Add card value to running total of the hand
        cardIndex++;
    }
    
    // Recieve hand of the player from the server
    public void GetHand()
    {
        
    }
    
    // Get current Money from server
    public void GetMoney()
    {
        int amount = 0;
        _money = amount;
    }
    
    // Hides all cards, resets the needed variables
    public void ResetHand()
    {
        for (int i = 0; i < hand.Length; i++)
        {
            hand[i].GetComponent<CardScript>().ResetCard();
            hand[i].GetComponent<Renderer>().enabled = false;
        }
    
        cardIndex = 0;
    }

    private IEnumerator GetGame(string gameId)
    {
        using (var request = UnityWebRequest.Get(_blackjackUri + "/game?game_id=" + gameId))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);
            var blackjackGame = JsonConvert.DeserializeObject<BlackjackGame>(result);
            if (blackjackGame == null) yield break;
            _currentGame = blackjackGame;
            _dealerHand = blackjackGame.dealerHand;
            for (var i = 0; i < blackjackGame.players.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        _user0Name = blackjackGame.players[i].name;
                        _user0Hand = blackjackGame.players[i].hand;
                        break;
                    case 1:
                        _user1Name = blackjackGame.players[i].name;
                        _user1Hand = blackjackGame.players[i].hand;
                        break;
                    case 2:
                        _user2Name = blackjackGame.players[i].name;
                        _user2Hand = blackjackGame.players[i].hand;
                        break;
                    case 3:
                        _user3Name = blackjackGame.players[i].name;
                        _user3Hand = blackjackGame.players[i].hand;
                        break;
                }
            }
        }
    }

    private IEnumerator Bet(string gameId, int amount)
    {
        var input = new BetInput { game_id = gameId, bet = amount };
        using (var request = Utils.AuthorizedPostRequest(_blackjackUri + "/bet", input))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);
            if (result == Utils.ErrorMessage("Not enough money"))
            {
                notEnoughMoneyError.enabled = true;
                StartCoroutine(WaitNotEnoughMoney());
            }
            else if (result == Utils.ErrorMessage("game is not waiting bet"))
            {
                badRequestError.enabled = true;
                StartCoroutine(WaitBadRequest());
            }
            else if (result == Utils.ErrorMessage("bet must be more than 0"))
            {
                badRequestError.enabled = true;
                StartCoroutine(WaitBadRequest());
            }

            var blackjackGame = JsonConvert.DeserializeObject<BlackjackGame>(result);
            if (blackjackGame == null) yield break;
            _bet = amount;
            _currentGame = blackjackGame;
        }
    }


    private IEnumerator Hit(string gameId)
    {
        using (var request = Utils.AuthorizedPostUnityWebRequest(_blackjackUri + "/hit", gameId))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);
            var blackjackGame = JsonConvert.DeserializeObject<BlackjackGame>(result);
            if (blackjackGame == null) yield break;
            _currentGame = blackjackGame;
        }
    }


    private IEnumerator Stand(string gameId)
    {
        using (var request = Utils.AuthorizedPostUnityWebRequest(_blackjackUri + "/money/deposit", gameId))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);
            if (result == Utils.ErrorMessage("game is not waiting choice"))
            {
                badRequestError.enabled = true;
                StartCoroutine(WaitBadRequest());
            }

            var blackjackGame = JsonConvert.DeserializeObject<BlackjackGame>(result);
            if (blackjackGame == null) yield break;
            _currentGame = blackjackGame;
        }
    }


    private IEnumerator DoubleDown(string gameId)
    {
        using (var request = Utils.AuthorizedPostUnityWebRequest(_blackjackUri + "/money/deposit", gameId))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);
            if (result == Utils.ErrorMessage("Not enough money"))
            {
                notEnoughMoneyError.enabled = true;
                StartCoroutine(WaitNotEnoughMoney());
            }
            else if (result == Utils.ErrorMessage("game is not waiting choice"))
            {
                badRequestError.enabled = true;
                StartCoroutine(WaitBadRequest());
            }
            else if (result == Utils.ErrorMessage("can't double down"))
            {
                badRequestError.enabled = true;
                StartCoroutine(WaitBadRequest());
            }

            var blackjackGame = JsonConvert.DeserializeObject<BlackjackGame>(result);
            if (blackjackGame == null) yield break;
            _currentGame = blackjackGame;
        }
    }


    private IEnumerator GetMe()
    {
        using (var request = Utils.AuthorizedGetRequest(_userUri + "/me"))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);
            if (result == Utils.ErrorMessage("Invalid credentials")) Debug.Log("Invalid credentials");
            var user = JsonConvert.DeserializeObject<User>(result);
            if (user == null) yield break;
            _money = user.money;
            _myName = user.name;
        }
    }

    private IEnumerator Deposit(int amount)
    {
        using (var request = Utils.AuthorizedPostRequest(_userUri + "/money/deposit", amount))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);
            if (result == Utils.ErrorMessage("Invalid credentials")) Debug.Log("Invalid credentials");
            var user = JsonConvert.DeserializeObject<User>(result);
            if (user == null) yield break;
            _money = user.money;
        }
    }

    private IEnumerator WithDraw(int amount)
    {
        if (amount > _money)
        {
            notEnoughMoneyError.enabled = true;
            StartCoroutine(WaitNotEnoughMoney());
            yield break;
        }

        using (var request = Utils.AuthorizedPostRequest(_userUri + "/money/withdraw", amount))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);
            if (result == Utils.ErrorMessage("Invalid credentials")) Debug.Log("Invalid credentials");
            var user = JsonConvert.DeserializeObject<User>(result);
            if (user == null) yield break;
            _money = user.money;
        }
    }

    private IEnumerator WaitNotEnoughMoney()
    {
        yield return new WaitForSeconds(3);
        notEnoughMoneyError.enabled = false;
    }

    private IEnumerator WaitBadRequest()
    {
        yield return new WaitForSeconds(3);
        badRequestError.enabled = false;
    }
}

internal class BetInput
{
    public string game_id;
    public int bet;
}