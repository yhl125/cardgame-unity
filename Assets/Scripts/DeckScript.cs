using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class DeckScript : MonoBehaviour
{
    public Sprite[] cardSprites;

    //댁의 인덱스에 해당하는 값으로 card에 값과 이미지를 저장
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
