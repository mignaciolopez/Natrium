using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Natrium
{
    public class Gizmo : MonoBehaviour
    {
        public static UnityEvent s_OnDrawGizmos;

        private void Awake()
        {
            s_OnDrawGizmos = new UnityEvent();
        }

        private void OnDrawGizmos()
        {
            s_OnDrawGizmos?.Invoke();
        }
    }
}
