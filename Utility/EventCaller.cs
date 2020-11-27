using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EventCaller : MonoBehaviour
{
    public UnityEvent m_activatedEvent;

    public void OnActivate()
    {
        m_activatedEvent?.Invoke();
    }
}
