using UnityEngine;

public class DeckScript : MonoBehaviour
{
    public Sprite[] cardSprites;

    public void AddCard(CardScript cardScript, int value)
    {
        cardScript.SetSprite(cardSprites[value]);
        cardScript.SetValue(value);
    }

    public Sprite GetCardBack()
    {
        return cardSprites[0];
    }
}