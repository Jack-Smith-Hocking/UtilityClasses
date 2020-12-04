using UnityEngine;

namespace Custom.Utility
{
    /// <summary>
    /// Since Triggers and Collisions have no callback functionality (to my knowledge) i made a simple class to do just that
    /// Has a set of UnityEvents with no arguments and Actions that take the appropriate arguments 
    /// </summary>
    public class Collision3DCallback : Callback<Collision>
    {
        public void OnCollisionEnter(Collision collision)
        {
            OnEnter(collision);
        }
        public void OnCollisionStay(Collision collision)
        {
            OnStay(collision);
        }
        public void OnCollisionExit(Collision collision)
        {
            OnExit(collision);
        }
    }
}