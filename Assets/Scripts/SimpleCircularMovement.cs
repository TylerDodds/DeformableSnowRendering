#if ENABLE_INPUT_SYSTEM && ENABLE_INPUT_SYSTEM_PACKAGE
#define USE_INPUT_SYSTEM
    using UnityEngine.InputSystem;
    using UnityEngine.InputSystem.Controls;
#endif

using UnityEngine;

namespace UnityTemplateProjects
{
    public class SimpleCircularMovement : MonoBehaviour
    {
        public Vector2 Radii = Vector2.one;
        public float Angle = 0f;
        public float Frequency = 1f;

        [SerializeField]
        private Vector3 _initialPosition;

        private void Awake()
        {
            _initialPosition = transform.position;
        }

        private void Update()
        {
            float timeAngle = Time.time * Mathf.PI * 2 * Frequency;
            Vector2 alignedDelta = new Vector2(Radii.x * Mathf.Cos(timeAngle), Radii.y * Mathf.Sin(timeAngle));
            Vector2 delta = new Vector2(Mathf.Cos(Angle) * alignedDelta.x - Mathf.Sin(Angle) * alignedDelta.y, Mathf.Sin(Angle) * alignedDelta.x + Mathf.Cos(Angle) * alignedDelta.y);
            Vector3 finalPosition = _initialPosition + (Vector3.right * delta.x + Vector3.forward * delta.y);
            transform.position = finalPosition;
        }
    }

}