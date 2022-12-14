using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class BlackjackScript : MonoBehaviour
{
    private readonly string _userUri = Environment.GetEnvironmentVariable("API_URI") + "/user";
    private readonly string _blackjackUri = Environment.GetEnvironmentVariable("API_URI") + "/blackjack";

    // public CardScript cardScript;
    public DeckScript deckScript;

    public TextMeshProUGUI notEnoughMoneyError, badRequestError, playerCount;
    public TextMeshProUGUI money, bet;
    public TextMeshProUGUI user0Name, user1Name, user2Name, user3Name;
    public SpriteRenderer user0Card0, user0Card1, user0Card2, user0Card3, user0Card4, user0Card5;
    public SpriteRenderer user1Card0, user1Card1, user1Card2, user1Card3, user1Card4, user1Card5;
    public SpriteRenderer user2Card0, user2Card1, user2Card2, user2Card3, user2Card4, user2Card5;
    public SpriteRenderer user3Card0, user3Card1, user3Card2, user3Card3, user3Card4, user3Card5;
    public SpriteRenderer dealerCard0, dealerCard1, dealerCard2, dealerCard3, dealerCard4, dealerCard5;

    private string _myName, _myStatus;
    private int _money; // current money
    private int _bet; // current bet

    private BlackjackGame _currentGame;
    private float _timePassed;
    private List<Card> _dealerHand, _user0Hand, _user1Hand, _user2Hand, _user3Hand;
    private string _user0Name, _user1Name, _user2Name, _user3Name;
    private int _readyCount, _count;

    private Thread _receiveThread;
    private TcpClient _client;
    private TcpListener _listener;
    private const int Port = 9999;

    private readonly Queue<string> _queue = new(); //string형태의 que생성

    void Start()
    {
        StartCoroutine(GetMe());
        StartCoroutine(GetGame(PlayerPrefs.GetString("gameId")));
        InitTcp();
    }

    // Launch TCP to receive message from python
    private void InitTcp()
    {
        _receiveThread = new Thread(ReceiveData)
        {
            IsBackground = true // 생성한 thread를 백그라운드로 사용
        }; //새로운 thread를 만들고 데이터를 받는 메서드를 넘김.
        _receiveThread.Start();
    }

    private void ReceiveData()
    {
        try
        {
            print("Waiting");
            _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), Port); // ip주소와 포트 번호를 할당하여 TCP Listener를 생성
            _listener.Start();
            var bytes = new byte[1024]; // 클라이언트로부터 받아올 데이터의 크기를 byte로 생성

            while (true)
            {
                using (_client = _listener.AcceptTcpClient()) // 클라이언트 연결을 수락하고 TcpClient 객체를 반환함
                {
                    using (var stream = _client.GetStream())
                    {
                        int length;
                        while ((length = stream.Read(bytes, 0, bytes.Length)) !=
                               0) // byte 변수에 스트림 데이터를 읽어들임. 연결이 끊어지면 0을 반환함
                        {
                            var clientMessage =
                                Encoding.UTF8.GetString(bytes, 0, length); //byte 형식 데이터를 UTF-8 형식으로 인코딩하고 문자열로 변환함
                            Debug.Log(clientMessage);
                            _queue.Enqueue(clientMessage); // 큐에 데이터를 저장
                        }

                        Debug.Log("연결이 끊어졌습니다.");
                        Debug.Log("프로세스를 종료합니다.");
                        OnApplicationQuit();
                    }
                }
            }
        }
        catch (Exception e) // 에러발생시
        {
            print(e.ToString()); //에러문 출력
        }
    }

    void OnApplicationQuit()
    {
        // close the thread when the application quits
        _receiveThread.Abort();
    }

    void Update()
    {
        _timePassed += Time.deltaTime;
        if (_timePassed >= 5f)
        {
            _timePassed = 0f;
            StartCoroutine(GetGame(PlayerPrefs.GetString("gameId")));
        }

        if (_queue.Count > 0)
        {
            var command = _queue.Dequeue();
            if (_currentGame.status == "end")
            {
                switch (command)
                {
                    case "game_start":
                        StartCoroutine(ReadyGame(PlayerPrefs.GetString("gameId")));
                        break;
                    case "quit":
                        StartCoroutine(LeaveGame(PlayerPrefs.GetString("gameId")));
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
                        break;
                }
            }
            else if (_currentGame.status == "waiting_bet")
            {
                switch (command)
                {
                    case "one":
                        _bet += 1;
                        break;
                    case "two":
                        _bet += 5;
                        break;
                    case "three":
                        _bet += 25;
                        break;
                    case "four":
                        _bet += 50;
                        break;
                    case "five":
                        _bet += 100;
                        break;
                    case "ok":
                        StartCoroutine(Bet(PlayerPrefs.GetString("gameId"), _bet));
                        break;
                }
            }
            else if (_currentGame.status == "waiting_choice" && _myStatus != "stand")
            {
                switch (command)
                {
                    case "hit":
                        StartCoroutine(Hit(PlayerPrefs.GetString("gameId")));
                        break;
                    case "stand":
                        StartCoroutine(Stand(PlayerPrefs.GetString("gameId")));
                        break;
                    case "two":
                        StartCoroutine(DoubleDown(PlayerPrefs.GetString("gameId")));
                        break;
                }
            }
        }
        
        if (_user0Hand != null)
        {
            for (var i = 0; i < _user0Hand.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        RenderCard(_user0Hand[i], user0Card0);
                        break;
                    case 1:
                        RenderCard(_user0Hand[i], user0Card1);
                        break;
                    case 2:
                        RenderCard(_user0Hand[i], user0Card2);
                        break;
                    case 3:
                        RenderCard(_user0Hand[i], user0Card3);
                        break;
                    case 4:
                        RenderCard(_user0Hand[i], user0Card4);
                        break;
                    case 5:
                        RenderCard(_user0Hand[i], user0Card5);
                        break;
                }
            }
        }

        if (_user1Hand != null)
        {
            for (var i = 0; i < _user1Hand.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        RenderCard(_user1Hand[i], user1Card0);
                        break;
                    case 1:
                        RenderCard(_user1Hand[i], user1Card1);
                        break;
                    case 2:
                        RenderCard(_user1Hand[i], user1Card2);
                        break;
                    case 3:
                        RenderCard(_user1Hand[i], user1Card3);
                        break;
                    case 4:
                        RenderCard(_user1Hand[i], user1Card4);
                        break;
                    case 5:
                        RenderCard(_user1Hand[i], user1Card5);
                        break;
                }
            }
        }

        if (_user2Hand != null)
        {
            for (var i = 0; i < _user2Hand.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        RenderCard(_user2Hand[i], user2Card0);
                        break;
                    case 1:
                        RenderCard(_user2Hand[i], user2Card1);
                        break;
                    case 2:
                        RenderCard(_user2Hand[i], user2Card2);
                        break;
                    case 3:
                        RenderCard(_user2Hand[i], user2Card3);
                        break;
                    case 4:
                        RenderCard(_user2Hand[i], user2Card4);
                        break;
                    case 5:
                        RenderCard(_user2Hand[i], user2Card5);
                        break;
                }
            }
        }

        if (_user3Hand != null)
        {
            for (var i = 0; i < _user3Hand.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        RenderCard(_user3Hand[i], user3Card0);
                        break;
                    case 1:
                        RenderCard(_user3Hand[i], user3Card1);
                        break;
                    case 2:
                        RenderCard(_user3Hand[i], user3Card2);
                        break;
                    case 3:
                        RenderCard(_user3Hand[i], user3Card3);
                        break;
                    case 4:
                        RenderCard(_user3Hand[i], user3Card4);
                        break;
                    case 5:
                        RenderCard(_user3Hand[i], user3Card5);
                        break;
                }
            }
        }

        if (_dealerHand != null)
        {
            for (var i = 0; i < _dealerHand.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        RenderCard(_dealerHand[i], dealerCard0);
                        break;
                    case 1:
                        RenderCard(_dealerHand[i], dealerCard1);
                        break;
                    case 2:
                        RenderCard(_dealerHand[i], dealerCard2);
                        break;
                    case 3:
                        RenderCard(_dealerHand[i], dealerCard3);
                        break;
                    case 4:
                        RenderCard(_dealerHand[i], dealerCard4);
                        break;
                    case 5:
                        RenderCard(_dealerHand[i], dealerCard5);
                        break;
                }
            }
        }
        money.text = (_money - _bet).ToString();
        bet.text = _bet.ToString();
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
            _count = blackjackGame.players.Count;
            for (var i = 0; i < blackjackGame.players.Count; i++)
            {
                switch (i)
                {
                    case 0:
                        _user0Name = blackjackGame.players[i].name;
                        _user0Hand = blackjackGame.players[i].hand;
                        if (_myName == _user0Name) _myStatus = blackjackGame.players[i].status;
                        break;
                    case 1:
                        _user1Name = blackjackGame.players[i].name;
                        _user1Hand = blackjackGame.players[i].hand;
                        if (_myName == _user1Name) _myStatus = blackjackGame.players[i].status;
                        break;
                    case 2:
                        _user2Name = blackjackGame.players[i].name;
                        _user2Hand = blackjackGame.players[i].hand;
                        if (_myName == _user2Name) _myStatus = blackjackGame.players[i].status;
                        break;
                    case 3:
                        _user3Name = blackjackGame.players[i].name;
                        _user3Hand = blackjackGame.players[i].hand;
                        if (_myName == _user3Name) _myStatus = blackjackGame.players[i].status;
                        break;
                }
            }

            user0Name.text = _user0Name;
            user1Name.text = _user1Name;
            user2Name.text = _user2Name;
            user3Name.text = _user3Name;
            if (_currentGame.status == "end")
            {
                playerCount.enabled = true;
                playerCount.SetText("(" + _readyCount + "/" + _count + " Players)");
            }
            else
            {
                playerCount.enabled = false;
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

    private IEnumerator ReadyGame(string gameId)
    {
        using (var request = Utils.AuthorizedPostUnityWebRequest(_blackjackUri + "/ready", gameId))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);
            if (result == Utils.ErrorMessage("already started"))
            {
                yield break;
            }

            _readyCount += 1;
        }
    }

    private IEnumerator LeaveGame(string gameId)
    {
        using (var request = Utils.AuthorizedPostUnityWebRequest(_blackjackUri + "/game/leave", gameId))
        {
            yield return request.SendWebRequest();

            var result = Utils.RequestResult(request);
            if (result == Utils.ErrorMessage("game already started"))
            {
                yield break;
            }
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

    private int CardToNumber(Card card)
    {
        var value = card.suit switch
        {
            "Hearts ♥" => 0,
            "Diamonds ♦" => 13,
            "Spades ♠" => 26,
            "Clubs ♣" => 39,
            _ => 0
        };

        switch (card.rank)
        {
            case "A":
                value += 1;
                break;
            case "2":
                value += 2;
                break;
            case "3":
                value += 3;
                break;
            case "4":
                value += 4;
                break;
            case "5":
                value += 5;
                break;
            case "6":
                value += 6;
                break;
            case "7":
                value += 7;
                break;
            case "8":
                value += 8;
                break;
            case "9":
                value += 9;
                break;
            case "10":
                value += 10;
                break;
            case "J":
                value += 11;
                break;
            case "K":
                value += 12;
                break;
            case "Q":
                value += 13;
                break;
        }

        return value;
    }

    // Render the card with SpriteRenderer
    private void RenderCard(Card card, SpriteRenderer spriteRenderer)
    {
        var value = CardToNumber(card);
        spriteRenderer.sprite = deckScript.cardSprites[value];
        spriteRenderer.enabled = true;
    }
}

internal class BetInput
{
    public string game_id;
    public int bet;
}