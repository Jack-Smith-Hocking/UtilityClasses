using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;


namespace Custom.Utility
{
    // Inputs
    #region InputOperations
    /// <summary>
    /// Binds and unbinds actions to inputs
    /// </summary>
    public class BoundInput
    {
        public enum BindCode
        {
            SUCCESS, FAILURE, INVALID_MAP, INVALID_ACTION, RE_BIND_SUCCEEDED
        }

        /// <summary>
        /// The currently bound input action
        /// </summary>
        public InputAction m_inputAction { get; private set; } = null;

        public List<InputAction> m_otherBoundInputs { get; private set; } = new List<InputAction>();

        /// <summary>
        /// The actions to be added to the 'performed' callback of the input
        /// </summary>
        public Action<InputAction.CallbackContext> m_performedActions = (InputAction.CallbackContext cc) => { return; };
        /// <summary>
        /// The actions to be add to the 'cancelled' callback of the input
        /// </summary>
        public Action<InputAction.CallbackContext> m_cancelledActions = (InputAction.CallbackContext cc) => { return; };

        #region CurrentValues
        public float CurrentFloatVal
        {
            get
            {
                float _val = 0;
                if (m_inputAction != null) _val = m_inputAction.ReadValue<float>();

                float _tempVal = 0;
                Action<InputAction> _loopAction = (InputAction action) =>
                {
                    _tempVal = action.ReadValue<float>();

                    if (_tempVal > _val) _val = _tempVal;
                };

                ListUtil.LoopList<InputAction>(m_otherBoundInputs, _loopAction);

                return _val;
            }
        }
        public bool CurrentBoolVal
        {
            get
            {
                bool _val = false;
                if (m_inputAction != null) _val = m_inputAction.ReadValue<float>() >= 0.01f;

                ListUtil.LoopList<InputAction>(m_otherBoundInputs, (InputAction action) => { if (!_val) _val = action.ReadValue<float>() >= 0.01f; }, () => { return _val; });

                return _val;
            }
        }
        public Vector2 CurrentVec2Val
        {
            get
            {
                Vector2 _val = Vector2.zero;
                if (m_inputAction != null) _val = m_inputAction.ReadValue<Vector2>();

                Vector2 _tempVal = Vector2.zero;
                Action<InputAction> _loopAction = (InputAction action) =>
                {
                    _tempVal = action.ReadValue<Vector2>();

                    if (_tempVal.x > _val.x) _val.x = _tempVal.x;
                    if (_tempVal.y > _val.y) _val.y = _tempVal.y;
                };

                ListUtil.LoopList<InputAction>(m_otherBoundInputs, _loopAction);

                return _val;
            }
        }

        public bool IsHeld { get; private set; } = false;
        public bool IsToggled { get; private set; } = false;
        #endregion

        public void AddDefaultActions()
        {
            m_inputAction.performed += (InputAction.CallbackContext cc) =>
            {
                IsHeld = true;
            };
            m_inputAction.canceled += (InputAction.CallbackContext cc) =>
            {
                IsHeld = false;
            };
            m_inputAction.performed += (InputAction.CallbackContext cc) =>
            {
                IsToggled = !IsToggled;
            };
        }

        #region Bind
        /// <summary>
        /// Bind an action to an InputAction on the passed in PlayerInput with the name 'actionName', remember to bind actions to "PerformedActions" and "CancelledActions" before calling this
        /// </summary>
        /// <param name="playerInput">The PlayerInput to get input for</param>
        /// <param name="actionName">The name of the action in the control map</param>
        /// <param name="deleteOldBindings">Whether or not to keep previous bindings for this action</param>
        /// <returns></returns>
        public BindCode Bind(PlayerInput playerInput, string actionName, bool deleteOldBindings = true)
        {
            BindCode _bindCode = BindCode.SUCCESS;

            if (playerInput == null)
            {
                return BindCode.INVALID_MAP;
            }
            else
            {
                // Get the InputAction of name 'actionName' from the playerInput
                InputAction _tempAction = playerInput.currentActionMap.FindAction(actionName, false);

                _bindCode = Bind(_tempAction, deleteOldBindings);
            }


            if (_bindCode != BindCode.SUCCESS && _bindCode != BindCode.RE_BIND_SUCCEEDED)
            {
                Debug.LogWarning($"There was an error attempting to rebind action '{actionName}', error code: {_bindCode}");
            }

            return _bindCode;
        }

        public BindCode Bind(InputActionReference actionRef, bool deleteOldBindings = true)
        {
            if (actionRef == null)
            {
                Debug.LogWarning($"There was an issue attempting to bind input, Error Code: {BindCode.INVALID_ACTION.ToString()}");

                return BindCode.INVALID_ACTION;
            }

            return Bind(actionRef.action, deleteOldBindings);
        }

