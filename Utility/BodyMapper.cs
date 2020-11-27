using System;
using UnityEngine;

namespace Custom.Utility
{
    public enum BodyPartMap
    {
        HEAD,

        UPPER_BODY,
        LOWER_BODY,

        RIGHT_ARM,
        RIGHT_HAND,
        RIGHT_LEG,
        RIGHT_FOOT,

        LEFT_ARM,
        LEFT_HAND,
        LEFT_LEG,
        LEFT_FOOT
    }

    public class BodyMapper : MonoBehaviour
    {
        [SerializeField]
        public Transform[] m_transforms = null;

        public Transform GetBodyPart(BodyPartMap bodyPart)
        {
            return m_transforms[(int)bodyPart];
        }
    }
}