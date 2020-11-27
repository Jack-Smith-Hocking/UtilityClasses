using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DelayEvent : MonoBehaviour
{
    [Min(0)] public float m_delay = 0;
    public UnityEvent m_event;

    public void UnscaledDelay()
    {
        StartCoroutine(UnscaledDelay(m_delay));
    }
    public void ScaledDelay()
    {
        StartCoroutine(ScaledDelay(m_delay));
    }

    public IEnumerator ScaledDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        m_event.Invoke();
    }
    public IEnumerator UnscaledDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        m_event.Invoke();
    }
}
