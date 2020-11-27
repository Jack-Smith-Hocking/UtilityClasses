using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Custom.Utility.DelayedEvents.V2
{
    public enum TriggerState
    { 
        ON_ENTER,
        ON_STAY,
        ON_EXIT
    }

    /// <summary>
    /// Holds the data for an event trigger
    /// </summary>
    [System.Serializable]
    public class EventData : ICloneable
    {
        [Header("List Details")]
        [Tooltip("Identifier for big lists, changes the name of the element")] public string eventName = "New Event";

        [Header("Event Details")]
        [Tooltip("How long after the previous event should be waited")] public float eventDelay = 0;


        [Tooltip("Triggered by Enter, after delay")] public UnityEvent onEnterEvent;
        [Tooltip("Triggered by Stay, after delay")] public UnityEvent onStayEvent;
        [Tooltip("Triggered by Exit, after delay")] public UnityEvent onExitEvent;

        public object Clone()
        {
            EventData _newEvent = new EventData();

            _newEvent.eventName = eventName;
            _newEvent.eventDelay = eventDelay;
            
            _newEvent.onEnterEvent = onEnterEvent;
            _newEvent.onStayEvent = onStayEvent;
            _newEvent.onExitEvent = onExitEvent;

            return _newEvent;
        }
    }

    /// <summary>
    /// Triggers a set of events one after another
    /// </summary>
    public class DelayedEvents : MonoBehaviour
    {
        [Tooltip("The collider to trigger events")] public Collider m_triggeringCollider = null;

        [TextArea]
        [Tooltip("List of valid tags for collisions (separate by ','), only GameObjects with these tags will invoke events")] public string collisionTags = "";

        [Tooltip("Enables Enter triggers")] public bool onEnter = true;
        [Tooltip("Enables Stay triggers")] public bool onStay = true;
        [Tooltip("Enables Exit triggers")] public bool onExit = true;

        [Tooltip("How long after triggering to be able to re-trigger")] public float m_triggerCooldown = 0;

        public bool showEditor = true;
        
        public List<EventData> events = new List<EventData>();
        public EventData currentEvent = null;

        private List<string> validCollisionTags = new List<string>();

        private float m_lastTrigerTime = 0;

        private void Start()
        {
            string[] _tags = collisionTags.Split(',');
            for (int i = 0; i < _tags.Length; i++)
            {
                _tags[i] = _tags[i].Trim();
                validCollisionTags.Add(_tags[i]);
            }
        }

        /// <summary>
        /// Starts triggering the events
        /// </summary>
        private void StartEventChain(Action<EventData> action)
        {
            for (int i = 0; i < events.Count; i++)
            {
                StartCoroutine(FireEvent(action, events[i]));
            }
        }

        /// <summary>
        /// Fire an event (Enter, Stay or Exit) after a delay
        /// </summary>
        /// <param name="action">Action to perform on an event</param>
        /// <param name="ed">The EventData to act upon</param>
        /// <returns></returns>
        IEnumerator FireEvent(Action<EventData> action, EventData ed)
        {
            if (ed == null) yield return null;

            yield return new WaitForSeconds(ed.eventDelay);

            action?.Invoke(ed);
        }

        private void TriggerEvent(string tag, bool required, TriggerState triggerState)
        {
            if (!required) return;

            if (Time.time > (m_lastTrigerTime + m_triggerCooldown))
            {
                m_lastTrigerTime = Time.time;
            }
            else
            {
                return;
            }

            if (validCollisionTags.Contains(tag))
            {
                switch (triggerState)
                {
                    case TriggerState.ON_ENTER:
                        // Start the chain and invoke enter events
                        StartEventChain((EventData ed) => { ed.onEnterEvent?.Invoke(); });
                        break;
                    
                    case TriggerState.ON_STAY:
                        // Start the chain and invoke stay events
                        StartEventChain((EventData ed) => { ed.onStayEvent?.Invoke(); });
                        break;
                    
                    case TriggerState.ON_EXIT:
                        // Start the chain and invoke exit events
                        StartEventChain((EventData ed) => { ed.onExitEvent?.Invoke(); });
                        break;

                    default:
                        break;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            TriggerEvent(other.tag, onEnter, TriggerState.ON_ENTER);
        }
        private void OnTriggerStay(Collider other)
        {
            TriggerEvent(other.tag, onStay, TriggerState.ON_STAY);
        }
        private void OnTriggerExit(Collider other)
        {
            TriggerEvent(other.tag, onExit, TriggerState.ON_EXIT);
        }

        private void OnCollisionEnter(Collision collision)
        {
            TriggerEvent(collision.gameObject.tag, onEnter, TriggerState.ON_ENTER);
        }
        private void OnCollisionStay(Collision collision)
        {
            TriggerEvent(collision.gameObject.tag, onStay, TriggerState.ON_STAY);
        }
        private void OnCollisionExit(Collision collision)
        {
            TriggerEvent(collision.gameObject.tag, onExit, TriggerState.ON_EXIT);
        }
    }
}