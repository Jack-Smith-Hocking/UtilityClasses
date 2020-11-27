using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Jack.Controller
{
    public class InputChangeManager : MonoBehaviour
    {
        public static InputChangeManager Instance = null;
        public static List<InputChangeHandler> m_inputHandlers = new List<InputChangeHandler>();

        [Tooltip("Leave null to search for first PlayerInput, will dictate what to check for current scheme")] public PlayerInput m_playerInput = null;

        public System.Action<string, string> m_onSchemeChange = (o, n) => { };

        private string m_currentScheme;

        // Start is called before the first frame update
        void Awake()
        {
            Instance = this;
        }

        private IEnumerator Start()
        {
            yield return new WaitForEndOfFrame();

            if (m_playerInput == null)
            {
                m_playerInput = PlayerInput.all[0];

                if (m_playerInput != null)
                {
                    m_onSchemeChange?.Invoke(m_currentScheme, m_playerInput.currentControlScheme);
                    m_currentScheme = m_playerInput.currentControlScheme;
                    UpdateInputHandlers();
                }
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (m_playerInput != null)
            {
                if (m_playerInput.currentControlScheme != m_currentScheme)
                {
                    m_currentScheme = m_playerInput.currentControlScheme;
                    UpdateInputHandlers();
                }
            }
        }

        void UpdateInputHandlers()
        {
            foreach (var handler in m_inputHandlers)
            {
                handler.UpdateToCurrentScheme(m_currentScheme);
            }
        }
    }
}