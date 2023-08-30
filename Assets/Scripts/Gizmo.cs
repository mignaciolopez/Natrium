using UnityEngine;
using UnityEngine.Events;

namespace Natrium
{
    public class Gizmo : MonoBehaviour
    {
        public static UnityEvent SOnDrawGizmos;

        private void Awake()
        {
            SOnDrawGizmos = new UnityEvent();
        }

        private void OnDrawGizmos()
        {
            SOnDrawGizmos?.Invoke();
        }
    }
}
