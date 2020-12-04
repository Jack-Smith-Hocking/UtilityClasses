using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Utility.Mobile
{
    public class TouchInputWrapper : MonoBehaviour
    {
        public static TouchInputWrapper Instance
        {
            get
            {
                if (m_instance == null)
                {
                    GameObject _inputManager = Instantiate(new GameObject("TouchInputManager"));
                    m_instance = _inputManager.AddComponent<TouchInputWrapper>();
                }

                return m_instance;
            }
        }

        public static bool MainInputPresent { get; private set; }
        public static bool MainInputPressed { get; private set; }
        public static bool MainInputDown { get; private set; }

        public Vector2 MainPositionDelta { get; private set; } = Vector2.zero;
        public System.Action m_mainInputActivated = () => { };

        private static TouchInputWrapper m_instance = null;

        private Vector2 m_previousPosition;
        private bool m_trackingPosition = false;

        private void Awake()
        {
            if (m_instance != null)
            {
                Destroy(gameObject);
            }
            else
            {
                m_instance = this;
            }
        }

        public void Update()
        {
            MainInputPresent = ValidInputPresent();

            StartCoroutine(TrackMainInputPhase());
            TrackMainPositionDelta();
        }

        /// <summary>
        /// Return whether the screen has at least one touch on it, or there is a mouse
        /// </summary>
        /// <returns></returns>
        public static bool ValidInputPresent()
        {
            if (Application.isMobilePlatform)
            {
                return Input.touchCount > 0;
            }

            return true;
        }

        /// <summary>
        /// Get the position of the mouse or the touch on the screen that is determined as the main
        /// </summary>
        /// <param name="inputPresent">Whether there is actually an input on the screen at all</param>
        /// <returns></returns>
        public static Vector2 GetMainInputPosition(out bool inputPresent)
        {
            inputPresent = ValidInputPresent();

            if (Application.isMobilePlatform)
            {
                if (Input.touchCount > 0)
                {
                    return Input.GetTouch(0).position;
                }
            }
            else
            {
                return Input.mousePosition;
            }

            return Vector2.zero;
        }
        /// <summary>
        /// Get a list of all the positions of touches (or the singular mouse position)
        /// </summary>
        /// <returns></returns>
        public static List<Vector2> GetAllInputPositions()
        {
            List<Vector2> _positions = new List<Vector2>();

            if (Application.isMobilePlatform)
            {
                _positions.Capacity = Input.touchCount;

                foreach (var touch in Input.touches)
                {
                    _positions.Add(touch.position);
                }
            }
            else
            {
                _positions.Add(Input.mousePosition);
            }

            return _positions;
        }

        #region AndroidSpecific
#if UNITY_ANDROID
        /// <summary>
        /// Returns whether there is a valid amount of touches on the screen
        /// </summary>
        /// <param name="touchID">The amount of touches on the screen</param>
        /// <returns></returns>
        public static bool ValidTouchPresent(int touchID = 0)
        {
            return Input.touchCount > touchID && touchID >= 0;
        }
        /// <summary>
        /// Get the position of a touch on the screen
        /// </summary>
        /// <param name="touchID">The touch to check</param>
        /// <param name="pos">Position of the touch</param>
        /// <returns></returns>
        public static bool GetTouchPosition(int touchID, out Vector2 pos)
        {
            bool _validTouch = ValidInputPresent();

            if (_validTouch)
            {
                pos = Input.GetTouch(touchID).position;
            }
            else
            {
                pos = Vector2.zero;
            }

            return _validTouch;
        }

        /// <summary>
        /// Tries to return a valid Touch if there is one
        /// </summary>
        /// <param name="touchID">The touch on the screen to get</param>
        /// <param name="touch">The touch that was retrieved</param>
        /// <returns></returns>
        public static bool GetTouch(int touchID, out Touch touch)
        {
            bool _validTouch = ValidTouchPresent(touchID);

            if (_validTouch)
            {
                touch = Input.GetTouch(touchID);
            }
            else
            {
                touch = new Touch();
            }

            return _validTouch;
        }
#endif
        #endregion
        
        #region Tracking
        /// <summary>
        /// Check whether the main input is down or up (touch on the screen or left click)
        /// </summary>
        /// <returns></returns>
        private IEnumerator TrackMainInputPhase()
        {
            // If there is no touch and left click is up, then the main input is not down
            if (Input.GetMouseButtonUp(0) || Input.touchCount == 0)
            {
                MainInputDown = false;
            }

            // Check if the main input was pressed this frame
            if (Input.GetMouseButtonDown(0) || Input.touchCount > 0 && !MainInputDown)
            {
                m_mainInputActivated?.Invoke();

                MainInputPressed = true;
                MainInputDown = true;
            }

            if (MainInputPressed)
            {
                yield return new WaitForEndOfFrame();

                MainInputPressed = false;
            }
        }
        /// <summary>
        /// Calculate the delta between the main inputs position this frame and last frame (if it was down)
        /// </summary>
        private void TrackMainPositionDelta()
        {
            // If there is a valid input on the screen start tracking the position delta
            if (ValidInputPresent())
            {
                bool _validInput;

                if (!m_trackingPosition)
                {
                    m_previousPosition = GetMainInputPosition(out _validInput);
                    m_trackingPosition = true;
                }

                MainPositionDelta = (m_previousPosition - GetMainInputPosition(out _validInput)).normalized;

                m_previousPosition = GetMainInputPosition(out _validInput);
            }
            else
            {
                m_trackingPosition = false;
                MainPositionDelta = Vector2.zero;
            }
        }
        #endregion

        #region PropertyGetters
        public static bool GetMainInputPressed()
        {
            return MainInputPressed;
        }
        public static bool GetMainInputDown()
        {
            return MainInputDown;
        }
        public static bool GetMainInputPresent()
        {
            return MainInputPresent;
        }
        #endregion
    }
}