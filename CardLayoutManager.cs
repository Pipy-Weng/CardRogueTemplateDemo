using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class CardLayoutManager : MonoBehaviour
{
    public bool isHorizontal; //横向排列或者扇形排列

    public float maxWidth = 7f;
    public float cardSpacing = 2f; //卡牌和卡牌之间的距离

    [Header("弧形参数")]
    public float angleBetweenCards = 7f;
    public float radius = 17f;


    public Vector3 centerPoint;

    [SerializeField]private List<Vector3> cardPositions = new(); //卡牌的坐标
    [SerializeField]private List<Quaternion> cardRotations = new(); //卡牌的角度

    private void Awake()
    {
        centerPoint = isHorizontal ? Vector3.up * -4.5f : Vector3.up * -21.5f; //根据是否为弧形设置不同中心点
    }

    public CardTransform GetCardTransform(int index, int totalCards)
    {
        CalculatePositions(totalCards, isHorizontal);
        return new CardTransform(cardPositions[index], cardRotations[index]);
    }



    private void CalculatePositions(int numberOfCards, bool horizontal)
    {
        cardPositions.Clear();
        cardRotations.Clear();
        if (horizontal) {  //横向排列时
            float currentWidth = cardSpacing * (numberOfCards - 1); //卡牌越多越宽
            float totalWidth = Mathf.Min(currentWidth, maxWidth); //把卡牌的宽度限制在最大为maxWidth

            float currentSpacing = totalWidth > 0 ? totalWidth / (numberOfCards -1) : 0; //卡牌越多，间隙越小

            for (int i = 0; i < numberOfCards; i++) { //排列所有卡牌
                float xPos = 0 - (totalWidth / 2) + (i * currentSpacing); //设置每张牌的x坐标值

                var pos = new Vector3(xPos, centerPoint.y, centerPoint.z);
                var rotation = Quaternion.identity;
                cardPositions.Add(pos);
                cardRotations.Add(rotation);
            }
            
        } else //扇形排列
        {
            float cardAngle = (numberOfCards - 1) * angleBetweenCards / 2;

            for (int i = 0; i < numberOfCards; ++i) { //排列所有卡牌
                var pos = FanCardPosition(cardAngle - i * angleBetweenCards);
                var rotation = Quaternion.Euler(0,0,cardAngle - i*angleBetweenCards);
                cardPositions.Add(pos);
                cardRotations.Add(rotation);
            }
        }
    }

    private Vector3 FanCardPosition(float angle)
    {

        return new Vector3(centerPoint.x - Mathf.Sin(Mathf.Deg2Rad * angle) * radius,
                           centerPoint.y + Mathf.Cos(Mathf.Deg2Rad * angle) * radius,
                           0
            );
    }

}
