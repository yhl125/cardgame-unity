using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class DeckScript : MonoBehaviour
{
    public Sprite[] cardSprites;

    //���� �ε����� �ش��ϴ� ������ card�� ���� �̹����� ����
    public void AddCard(CardScript cardScript,int value)
    {
        cardScript.SetSprite(cardSprites[value]);
        cardScript.SetValue(value);
    }

    public Sprite GetCardBack()
    {
        return cardSprites[0];
    }
}
