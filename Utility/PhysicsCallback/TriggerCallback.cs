﻿using UnityEngine;

namespace Custom.Utility
{
    public class TriggerCallback : Callback<Collider>
    {
        public void OnTriggerEnter(Collider other)
        {
            OnEnter(other);
        }
        public void OnTriggerStay(Collider other)
        {
            OnStay(other);
        }
        public void OnTriggerExit(Collider other)
        {
            OnExit(other);
        }
    }
}