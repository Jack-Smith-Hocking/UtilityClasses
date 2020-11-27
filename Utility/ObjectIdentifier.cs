using System.Collections.Generic;
using Custom.Utility;
using UnityEngine;

namespace Custom.Utility
{
    public enum ObjectID
    {
        MAIN_CHARACTER,
        MAIN_CAM,
        MINI_MAP,
        MENU,
        LOSE_STATE
    }

    [System.Serializable]
    public class Identifier
    {
        public bool m_useEnumID = true;

        [Header("ID Details")]
        public ObjectID m_enumID = ObjectID.MAIN_CHARACTER;

        public string m_stringID;

        public string GetID()
        {
            return m_useEnumID ? m_enumID.ToString() : m_stringID;
        }
    }

    [DefaultExecutionOrder(-100)]
    public class ObjectIdentifier : MonoBehaviour
    {
        private static Dictionary<string, GameObject> m_objects = new Dictionary<string, GameObject>();

        [Tooltip("Used to identify the string value to store this GameObject at")] public Identifier m_identifier;

        [Header("Overwrite Details")]
        [Tooltip("If there is something with the same ID, should this one overwrite")] public bool m_overwrite = false;
        [Tooltip("Should an overwrite warning be printed?")] public bool m_printOverwriteWarning = true;
        [Tooltip("whether to set this object to inactive after start up")] public bool m_setInactive = true;

        public bool m_isTagged = false;

        void Awake()
        {
            m_isTagged = DictUtil.SetInDictionary(ref m_objects, m_identifier.GetID(), gameObject, m_printOverwriteWarning, m_overwrite);

            if (m_setInactive)
            {
                gameObject.SetActive(false);
            }
        }

        public static GameObject FindObject(string objectIdentifier)
        {
            return DictUtil.GetFromDictionary(m_objects, objectIdentifier, null);
        }
        public static GameObject FindObject(ObjectID objectIdentifier)
        {
            return FindObject(objectIdentifier.ToString());
        }

        public void OnDestroy()
        {
            if (m_isTagged)
            {
                m_objects.Remove(m_identifier.GetID());
            }
        }
    }
}