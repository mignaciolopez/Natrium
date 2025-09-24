using UnityEngine;
using UnityEngine.Events;

namespace MyGizmos
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