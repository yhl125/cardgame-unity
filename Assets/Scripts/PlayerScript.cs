using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScript : MonoBehaviour
{
    public CardScript cardScript;
    public DeckScript deckScript;

    private int money = 1000; // current money

    // Array of card objects on table
    public GameObject[] hand;
    // Index of next card to be turned over
    public int cardIndex = 0;

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
        int[] recieved_hand = { 0, 1, 2 };
        for (int i=0; i < recieved_hand.Length; i++)
        {
            deckScript.AddCard(hand[cardIndex].GetComponent<CardScript>(), recieved_hand[i]);
            hand[cardIndex].GetComponent<Renderer>().enabled = true;
            cardIndex++;
        }
    }
    // Get current Money from server
    public void GetMoney()
    {
        int amount = 0;
        money = amount;
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
}
