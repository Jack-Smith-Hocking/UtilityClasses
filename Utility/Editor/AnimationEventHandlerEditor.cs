using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Custom.Utility;

namespace Custom.VFX
{
    [CustomEditor(typeof(AnimationEventHandler))]
    public class AnimationEventHandlerEditor : UnityEditor.Editor
    {
        private AnimEventData m_currentlyEditing = null;

        private PhysicMaterial m_material = null;

        private BodyPartMap m_location = BodyPartMap.HEAD;
        
        private string m_animName = "New Event";

        private bool m_isEditing = false;


        public override void OnInspectorGUI()
        {
            AnimationEventHandler _target = target as AnimationEventHandler;

            _target.m_showEditor = EditorGUILayout.ToggleLeft("Show Editor", _target.m_showEditor);

            serializedObject.Update();

            if (!_target.m_showEditor)
            {
                Editor.DrawPropertiesExcluding(serializedObject, new string[] { "m_showEditor" });
                //base.OnInspectorGUI();
            }
            else
            {
                DrawProperties(_target);
                DrawCurrent(_target);
                DrawButtons(_target);
            }

            serializedObject.ApplyModifiedProperties();
        }

        void DrawProperties(AnimationEventHandler target)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(target.m_bodyMapper)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(target.m_prefabHolder)));

            EditorGUILayout.Separator();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(target.m_raycastPoint)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(target.m_raycastLayers)));

            EditorGUILayout.Separator();

            List<AnimEventData> _toRemove = new List<AnimEventData>();
            List<AnimEventData> _toAdd = new List<AnimEventData>();

            foreach (var eventData in target.m_animEvents)
            {
                EditorGUILayout.BeginHorizontal();

                eventData.m_animName = EditorGUILayout.TextField(eventData.m_animName);
                eventData.m_materialType = EditorGUILayout.ObjectField(eventData.m_materialType, typeof(PhysicMaterial), false) as PhysicMaterial;
                eventData.m_location = (BodyPartMap)EditorGUILayout.EnumPopup(eventData.m_location);

                if (GUILayout.Button("Edit"))
                {
                    m_currentlyEditing = eventData;
                    target.m_currentEventData = eventData;
                    m_isEditing = true;
                }
                if (GUILayout.Button("Remove"))
                {
                    _toRemove.Add(eventData);
                }
                if (GUILayout.Button("Clone"))
                {
                    _toAdd.Add(eventData.Clone() as AnimEventData);
                }

                EditorGUILayout.EndHorizontal();
            }

            foreach (var rem in _toRemove)
            {
                RemoveEvent(target, rem);
            }
            foreach (var add in _toAdd)
            {
                AddEvent(target, add);
            }
        }

        void DrawCurrent(AnimationEventHandler target)
        {
            if (m_isEditing)
            {
                EditorGUILayout.Separator();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(target.m_currentEventData)));

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Finish"))
                {
                    m_currentlyEditing = null;
                    target.m_currentEventData = null;
                    m_isEditing = false;
                }
                if (GUILayout.Button("Remove"))
                {
                    RemoveEvent(target, target.m_currentEventData);
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        void DrawButtons(AnimationEventHandler target)
        {
            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();

            m_animName = EditorGUILayout.TextField(m_animName);
            m_material = EditorGUILayout.ObjectField(m_material, typeof(PhysicMaterial)) as PhysicMaterial;
            m_location = (BodyPartMap)EditorGUILayout.EnumPopup(m_location);

            if (GUILayout.Button("Add"))
            {
                AnimEventData _newEvent = new AnimEventData();
                _newEvent.m_animName = m_animName;
                _newEvent.m_materialType = m_material;
                _newEvent.m_location = m_location;

                AddEvent(target, _newEvent);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(8);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Sort (Location : Asc)"))
            {
                target.m_animEvents.Sort(
                    (a, b) =>
                    {
                        return (a.m_location.ToString().CompareTo(b.m_location.ToString()));
                    });
            }
            if (GUILayout.Button("Sort (Location : Desc)"))
            {
                target.m_animEvents.Sort(
                    (a, b) =>
                    {
                        return (b.m_location.ToString().CompareTo(a.m_location.ToString()));
                    });
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Sort (Alphabetic : Asc)"))
            {
                target.m_animEvents.Sort(
                    (a, b) =>
                    {
                        return (a.m_animName.CompareTo(b.m_animName));
                    });
            }
            if (GUILayout.Button("Sort (Alphabetic : Desc)"))
            {
                target.m_animEvents.Sort(
                    (a, b) =>
                    {
                        return (b.m_animName.CompareTo(a.m_animName));
                    });
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Sort (Material : Asc)"))
            {
                target.m_animEvents.Sort(
                    (a, b) =>
                    {
                        string _matNameOne = (a.m_materialType) ? a.m_materialType.name : "aaaa";
                        string _matNameTwo = (b.m_materialType) ? b.m_materialType.name : "aaaa";

                        return _matNameOne.CompareTo(_matNameTwo);
                    });
            }
            if (GUILayout.Button("Sort (Material : Desc)"))
            {
                target.m_animEvents.Sort(
                    (a, b) =>
                    {
                        string _matNameOne = (a.m_materialType) ? a.m_materialType.name : "zzzz";
                        string _matNameTwo = (b.m_materialType) ? b.m_materialType.name : "zzzz";

                        return _matNameTwo.CompareTo(_matNameOne);
                    });
            }

            EditorGUILayout.EndHorizontal();
        }

        void AddEvent(AnimationEventHandler target, AnimEventData eventData)
        {
            if (!GeneralUtil.IsValid(target) || !GeneralUtil.IsValid(eventData)) return;

            target.m_animEvents.Add(eventData);
        }

        void RemoveEvent(AnimationEventHandler target, AnimEventData eventData)
        {
            if (!GeneralUtil.IsValid(target)) return;

            if (GeneralUtil.AreEqual(m_currentlyEditing, eventData))
            {
                m_currentlyEditing = null;
                target.m_currentEventData = null;
                m_isEditing = false;
            }

            target.m_animEvents.Remove(eventData);
        }
    }
}