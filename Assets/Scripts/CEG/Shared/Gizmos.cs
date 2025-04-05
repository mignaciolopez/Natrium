using UnityEngine;
using UnityEngine.Events;

namespace CEG.Shared
{
    public class Gizmos : MonoBehaviour
    {
        public static UnityEvent OnEcsGizmos;

        private void Awake()
        {
            OnEcsGizmos = new UnityEvent();
        }

        private void OnDrawGizmos()
        {
            OnEcsGizmos?.Invoke();
        }
    }
}
