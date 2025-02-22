using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BaseEventSO<T> : ScriptableObject
{
    public string description; //添加事件描述


    public UnityAction<T> OnEventRaised; //当事件被发起时

    public string lastSender;

    public void RaiseEvent(T value, object sender)
    {
        OnEventRaised?.Invoke(value);
        lastSender = sender.ToString();
    }
}
