using CubeShift.Player;
using UnityEngine;

namespace CubeShift.Cameras
{
    /// <summary>
    /// Smoothly follows the cube from an isometric-friendly offset.
    /// </summary>
    public sealed class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(6f, 7f, -6f);
        [SerializeField] private Vector3 lookOffset = Vector3.zero;
        [SerializeField, Min(0.01f)] private float smoothTime = 0.12f;
        [SerializeField, Min(0.01f)] private float rotationSmooth = 12f;
        [SerializeField] private bool rotateTowardTarget = true;

        private Vector3 velocity;

        private void Awake()
        {
            if (target == null)
            {
                PlayerCubeController player = FindAnyObjectByType<PlayerCubeController>();
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

            if (!rotateTowardTarget)
            {
                return;
            }

            Vector3 lookPoint = target.position + lookOffset;
            Vector3 lookDirection = lookPoint - transform.position;
            if (lookDirection.sqrMagnitude <= 0.001f)
            {
                return;
            }

            Quaternion desiredRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                1f - Mathf.Exp(-rotationSmooth * Time.deltaTime));
        }
    }
}
