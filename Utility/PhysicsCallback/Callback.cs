using UnityEngine;
using System;
using UnityEngine.Events;

namespace Custom.Utility
{
    public abstract class Callback<T> : MonoBehaviour
    {
        [Header("Unity Events")]
        [Tooltip("Callback for when this object enters a trigger/collision")] public UnityEvent m_onEnterEvent = null;
        [Tooltip("Callback for when this object exits a trigger/collision")] public UnityEvent m_onExitEvent = null;
        [Tooltip("Callback for when this object stays in a trigger/collision")] public UnityEvent m_onStayEvent = null;

        public Action<T> m_onEnterAction = (T data) => { };
        public Action<T> m_onExitAction = (T data) => { };
        public Action<T> m_onStayAction = (T data) => { };

        public virtual void OnEnter(T info)
        {
            m_onEnterEvent?.Invoke();
            m_onEnterAction?.Invoke(info);
        }
        public virtual void OnStay(T info)
        {
            m_onStayEvent?.Invoke();
            m_onStayAction?.Invoke(info);
        }
        public virtual void OnExit(T info)
        {
            m_onExitEvent?.Invoke();
            m_onExitAction?.Invoke(info);
        }
    }
}