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

public class BlackjackScript : MonoBehaviour
{
    private readonly string _userUri = Environment.GetEnvironmentVariable("API_URI") + "/user";
    private readonly string _blackjackUri = Environment.GetEnvironmentVariable("API_URI") + "/blackjack";

    // public CardScript cardScript;
    // public DeckScript deckScript;
    public TextMeshProUGUI notEnoughMoneyError, badRequestError;
    public TextMeshProUGUI money, bet;

    private string _myName, _myStatus;
    private int _money; // current money
    private int _bet; // current bet

    // // Array of card objects on table
    // public GameObject[] hand;
    //
    // // Index of next card to be turned over
    // public int cardIndex = 0;

    private BlackjackGame _currentGame;
    private float _timePassed;
    private List<Card> _dealerHand, _user0Hand, _user1Hand, _user2Hand, _user3Hand;
    private string _user0Name, _user1Name, _user2Name, _user3Name;
    private int _readyCount, _count;

    private Thread _receiveThread;
    private TcpClient _client;
    private TcpListener _listener;
    private const int Port = 9999;

    private readonly Queue<string> _queue = new Queue<string>(); //string형태의 que생성

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
}

internal class BetInput
{
    public string game_id;
    public int bet;
}