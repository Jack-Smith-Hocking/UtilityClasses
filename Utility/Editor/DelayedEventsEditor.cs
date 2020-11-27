using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Custom.Utility.DelayedEvents.V2
{
    [CustomEditor(typeof(DelayedEvents))]
    public class DelayedEventsEditor : UnityEditor.Editor
    {
        string eventToModify = "New Event";
        string tagField = "";

        float eventToAddDelay = 0;

        bool isEditing = false;

        EventData currentlyEditing = null;

        public override void OnInspectorGUI()
        {
            DelayedEvents _target = target as DelayedEvents;

            if (!_target.showEditor)
            {
                _target.showEditor = EditorGUILayout.BeginToggleGroup("Show Editor", _target.showEditor);
                EditorGUILayout.EndToggleGroup();

                base.OnInspectorGUI();
                return;
            }

            serializedObject.Update();

            _target.showEditor = EditorGUILayout.BeginToggleGroup("Show Editor", _target.showEditor);

            GUIStyle _labelStyle = new GUIStyle();
            _labelStyle.fontStyle = FontStyle.Bold;
            _labelStyle.alignment = TextAnchor.MiddleCenter;

            EditorGUILayout.LabelField("Collider", _labelStyle);
            
            EditorGUILayout.BeginHorizontal();
            
            _target.m_triggeringCollider = EditorGUILayout.ObjectField(_target.m_triggeringCollider, typeof(Collider)) as Collider;
            if (_target.m_triggeringCollider != null)
            {
                _target.m_triggeringCollider.isTrigger = EditorGUILayout.ToggleLeft("Is Trigger", _target.m_triggeringCollider.isTrigger);
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Separator();

            DrawStateButtons(_target);
            DrawProperties(_target);

            //GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Collision Tags", _labelStyle);
            EditorGUILayout.TextArea(_target.collisionTags);
            //GUILayout.EndHorizontal();

            #region EventFunctionality
            GUILayout.Space(8);

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Sort (Asc)"))
            {
                _target.events.Sort((a, b) => { return a.eventDelay.CompareTo(b.eventDelay); });
            }
            if (GUILayout.Button("Sort (Desc)"))
            {
                _target.events.Sort((a, b) => { return b.eventDelay.CompareTo(a.eventDelay); });
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(8);

            GUILayout.BeginHorizontal();

            GUILayout.BeginVertical();
            GUILayout.Label("Event Name", _labelStyle);
            eventToModify = EditorGUILayout.TextField(eventToModify);
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            GUILayout.Label("Event Delay", _labelStyle);
            eventToAddDelay = EditorGUILayout.FloatField(eventToAddDelay);
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();

            if (GUILayout.Button("Add Event"))
            {
                AddEvent(eventToModify, eventToAddDelay, _target);
            }
            if (GUILayout.Button("Remove Event"))
            {
                RemoveEvent(eventToModify, _target);
            }

            GUILayout.EndHorizontal();
            #endregion

            #region TagFunctionality
            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            tagField = EditorGUILayout.TagField(tagField);
            if (GUILayout.Button("Add Tag"))
            {
                AddTag(tagField, _target);
            }
            if (GUILayout.Button("Remove Tag"))
            {
                RemoveTag(tagField, _target);
            }
            if (GUILayout.Button("Clear Tags"))
            {
                _target.collisionTags = "";
            }
            EditorGUILayout.EndHorizontal();
            #endregion

            EditorGUILayout.EndToggleGroup();

            serializedObject.ApplyModifiedProperties();
        }

        void DrawStateButtons(DelayedEvents delayedEvents)
        {
            EditorGUILayout.BeginHorizontal();

            GUIStyle _enterStyle = new GUIStyle(GUI.skin.button);
            _enterStyle.fontStyle = (delayedEvents.onEnter) ? FontStyle.Bold : FontStyle.Normal;
            _enterStyle.normal.textColor = (delayedEvents.onEnter) ? Color.blue : Color.red;
            if (GUILayout.Button("OnEnter", _enterStyle))
            {
                delayedEvents.onEnter = !delayedEvents.onEnter;
            }

            _enterStyle.fontStyle = (delayedEvents.onStay) ? FontStyle.Bold : FontStyle.Normal;
            _enterStyle.normal.textColor = (delayedEvents.onStay) ? Color.blue : Color.red;
            if (GUILayout.Button("OnStay", _enterStyle))
            {
                delayedEvents.onStay = !delayedEvents.onStay;
            }

            _enterStyle.fontStyle = (delayedEvents.onExit) ? FontStyle.Bold : FontStyle.Normal;
            _enterStyle.normal.textColor = (delayedEvents.onExit) ? Color.blue : Color.red;
            if (GUILayout.Button("OnExit", _enterStyle))
            {
                delayedEvents.onExit = !delayedEvents.onExit;
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Separator();
        }

        void DrawProperties(DelayedEvents delayedEvents)
        {
            EditorGUILayout.Separator();

            delayedEvents.m_triggerCooldown = EditorGUILayout.FloatField("Re-Trigger Cooldown", delayedEvents.m_triggerCooldown);
            
            EditorGUILayout.Separator();

            int _indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel += 1;

            float _labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 50;

            EventData eventData = null;
            for (int i = 0; i < delayedEvents.events.Count; i++)
            {
                eventData = delayedEvents.events[i];

                GUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(eventData.eventName);
                eventData.eventDelay = EditorGUILayout.FloatField(eventData.eventDelay);

                if (GUILayout.Button("Edit"))
                {
                    currentlyEditing = eventData;
                    delayedEvents.currentEvent = eventData.Clone() as EventData;
                    isEditing = true;
                }
                if (GUILayout.Button("Remove"))
                {
                    RemoveEvent(eventData, delayedEvents);

                    if (eventData.Equals(currentlyEditing))
                    {
                        delayedEvents.currentEvent = null;
                        isEditing = false;
                    }
                }
                if (GUILayout.Button("Duplicate"))
                {
                    AddEvent(eventData.Clone() as EventData, delayedEvents);
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }

            EditorGUI.indentLevel = _indent;
            EditorGUIUtility.labelWidth = _labelWidth;

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginToggleGroup("Is Editing", isEditing);
            var _current = serializedObject.FindProperty(nameof(delayedEvents.currentEvent));
            EditorGUILayout.PropertyField(_current, isEditing);
            EditorGUILayout.EndToggleGroup();

            if (isEditing)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Finish Editing"))
                {
                    for (int i = 0; i < delayedEvents.events.Count; i++)
                    {
                        if (delayedEvents.events[i].Equals(currentlyEditing))
                        {
                            delayedEvents.events[i] = delayedEvents.currentEvent;
                            break;
                        }
                    }

                    delayedEvents.currentEvent = null;
                    isEditing = false;
                }
                if (GUILayout.Button("Remove Event"))
                {
                    RemoveEvent(currentlyEditing, delayedEvents);

                    delayedEvents.currentEvent = null;
                    isEditing = false;
                }
                GUILayout.EndHorizontal();
            }

            EditorGUILayout.Space(8);
        }

        void RemoveTag(string tagName, DelayedEvents delayedEvents)
        {
            if (delayedEvents == null) return;

            List<string> _tags = new List<string>(delayedEvents.collisionTags.Split(','));

            for (int i = 0; i < _tags.Count; i++)
            {
                _tags[i] = _tags[i].Trim();
            }

            _tags.Remove(tagName);

            string _formattedTags = "";

            for (int i = 0; i < _tags.Count; i++)
            {
                _formattedTags += _tags[i];

                if (i < _tags.Count - 1)
                {
                    _formattedTags += ", ";
                }
            }

            delayedEvents.collisionTags = _formattedTags;
        }
        void AddTag(string tagName, DelayedEvents delayedEvents)
        {
            if (delayedEvents == null) return;

            string _tags = delayedEvents.collisionTags;
            _tags = _tags.Trim();

            if (!_tags.Contains(tagName))
            {
                if (_tags.Length == 0 || _tags[_tags.Length - 1] == ',')
                {
                    _tags += tagName;
                }
                else
                {
                    _tags += ", " + tagName;
                }

                delayedEvents.collisionTags = _tags;
            }
        }

        void AddEvent(string eventName, float delay, DelayedEvents delayedEvents)
        {
            if (delayedEvents == null) return;

            EventData _eventData = new EventData();
            _eventData.eventName = eventName;
            _eventData.eventDelay = delay;

            delayedEvents.events.Add(_eventData);
        }
        void AddEvent(EventData eventData, DelayedEvents delayedEvents)
        {
            if (delayedEvents == null || eventData == null) return;

            delayedEvents.events.Add(eventData);
        }

        void RemoveEvent(string eventName, DelayedEvents delayedEvents)
        {
            if (delayedEvents == null) return;

            int _removalIndex = -1;
            EventData _eventData = null;

            for (int i = 0; i < delayedEvents.events.Count; i++)
            {
                _eventData = delayedEvents.events[i];

                if (_eventData.eventName.Equals(eventName))
                {
                    _removalIndex = i;
                    break;
                }
            }

            if (_removalIndex >= 0)
            {
                delayedEvents.events.RemoveAt(_removalIndex);
            }
        }
        void RemoveEvent(int index, DelayedEvents delayedEvents)
        {
            if (delayedEvents == null) return;

            delayedEvents.events.RemoveAt(index);
        }
        void RemoveEvent(EventData eventData, DelayedEvents delayedEvents)
        {
            if (delayedEvents == null) return;

            delayedEvents.events.Remove(eventData);
        }
    }
}