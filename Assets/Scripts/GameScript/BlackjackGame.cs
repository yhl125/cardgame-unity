using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

[Serializable]
public class BlackjackGame
{
    public string _id;
    public string name;
    public List<BlackjackPlayer> players;
    public List<Card> deck;
    public List<Card> dealerHand;
    public string status;
    public string playedCount;
}

[Serializable]
public class BlackjackPlayer
{
    public string name;
    public List<Card> hand;
    public int bet;
    public string status;
}

[Serializable]
public class Card
{
    public string rank;
    public string suit;
}
