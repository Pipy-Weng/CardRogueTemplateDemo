using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDataSO", menuName = "Cards/CardDataSO")]
public class CardDataSO : ScriptableObject
{
    public string cardName;

    public Sprite cardImage;

    public int cardCost;

    public CardType cardType;

    [TextArea]
    public string description;

    //TODO:¿¨ÅÆÐ§¹û
    public List<Effect> effects;

}
