using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class CardLayoutManager : MonoBehaviour
{
    public bool isHorizontal; //�������л�����������

    public float maxWidth = 7f;
    public float cardSpacing = 2f; //���ƺͿ���֮��ľ���

    [Header("���β���")]
    public float angleBetweenCards = 7f;
    public float radius = 17f;


    public Vector3 centerPoint;

    [SerializeField]private List<Vector3> cardPositions = new(); //���Ƶ�����
    [SerializeField]private List<Quaternion> cardRotations = new(); //���ƵĽǶ�

    private void Awake()
    {
        centerPoint = isHorizontal ? Vector3.up * -4.5f : Vector3.up * -21.5f; //�����Ƿ�Ϊ�������ò�ͬ���ĵ�
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
        if (horizontal) {  //��������ʱ
            float currentWidth = cardSpacing * (numberOfCards - 1); //����Խ��Խ��
            float totalWidth = Mathf.Min(currentWidth, maxWidth); //�ѿ��ƵĿ�����������ΪmaxWidth

            float currentSpacing = totalWidth > 0 ? totalWidth / (numberOfCards -1) : 0; //����Խ�࣬��϶ԽС

            for (int i = 0; i < numberOfCards; i++) { //�������п���
                float xPos = 0 - (totalWidth / 2) + (i * currentSpacing); //����ÿ���Ƶ�x����ֵ

                var pos = new Vector3(xPos, centerPoint.y, centerPoint.z);
                var rotation = Quaternion.identity;
                cardPositions.Add(pos);
                cardRotations.Add(rotation);
            }
            
        } else //��������
        {
            float cardAngle = (numberOfCards - 1) * angleBetweenCards / 2;

            for (int i = 0; i < numberOfCards; ++i) { //�������п���
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