        public BindCode Bind(InputAction action, bool deleteOldBindings = true)
        {
            BindCode _bindCode = BindCode.SUCCESS;

            if (action == null)
            {
                Debug.LogWarning($"There was an issue attempting to bind input, Error Code: {BindCode.INVALID_ACTION.ToString()}");
                return BindCode.INVALID_ACTION;
            }

            // If the InputAction was successfully obtained continue
            if (m_inputAction != null && deleteOldBindings)
            {
                UnbindAll();

                m_otherBoundInputs.Clear();

                _bindCode = BindCode.RE_BIND_SUCCEEDED;
            }
            else if (m_inputAction != null && !deleteOldBindings)
            {
                // If we don't want to delete the old bindings, this will add an alternate binding 
                m_otherBoundInputs.Add(m_inputAction);

                _bindCode = BindCode.RE_BIND_SUCCEEDED;
            }

            m_inputAction = action;

            if (m_inputAction != null)
            {
                // Makes sure these fire first
                AddDefaultActions();
                m_inputAction.performed += m_performedActions;
                m_inputAction.canceled += m_cancelledActions;
            }

            return _bindCode;
        }
        #endregion
        public void Unbind(InputActionReference inputAction)
        {
            if (inputAction)
            {
                Unbind(inputAction.action);
            }
        }
        public void Unbind(InputAction inputAction)
        {
            if (inputAction != null)
            {
                inputAction.performed -= m_performedActions;
                inputAction.canceled -= m_cancelledActions;
            }
        }
        public void UnbindAll()
        {
            // Remove all of the actions from any previous bindings
            Unbind(m_inputAction);
            ListUtil.LoopList<InputAction>(m_otherBoundInputs, (InputAction action) => { Unbind(action); });
        }
    }
    #endregion

    // Dictionaries
    #region DictionaryOperations
    /// <summary>
    /// Helps to keep track of many timers, stored in a dictionary
    /// </summary>
    public class TimerTracker
    {
        private Dictionary<string, float> m_timerDict = new Dictionary<string, float>();

        /// <summary>
        /// Will add a timer (or overwrite) to the dictionary
        /// </summary>
        /// <param name="timerName">The key in the dictionary</param>
        /// <param name="time">The value to be held</param>
        /// <param name="overwrite">Whether or not to overwrite a timer that is held already</param>
        public void SetTimer(string timerName, float time, bool overwrite = false)
        {
            DictUtil.SetInDictionary<string, float>(ref m_timerDict, timerName, time, overwrite);
        }

        /// <summary>
        /// Get the value of a timer 
        /// </summary>
        /// <param name="timerName">The key of the timer in the dictionary</param>
        /// <returns>Returns the value of the timer, or -1 if there is no timer</returns>
        public float GetTimer(string timerName)
        {
            return DictUtil.GetFromDictionary<string, float>(m_timerDict, timerName, -1f);
        }
        /// <summary>
        /// Checks if the timer at timerName is <= to the timeCheck
        /// </summary>
        /// <param name="timerName">The name of the timer to get</param>
        /// <param name="timeCheck">The time to check against</param>
        /// <returns>Will return true if no timer was found at timerName</returns>
        public bool CheckTimer(string timerName, float timeCheck)
        {
            return GetTimer(timerName) <= timeCheck;
        }
    }

    public class Table<T1, T2, T3>
    {
        Dictionary<T1, Dictionary<T2, T3>> m_table = new Dictionary<T1, Dictionary<T2, T3>>();

        /// <summary>
        /// Retrieve data from the Table
        /// </summary>
        /// <param name="keyOne">The first key into the table</param>
        /// <param name="keyTwo">Second key, will check if first key is valid first</param>
        /// <param name="defaultReturn">Default value to return if nothing was found at the key index</param>
        /// <returns></returns>
        public T3 GetData(T1 keyOne, T2 keyTwo, T3 defaultReturn = default)
        {
            // Check that both keys are valid first
            if (!GeneralUtil.IsValid(keyOne) || !GeneralUtil.IsValid(keyTwo)) return defaultReturn;

            // If the table contains the first key, and the dictionary at the first key is not null then continue
            if (m_table.ContainsKey(keyOne))
            {
                if (m_table[keyOne] != null)
                {
                    // If the second key is in the table then return the value stored there
                    if (m_table[keyOne].ContainsKey(keyTwo))
                    {
                        return m_table[keyOne][keyTwo];
                    }
                }
            }

            return defaultReturn;
        }
        /// <summary>
        /// Add data to the Table given to index values 
        /// </summary>
        /// <param name="keyOne">First key to index at</param>
        /// <param name="keyTwo">Second key to index at</param>
        /// <param name="data">The data to store</param>
        /// <param name="overwrite">Whether to override a value stored at the index of the keys</param>
        /// <returns></returns>
        public bool AddData(T1 keyOne, T2 keyTwo, T3 data, bool overwrite = false)
        {
            // Check that both keys are valid first
            if (!GeneralUtil.IsValid(keyOne) || !GeneralUtil.IsValid(keyTwo)) return false;

            // If the Table contains the first key then we know there will be a dictionary there
            // Whcih means we can check if the second key is already set and then overwrite if needed
            if (m_table.ContainsKey(keyOne))
            {
                if (overwrite || !m_table[keyOne].ContainsKey(keyTwo))
                {
                    m_table[keyOne][keyTwo] = data;
                }
            }
            // If the first key is not detected then we first have to create a new dictionary entry at
            // The index of the first key and the set the value based on the second key
            else
            {
                m_table[keyOne] = new Dictionary<T2, T3>();
                m_table[keyOne][keyTwo] = data;
            }

            return false;
        }
    }

