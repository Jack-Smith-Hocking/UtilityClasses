using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Custom.Utility
{
    public class Collision2DCallback : Callback<Collision2D>
    {
        public void OnCollisionEnter2D(Collision2D collision)
        {
            OnEnter(collision);
        }
        public void OnCollisionStay2D(Collision2D collision)
        {
            OnStay(collision);
        }
        public void OnCollisionExit2D(Collision2D collision)
        {
            OnExit(collision);
        }
    }
}