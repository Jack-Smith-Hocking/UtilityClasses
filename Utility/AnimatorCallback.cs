﻿using System;
using UnityEngine;

namespace Custom.Utility
{
    /// <summary>
    /// Similar scenario to the CollisionsCallback class but for the animator, will pick up animation events that invoke the "PerformAction" function (ideally the standard)
    /// </summary>
    public class AnimatorCallback : MonoBehaviour
    {
        public Action<string> OnAnimationEvents = null;

        public void PerformEvent(string eventName)
        {
            if (OnAnimationEvents != null)
            {
                OnAnimationEvents.Invoke(eventName);
            }
        }
    }
}