    public static class DictUtil
    {
        #region DictionaryFunctions
        /// <summary>
        /// Add an entry to a dictionary, if entry is already populated will overwrite it if told to
        /// </summary>
        /// <typeparam name="Key">The key used for accessing the dictionary</typeparam>
        /// <typeparam name="Value">The value being stored by the dictionary</typeparam>
        /// <param name="dict">The dictionary to set an entry in</param>
        /// <param name="key">The key of the dictionary to add an entry at</param>
        /// <param name="value">The value to set at the key</param>
        /// <param name="overwrite">If an entry is already populated will override it</param>
        public static bool SetInDictionary<Key, Value>(ref Dictionary<Key, Value> dict, Key key, Value value, bool overwrite = false)
        {
            if (GeneralUtil.IsValid<Key>(key) && GeneralUtil.IsValid<Value>(value))
            {
                if (overwrite)
                {
                    dict[key] = value;

                    return true;
                }
                else if (!dict.ContainsKey(key))
                {
                    dict[key] = value;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Add an entry to a dictionary, if entry is already populated will overwrite it if told to
        /// </summary>
        /// <typeparam name="Key">The key used for accessing the dictionary</typeparam>
        /// <typeparam name="Value">The value being stored by the dictionary</typeparam>
        /// <param name="dict">The dictionary to set an entry in</param>
        /// <param name="key">The key of the dictionary to add an entry at</param>
        /// <param name="value">The value to set at the key</param>
        /// <param name="logWarning">If an entry is already populated will print a warning message</param>
        /// <param name="overwrite">If an entry is already populated will override it</param>
        public static bool SetInDictionary<Key, Value>(ref Dictionary<Key, Value> dict, Key key, Value value, bool logWarning, bool overwrite = false)
        {
            if (!logWarning)
            {
                return SetInDictionary(ref dict, key, value, overwrite);
            }

            if (GeneralUtil.IsValid<Key>(key) && GeneralUtil.IsValid<Value>(value))
            {
                if (dict.ContainsKey(key))
                {
                    if (!GeneralUtil.IsValid(dict[key])) overwrite = true;

                    Debug.LogWarning($"Attempting to overwrite (flag: '{overwrite}') '{dict[key]}' with '{value}', using identifier '{key}'");

                    if (overwrite)
                    {
                        dict[key] = value;

                        return true;
                    }
                }
                else
                {
                    dict[key] = value;

                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get a value from a dictionary at the specified key, will return defaultReturn if there is no entry
        /// </summary>
        /// <typeparam name="Key">The key to access the dictionary</typeparam>
        /// <typeparam name="Value">The value stored in the dictionary</typeparam>
        /// <param name="dict">The dictionary to retrieve value from</param>
        /// <param name="key">The location of the value to retrieve</param>
        /// <returns></returns>
        public static Value GetFromDictionary<Key, Value>(Dictionary<Key, Value> dict, Key key, Value defaultReturn = default)
        {
            if (GeneralUtil.IsValid<Key>(key))
            {
                if (dict.ContainsKey(key))
                {
                    return dict[key];
                }
            }

            return defaultReturn;
        }
        #endregion
    }
    #endregion

    // Text Formatting + Strings
    #region StringFormatting
    [System.Serializable]
    public class FormattedParagraph
    {
        public FormattedText m_heading;
        public FormattedText m_body;

        public string GetFormatted()
        {
            return m_heading.GetFormatted() + m_body.GetFormatted();
        }
    }
    [System.Serializable]
    public class FormattedText
    {
        [TextArea] [Tooltip("Main body of text")] public string m_text;
        [Space]
        [Min(0)]
        [Tooltip("Number of new lines after the text")] public int m_newLinesAfterText = 1;

        [Header("Size Details")]
        [Tooltip("Size of the text")] public int m_textSize = 10;
        [Tooltip("Whether to use the default text size")] public bool m_useDefaultSize = false;

        [Header("Colour Details")]
        [Tooltip("Colour to set the text as")] public Color m_textColour = Color.red;

        [Header("Style Details")]
        [Tooltip("Whether to make the text bold")] public bool m_boldText = false;
        [Tooltip("Whether to make the text italic")] public bool m_italicText = false;
        [Tooltip("Whether to make the text strike-through")] public bool m_strikeThrough = false;

        /// <summary>
        /// Format a string of text based on the setting of this instance
        /// </summary>
        /// <param name="text">The text to format</param>
        /// <returns>The origanl text formatted correctly</returns>
        public string FormatText(string text)
        {
            string _formattedText = text;

            if (m_boldText)
            {
                _formattedText = StringUtil.MakeBold(_formattedText);
            }
            if (m_italicText)
            {
                _formattedText = StringUtil.MakeItalic(_formattedText);
            }
            if (m_strikeThrough)
            {
                _formattedText = StringUtil.MakeStrikethrough(_formattedText);
            }
            if (!m_useDefaultSize)
            {
                _formattedText = StringUtil.SetSize(_formattedText, m_textSize);
            }

            _formattedText = StringUtil.SetColour(_formattedText, m_textColour);
            _formattedText = StringUtil.AddNewLines(_formattedText, m_newLinesAfterText);

            return _formattedText;
        }

        /// <summary>
        /// Formats the text associated with this instance
        /// </summary>
        /// <returns></returns>
        public string GetFormatted()
        {
            return FormatText(m_text);
        }
    }

    public static class StringUtil
    {
        /// <summary>
        /// Remove a string from another string
        /// </summary>
        /// <param name="original">The string to remove from</param>
        /// <param name="toExclude">The string to remove</param>
        /// <returns>The original string after toExclude is removed</returns>
        public static string Exclude(string original, string toExclude)
        {
            return original.Replace(toExclude, "");
        }
        /// <summary>
        /// Remove a list of strings from a list
        /// </summary>
        /// <param name="original">The string to remove from</param>
        /// <param name="toExclude">The list of strings to remove</param>
        /// <returns>The original string after all exclusions are done</returns>
        public static string MultiExclude(string original, List<string> toExclude)
        {
            // Go through each element in toExclude and remove it from the original string
            ListUtil.LoopList<string>(toExclude, (string s) =>
            {
                original = Exclude(original, s);
            });

            return original;
        }
        /// <summary>
        /// Separate a string by upper cases (place a space before every upper case)
        /// </summary>
        /// <param name="text">The text to change</param>
        /// <returns>The text with spaces before every upper case</returns>
        public static string SeparateByUpperCase(string text)
        {
            string _newText = "";

            for (int i = 0; i < text.Length; i++)
            {
                // Check if the character is upper case
                // Add a space if it is
                if (char.IsUpper(text[i]) && i > 0)
                {
                    _newText += " ";
                }

                // Add the character to the new string
                _newText += text[i];
            }

            return _newText;
        }

        #region RichTextFormatting
        /// <summary>
        /// Prepend a string to a string, then append a string to the end of that result
        /// </summary>
        /// <param name="text">The string to prepend/append to</param>
        /// <param name="prependText">The string to add to the start of 'text'</param>
        /// <param name="appendText">The string to add to the end of 'text'</param>
        /// <returns>'text' with 'prependText' at the start and 'appendText' at the end</returns>
        public static string Prepend_Append(string text, string prependText, string appendText)
        {
            text = text.Insert(0, prependText);
            text = text.Insert(text.Length, appendText);

            return text;
        }
        /// <summary>
        /// Using mark-up make a string bold
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string MakeBold(string text)
        {
            return Prepend_Append(text, "<b>", "</b>");
        }
        /// <summary>
        /// Using mark-up make a string italic
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string MakeItalic(string text)
        {
            return Prepend_Append(text, "<i>", "</i>");
        }
        public static string MakeStrikethrough(string text)
        {
            return Prepend_Append(text, "<s>", "</s>");
        }
        /// <summary>
        /// Using mark-up make a string a certain colour
        /// </summary>
        /// <param name="text"></param>
        /// <param name="textColour"></param>
        /// <returns></returns>
        public static string SetColour(string text, Color textColour)
        {
            string hex = "#" + ColorUtility.ToHtmlStringRGBA(textColour);

            return Prepend_Append(text, $"<color={hex}>", "</color>");
        }
        /// <summary>
        /// Using mark-up make a string a certain font size
        /// </summary>
        /// <param name="text"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string SetSize(string text, int size)
        {
            return Prepend_Append(text, $"<size={size}>", "</size>");
        }
        /// <summary>
        /// Add a mewline character to the end of a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string AddNewLine(string text)
        {
            text = text.Insert(text.Length, "\n");

            return text;
        }
        /// <summary>
        /// Add a number of newline characters to the end of a string
        /// </summary>
        /// <param name="text"></param>
        /// <param name="numLines"></param>
        /// <returns></returns>
        public static string AddNewLines(string text, int numLines)
        {
            for (int i = 0; i < numLines; i++)
            {
                text = AddNewLine(text);
            }

            return text;
        }
        #endregion
    }
    #endregion

    // Layers
    #region LayerOperations
    [System.Serializable]
    public class LayerMasker
    {
        [TextArea] [Tooltip("Separate by ','")] public string m_layers = "";
        private List<string> m_layerNames = new List<string>();

        /// <summary>
        /// Get the integer value of the layer mask
        /// </summary>
        public int MaskValue { get { return GetMaskValue(); } }

        /// <summary>
        /// Get the integer value of the layer mask
        /// </summary>
        /// <returns></returns>
        public int GetMaskValue()
        {
            if (m_layerNames.Count > 0)
            {
                return LayerMask.GetMask(m_layerNames.ToArray());
            }
            else
            {
                m_layerNames = new List<string>(m_layers.Split(','));

                for (int i = 0; i < m_layerNames.Count; i++)
                {
                    m_layerNames[i] = m_layerNames[i].Trim();
                }

                return LayerMask.GetMask(m_layerNames.ToArray());
            }
        }

        /// <summary>
        /// Get whether a Layer is in a LayerMask
        /// </summary>
        /// <param name="mask">The mask to check against</param>
        /// <param name="layer">The layer to check</param>
        /// <returns>Whether the layer is in the mask</returns>
        public static bool IsInLayerMask(LayerMask mask, int layer)
        {
            return IsInLayerMask(mask.value, layer);
        }

        /// <summary>
        /// Get whether a Layer is in a LayerMask
        /// </summary>
        /// <param name="mask">The mask to check against</param>
        /// <param name="layer">The layer to check</param>
        /// <returns>Whether the layer is in the mask</returns>
        public static bool IsInLayerMask(int mask, int layer)
        {
            return mask == (mask | (1 << layer));
        }
    }
    #endregion

    // Lists
    #region ListOperations
    public static class ListUtil
    {
        /// <summary>
        /// Returns whether a list is "Valid" meaning it has elements and isn't null
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool IsValidList<T>(List<T> list)
        {
            return (list != null && list.Count != 0);
        }

        /// <summary>
        /// Will add a list to another list, but will check if each element of the additive list is already in the original
        /// </summary>
        /// <typeparam name="T">The type of data being worked with</typeparam>
        /// <param name="originalList">The list to add to</param>
        /// <param name="addList">The list to add</param>
        public static void ListAddRange<T>(ref List<T> originalList, List<T> addList)
        {
            if (!IsValidList(addList)) return;

            if (!IsValidList(originalList))
            {
                originalList = new List<T>(addList);
                return;
            }

            foreach (T elem in addList)
            {
                ListAdd<T>(ref originalList, elem);
            }
        }

        /// <summary>
        /// Will add a item to an list if the item isn't already in it
        /// </summary>
        /// <typeparam name="T">The type of data being worked with</typeparam>
        /// <param name="originalList">The list to add to"</param>
        /// <param name="addObj">The object to add</param>
        public static bool ListAdd<T>(ref List<T> originalList, T addObj)
        {
            if (!GeneralUtil.IsValid(addObj) || originalList == null)
            {
                return false;
            }

            // If there are no elements in the list, add
            if (originalList.Count == 0)
            {
                originalList.Add(addObj);

                return true;
            }
            // If the obj is not in the list, add
            else if (!originalList.Contains(addObj))
            {
                originalList.Add(addObj);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Remove an element from a list
        /// </summary>
        /// <typeparam name="T">The type of data in the lsit</typeparam>
        /// <param name="trimList">The list to be 'trimmed'</param>
        /// <param name="trimElem">The element to remove from the list</param>
        public static void TrimElement<T>(ref List<T> trimList, T trimElem)
        {
            if (IsValidList(trimList))
            {
                trimList.Remove(trimElem);
            }
        }
        /// <summary>
        /// Remove a list of objects from another list
        /// </summary>
        /// <typeparam name="T">The type of data in the list</typeparam>
        /// <param name="listToTrim">The list to have elements removed from</param>
        /// <param name="trimList">The list of elements to remove</param>
        public static void TrimElements<T>(ref List<T> listToTrim, List<T> trimList)
        {
            // If both lists are valid
            if (IsValidList(listToTrim) && IsValidList(trimList))
            {
                foreach (T elem in trimList)
                {
                    TrimElement<T>(ref listToTrim, elem);
                }
            }
        }

        /// <summary>
        /// Remove all of the null or destroyed elements from a list
        /// </summary>
        /// <typeparam name="T">The type of data in the list</typeparam>
        /// <param name="listToTrim">The list to trim</param>
        public static void TrimList<T>(ref List<T> listToTrim)
        {
            TrimElements<T>(ref listToTrim, GetNullOrDestroyed<T>(listToTrim));
        }

        /// <summary>
        /// Remove all objects from a list based on a predicate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="trimList"></param>
        /// <param name="trimPredicate"></param>
        public static void TrimList<T>(ref List<T> trimList, Func<T, bool> trimPredicate)
        {
            if (IsValidList(trimList) && trimPredicate != null)
            {
                List<T> _trimElements = new List<T>(trimList.Count);

                foreach (var elem in trimList)
                {
                    if (trimPredicate(elem))
                    {
                        _trimElements.Add(elem);
                    }
                }

                TrimElements(ref trimList, _trimElements);
            }
        }

        /// <summary>
        /// Loop through a list of type T (For Each Loop) and execute an action on each element
        /// </summary>
        /// <typeparam name="T">The type of data being worked with</typeparam>
        /// <param name="loopList">The list to affect</param>
        /// <param name="loopAction">The action to perform on each element of the list</param>
        /// <param name="breakOut">An optional Func<bool> that will determine any break conditions for the loop</bool></param>
        public static void LoopList<T>(List<T> loopList, Action<T> loopAction, Func<bool> breakOut = null)
        {
            // Check if the list and action are both valid
            if (IsValidList(loopList) && loopAction != null)
            {
                T _loopVal;
                bool _validBreakOut = breakOut != null;

                // Loop through the list and perform an action
                for (int _index = 0; _index < loopList.Count; _index++)
                {
                    _loopVal = loopList[_index];

                    if (!GeneralUtil.IsValid<T>(_loopVal))
                    {
                        continue;
                    }

                    loopAction?.Invoke(_loopVal);

                    if (_validBreakOut && breakOut())
                    {
                        break;
                    }
                }
            }
        }
        /// <summary>
        /// Get a list of all the null or destroyed elements in a list
        /// </summary>
        /// <typeparam name="T">The type of data in the list</typeparam>
        /// <param name="listToCheck">The list to get null or destroyed elements from</param>
        /// <returns>A list of all null or destroyed elements in the list</returns>
        public static List<T> GetNullOrDestroyed<T>(List<T> listToCheck)
        {
            if (IsValidList(listToCheck))
            {
                return new List<T>(0);
            }

            List<T> nullList = new List<T>(listToCheck.Count);

            foreach (T elem in listToCheck)
            {
                if (!GeneralUtil.IsValid<T>(elem))
                {
                    nullList.Add(elem);
                }
            }

            return nullList;
        }
        /// <summary>
        /// Will loop (For Each) through a list and see if the GameObject is in it. Ensure the list is a list of MonoBehavioours, checks are in place to reject anything else (Modifies 'CachedList')
        /// </summary>
        /// <typeparam name="T">The type of list, will need to be a MonoBehaviour or child of a MonoBehaviour</typeparam>
        /// <param name="monoList">TThe list to check in</param>
        /// <param name="obj">The GameObject to check for</param>
        /// <returns>Returns true if the GameObject is in the list</returns>
        public static bool ObjectInMonoList<T>(List<T> monoList, GameObject obj) where T : MonoBehaviour // Forces T to be MonoBehaviour 
        {
            if (obj == null || !IsValidList(monoList)) return false;

            bool _inList = false;

            LoopList<T>(monoList,
            (T mono) =>
            // LoopAction
            {
                if (GeneralUtil.IsValid(mono))
                {
                    if (mono.gameObject.Equals(obj))
                    {
                        _inList = true;
                    }
                }
            },
            // BreakOut action
            () => { return _inList; });

            return _inList;
        }

        /// <summary>
        /// Returns a list of (the first found) MonoBehaviours of type T on each GameObject
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objs"></param>
        /// <returns></returns>
        public static List<T> GetObjectsWithBehaviour<T>(List<GameObject> objs) where T : MonoBehaviour
        {
            if (!IsValidList(objs))
            {
                return null;
            }

            List<T> _returnList = new List<T>(objs.Count);
            T _tempMono = null;

            LoopList<GameObject>(objs,
                (GameObject go) =>
                {
                    _tempMono = go.GetComponent<T>();

                    if (GeneralUtil.IsValid(_tempMono))
                    {
                        _returnList.Add(_tempMono);
                    }
                });

            return _returnList;
        }
    }
    #endregion

    // Mathematics
    #region MathematicsOperations
    public static class MathUtil
    {
        /// <summary>
        /// Get the distance between two GameObjects
        /// </summary>
        /// <param name="objOne">First object</param>
        /// <param name="objTwo">Second object</param>
        /// <returns>Returns -1 if either object is null</returns>
        public static float Distance(GameObject objOne, GameObject objTwo, float defaultReturn = -1)
        {
            if (!GeneralUtil.IsValid(objOne) || !GeneralUtil.IsValid(objTwo))
            {
                return defaultReturn;
            }

            return Vector3.Distance(objOne.transform.position, objTwo.transform.position);
        }
        /// <summary>
        /// Get the distance between two Transforms
        /// </summary>
        /// <param name="transOne">First object</param>
        /// <param name="transTwo">Second object</param>
        /// <returns>Returns -1 if either object is null</returns>
        public static float Distance(Transform transOne, Transform transTwo, float defaultReturn = -1)
        {
            if (!GeneralUtil.IsValid(transOne) || !GeneralUtil.IsValid(transTwo))
            {
                return defaultReturn;
            }

            return Vector3.Distance(transOne.position, transTwo.position);
        }

        public static float DistanceSqrd(Vector3 vec1, Vector3 vec2)
        {
            return Vector3.SqrMagnitude(vec1 - vec2);
        }
        /// <summary>
        /// Get the squared distance between two GameObjects
        /// </summary>
        /// <param name="objOne">First object</param>
        /// <param name="objTwo">Second object</param>
        /// <returns>Returns -1 if either object is null</returns>
        public static float DistanceSqrd(GameObject objOne, GameObject objTwo, float defaultReturn = -1)
        {
            if (!GeneralUtil.IsValid(objOne) || !GeneralUtil.IsValid(objTwo))
            {
                return defaultReturn;
            }

            return DistanceSqrd(objOne.transform.position, objTwo.transform.position);
        }
        /// <summary>
        /// Get the squared distance between two Transforms
        /// </summary>
        /// <param name="transOne">First object</param>
        /// <param name="transTwo">Second object</param>
        /// <returns>Returns -1 if either object is null</returns>
        public static float DistanceSqrd(Transform transOne, Transform transTwo, float defaultReturn = -1)
        {
            if (!GeneralUtil.IsValid(transOne) || !GeneralUtil.IsValid(transTwo))
            {
                return defaultReturn;
            }

            return DistanceSqrd(transOne.position, transTwo.position);
        }

        /// <summary>
        /// Return whether two points are within 'dist' of each other 
        /// </summary>
        /// <param name="start">The start position</param>
        /// <param name="end">The end position</param>
        /// <param name="dist">The distance to check against</param>
        /// <returns>Whether the two points are within 'dist'</returns>
        public static bool InDistance(Vector3 start, Vector3 end, float dist)
        {
            return (DistanceSqrd(start, end) <= (dist * dist));
        }
        /// <summary>
        /// Returns whether two points are further away than 'dist'
        /// </summary>
        /// <param name="start">The start position</param>
        /// <param name="end">The end position</param>
        /// <param name="dist">The distance to check against</param>
        /// <returns>Whether the two points are further than 'dist' apart</returns>
        public static bool OutDistance(Vector3 start, Vector3 end, float dist)
        {
            return !InDistance(start, end, dist);
        }

        /// <summary>
        /// Multiplies x by x, y by y and z by z
        /// </summary>
        /// <param name="vec1"></param>
        /// <param name="vec2"></param>
        /// <returns></returns>
        public static Vector3 MultiplyVectors(Vector3 vec1, Vector3 vec2)
        {
            return new Vector3(vec1.x * vec2.x, vec1.y * vec2.y, vec1.z * vec2.z);
        }
    }
    #endregion

    // GameObjects
    #region GameObjectOperations
    public static class GOUtil
    {
        /// <summary>
        /// Calculate the closest GameObject between one object and a list of GameObjects
        /// </summary>
        /// <typeparam name="T">The type of list to iterate through</typeparam>
        /// <param name="baseObject">Base object to check distance with</param>
        /// <param name="testObjects">List of templated objects to use for distance checking</param>
        /// <param name="dist">The squared distance of the closest GameObject, returns -1 if there is no closest GameObject</param>
        /// <returns>The closest calculated GameObject</returns>
        public static GameObject GetClosestObject(GameObject baseObject, List<GameObject> testObjects, out float dist, float defaultDist = 1000)
        {
            if (!GeneralUtil.IsValid(baseObject) || !ListUtil.IsValidList(testObjects))
            {
                dist = -1;
                return null;
            }

            GameObject _closestObject = null;
            float _closestDist = float.MaxValue;
            float _tempDist = 0;

            // TO-DO: Test to see if List.Sort(predicate) is faster

            // Loop through the list and check if the current object is closer than the previous closest
            ListUtil.LoopList<GameObject>(testObjects, (GameObject go) =>
            {
                // If these two aren't the same object, then continue calculating
                // Also continue if _tempObject is null
                if (!GeneralUtil.AreEqual(go, baseObject))
                {
                    _tempDist = MathUtil.DistanceSqrd(go, baseObject, float.MaxValue);

                    if (_tempDist < _closestDist)
                    {
                        _closestDist = _tempDist;
                        _closestObject = go;
                    }
                }
            });

            if (!_closestObject) dist = defaultDist;
            else dist = _closestDist;

            return _closestObject;
        }

        /// <summary>
        /// Only pass in MonoBehaviour as type parameter otherwise there will be errors!
        /// </summary>
        /// <typeparam name="T">Make sure this is a MonoBehaviour type</typeparam>
        /// <param name="obj">The object to check for the MonoBehaviour</param>
        /// <returns>The first found MonoBehaviour (Checks GetComponent, GetComponentInChildren then GetComponentInParent)</returns>
        public static T GetComponent<T>(GameObject obj)
        {
            if (!GeneralUtil.IsValid(obj)) return default;

            T _type = obj.GetComponent<T>();

            if (_type == null)
            {
                _type = obj.GetComponentInChildren<T>();
            }
            if (_type == null)
            {
                _type = obj.GetComponentInParent<T>();
            }

            return _type;
        }
        /// <summary>
        /// Only pass in MonoBehaviour as type parameter otherwise there will be errors! Gets all of the components in a GameObject hierarchy 
        /// </summary>
        /// <typeparam name="T">The type of MonoBehaviour to get</typeparam>
        /// <param name="obj">The GameObject to get the MonoBehaviours from</param>
        /// <returns>A list of found MonoBehaviours</returns>
        public static List<T> GetComponents<T>(GameObject obj)
        {
            if (!GeneralUtil.IsValid(obj)) return new List<T>(0);

            List<T> _componentList = new List<T>();

            _componentList.AddRange(obj.GetComponents<T>());
            _componentList.AddRange(obj.GetComponentsInChildren<T>());
            _componentList.AddRange(obj.GetComponentsInParent<T>());

            return _componentList;
        }
        /// <summary>
        /// Will send a message up and down a GameObject hierarchy 
        /// </summary>
        /// <param name="go">GameObject to send message to</param>
        /// <param name="message">Message to be sent</param>
        /// <param name="options">Whether there will be a warning message if there is no receiver</param>
        public static void SendMessageToChain(GameObject go, string message, SendMessageOptions options = SendMessageOptions.DontRequireReceiver)
        {
            // Check if the message and GameObject are both valid
            if (GeneralUtil.IsValid(go) && message.Length > 0)
            {
                // Send the message to all children
                go.BroadcastMessage(message, options);

                // Send the message to parent if it has one
                if (go.transform.parent)
                {
                    go.transform.parent.SendMessageUpwards(message, options);
                }
            }
        }
    }
    #endregion

    // General
    #region GeneralPurposeOperations
    public static class GeneralUtil
    {
        #region IEnumerators
        /// <summary>
        /// A coroutine to delay an action some amount of time, takes into account timeScale
        /// </summary>
        /// <param name="action">Action to perform after delay</param>
        /// <param name="delay">Delay before performing action</param>
        /// <returns></returns>
        public static IEnumerator DelayAction(System.Action action, float delay)
        {
            if (action == null) yield return null;

            yield return new WaitForSeconds(delay);

            action?.Invoke();
        }
        /// <summary>
        /// A coroutine to delay an action some amount of time, does not take into account timeScale
        /// </summary>
        /// <param name="action">Action to perform after delay</param>
        /// <param name="delay">Delay before performing action</param>
        /// <returns></returns>
        public static IEnumerator DelayActionRealtime(System.Action action, float delay)
        {
            if (action == null) yield return null;

            yield return new WaitForSecondsRealtime(delay);

            action?.Invoke();
        }
        /// <summary>
        /// Waits till the end of the frame before performing an action
        /// </summary>
        /// <param name="action">Action to perform at end of the frame</param>
        /// <returns></returns>
        public static IEnumerator DelayActionEOF(System.Action action)
        {
            if (action == null) yield return null;

            yield return new WaitForEndOfFrame();

            action?.Invoke();
        }
        #endregion

        /// <summary>
        /// Check if two objects are equal using the .Equals function
        /// </summary>
        /// <typeparam name="T">The type of objects to check</typeparam>
        /// <param name="checkOne">First object to check</param>
        /// <param name="checkTwo">Object to check against</param>
        /// <returns>Returns false if either objects are null or aren't equal</returns>
        public static bool AreEqual<T>(T checkOne, T checkTwo)
        {
            if (IsValid(checkOne) && IsValid(checkTwo))
            {
                if (checkOne.Equals(checkTwo))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// A 'safe' while loop, will loop until it hits the "maxIterations" where it will 
        /// Decide that it has looped too many times and break out
        /// </summary>
        /// <param name="action">Action to perform in the loop</param>
        /// <param name="predicate">Predicate to break out of the while loop</param>
        /// <param name="maxIterations">The max number of iterations before breaking out</param>
        public static void SafeWhile(Action action, Func<bool> predicate, uint maxIterations = 1000, bool logSafetyBreak = true)
        {
            if (action == null || predicate == null) return;

            uint _iterationCount = 0;

            while (predicate.Invoke())
            {
                action.Invoke();

                if (_iterationCount++ >= maxIterations)
                {
                    if (logSafetyBreak)
                    {
                        Debug.LogWarning("Broke out of a safe while loop!");
                    }

                    return;
                }
            }
        }
        /// <summary>
        /// A 'safe' do while loop, will loop until it hits the "maxIterations" where it will 
        /// Decide that it has looped too many times and break out
        /// </summary>
        /// <param name="action">Action to perform in the loop</param>
        /// <param name="predicate">Predicate to break out of the while loop</param>
        /// <param name="maxIterations">The max number of iterations before breaking out</param>
        public static void SafeDoWhile(Action action, Func<bool> predicate, uint maxIterations = 1000, bool logSafetyBreaks = true)
        {
            action?.Invoke();

            SafeWhile(action, predicate, maxIterations, logSafetyBreaks);
        }

        /// <summary>
        /// Determine whether an object is null or has been destroyed 
        /// </summary>
        /// <typeparam name="T">The type of data being worked with</typeparam>
        /// <param name="obj">The object to test</param>
        /// <returns>Returns true if it was null or destroyed</returns>
        public static bool IsValid<T>(T obj)
        {
            return (obj != null) && (!obj.Equals(null));
        }

        public static int BoolToInt(bool val)
        {
            return Convert.ToInt32(val);
        }

        /// <summary>
        /// Execute System.Action safely
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data">Data to pass to the action</param>
        /// <param name="action">Action to execute</param>
        /// <returns></returns>
        public static bool ActionUpdate<T>(T data, Action<T> action)
        {
            if (IsValid(data))
            {
                if (action != null)
                {
                    action.Invoke(data);

                    return true;
                }
            }
            return false;
        }
    }
    #endregion

    // Bit
    #region BitOperations
    public class BitLock
    {
        public bool IsLocked { get { return m_lockVal != 0; } }

        private int m_lockVal = 0;

        /// <summary>
        /// Set m_lockVal back to 0
        /// </summary>
        public void Reset()
        {
            m_lockVal = 0;
        }

        /// <summary>
        /// Set the 'nth' bit
        /// </summary>
        /// <param name="nth"></param>
        public void LockBit(int nth)
        {
            m_lockVal = m_lockVal | (1 << nth);
        }

        /// <summary>
        /// Unlock the 'nth' bit
        /// </summary>
        /// <param name="nth"></param>
        public void UnlockBit(int nth)
        {
            m_lockVal = m_lockVal & ~(1 << nth);
        }

        /// <summary>
        /// Check whether the 'nth' bit is set
        /// </summary>
        /// <param name="bit"></param>
        /// <returns></returns>
        public bool IsBitSet(int bit)
        {
            return (m_lockVal & (1 << bit)) != 0;
        }
    }
    #endregion
}
