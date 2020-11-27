using Custom.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Custom.VFX
{
    [System.Serializable]
    public class AnimEventData : ICloneable
    {
        public string m_animName;
        public PhysicMaterial m_materialType = null;
        public BodyPartMap m_location = BodyPartMap.HEAD;

        public List<GameObject> m_prefabs = new List<GameObject>();

        /// <summary>
        /// Create a new instance with the same data
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            AnimEventData _clone = new AnimEventData();

            _clone.m_animName = m_animName;
            _clone.m_materialType = m_materialType;
            _clone.m_location = m_location;
            _clone.m_prefabs = new List<GameObject>(m_prefabs);

            return _clone;
        }

        /// <summary>
        /// Get a random prefab from the list
        /// </summary>
        /// <returns></returns>
        public GameObject GetRandom()
        {
            if (m_prefabs.Count == 0) return null;

            return m_prefabs[UnityEngine.Random.Range(0, m_prefabs.Count)];
        }
        /// <summary>
        /// Spawn a random prefab from the list
        /// </summary>
        /// <param name="mapper">The body mapper associated to the object spawning this prefab</param>
        /// <param name="parent">The object to parent the spawned object to</param>
        public void SpawnRandom(BodyMapper mapper, Transform parent = null)
        {
            if (mapper == null) return;

            GameObject _spawnObject = GetRandom();

            if (_spawnObject != null)
            {
                GameObject.Instantiate(_spawnObject, mapper.GetBodyPart(m_location).position, Quaternion.identity, parent);
            }
        }
    }

    public class AnimationEventHandler : MonoBehaviour
    {
        public BodyMapper m_bodyMapper = null;

        public Transform m_raycastPoint = null;
        public LayerMasker m_raycastLayers = null;

        public Transform m_prefabHolder = null;

        public List<AnimEventData> m_animEvents = new List<AnimEventData>();
        public AnimEventData m_currentEventData = null;

        public bool m_showEditor = true;

        private Table<string, PhysicMaterial, AnimEventData> m_eventTable = new Table<string, PhysicMaterial, AnimEventData>();

        private PhysicMaterial m_currentMaterial = null;

        private float m_lastSpawnTime = 0;

        public void Start()
        {
            // Add all of the defined data to a custom Table class
            foreach (var data in m_animEvents)
            {
                m_eventTable.AddData(data.m_animName, data.m_materialType, data);
            }
        }

        /// <summary>
        /// Event to be called in Animation Events, passed the m_animName of a defined AnimEventData
        /// </summary>
        /// <param name="eventName">The m_animName of a defined AnimEventData</param>
        public void OnEvent(string eventName)
        {
            // Small delay between prefabs being allowed to be spawned
            if (Time.time > m_lastSpawnTime)
            {
                // Check the physics material underneath
                if (m_raycastPoint)
                {
                    RaycastHit _rayHit;
                    if (Physics.Raycast(m_raycastPoint.position, Vector3.down, out _rayHit, 100, m_raycastLayers.MaskValue))
                    {
                        m_currentMaterial = _rayHit.collider.sharedMaterial;
                    }
                }

                // Using custom table class see if there is a prefab to spawn with this eventName and the detected physics material
                AnimEventData _eventData = m_eventTable.GetData(eventName, m_currentMaterial, null);
                if (_eventData != null)
                {
                    _eventData.SpawnRandom(m_bodyMapper, m_prefabHolder);
                }

                m_lastSpawnTime = Time.time + 0.1f;
            }
        }
    }
}