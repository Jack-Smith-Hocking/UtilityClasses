using Custom.Utility;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Jack.Controller
{
    [System.Serializable]
    public class InputChangeData
    {
        [Tooltip("The name of the control scheme to activate on")] public string m_controlSchemeName = "Keyboard";
        public UnityEvent m_onSchemeActivated;
        public UnityEvent m_onSchemeDeactivated;
    }

    public class InputChangeHandler : MonoBehaviour
    {
        [Tooltip("List of events to invoke on change of the current control scheme")]
        public List<InputChangeData> m_inputChangeData = new List<InputChangeData>();

        private Dictionary<string, InputChangeData> m_inputData = new Dictionary<string, InputChangeData>();
        private InputChangeData m_currentInputData = null;

        private void Start()
        {
            foreach (var icd in m_inputChangeData)
            {
                DictUtil.SetInDictionary(ref m_inputData, icd.m_controlSchemeName, icd, true, false);
            }

            InputChangeManager.m_inputHandlers.Add(this);
        }

        /// <summary>
        /// Search a dictionary to check if the scheme that has been changed to exists
        /// If it exists then deactivate the last control scheme and activate the new one
        /// </summary>
        /// <param name="scheme">The scheme being changed to</param>
        public void UpdateToCurrentScheme(string scheme)
        {
            InputChangeData _tempData = m_inputData[scheme];

            if (m_currentInputData != null && _tempData != null)
            {
                // If there is a currently active control scheme that is recognised
                // And the new control scheme is recognised then deactivate old one
                // And then activate the new one
                if (!m_currentInputData.Equals(_tempData))
                {
                    m_currentInputData.m_onSchemeDeactivated.Invoke();

                    m_currentInputData = _tempData;
                    m_currentInputData.m_onSchemeActivated.Invoke();
                }
            }
            else if (m_currentInputData == null && _tempData != null)
            {
                m_currentInputData = _tempData;
                m_currentInputData.m_onSchemeActivated.Invoke();
            }
        }
    }
}