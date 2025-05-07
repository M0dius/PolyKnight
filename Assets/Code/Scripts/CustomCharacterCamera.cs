using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCharacterCamera : MonoBehaviour
{
    [Header("Framing")] public Camera Camera;
    public Vector2 FollowPointFraming = new Vector2(0f, 0f);
    public float FollowingSharpness = 10000f;
    
    public Transform Transform { get; private set; }
    public Transform FollowTransform { get; private set; }

    public Vector3 PlanarDirection { get; set; }
    
    private Vector3 _currentFollowPosition;

    void Awake()
    {
        Transform = this.transform;

        PlanarDirection = Vector3.forward;
    }
    
    // Set the transform that the camera will orbit around
    public void SetFollowTransform(Transform t)
    {
        FollowTransform = t;
        PlanarDirection = FollowTransform.forward;
        _currentFollowPosition = FollowTransform.position;
    }

    public void UpdateWithInput(float deltaTime, Vector3 rotationInput)
    {
        if (FollowTransform)
        {
            // Find the smoothed follow position
            _currentFollowPosition = Vector3.Lerp(_currentFollowPosition, FollowTransform.position,
                1f - Mathf.Exp(-FollowingSharpness * deltaTime));

            // Find the smoothed camera orbit position
            Vector3 targetPosition = _currentFollowPosition - Vector3.forward;

            // Handle framing
            targetPosition += Transform.right * FollowPointFraming.x;
            //targetPosition += Transform.up * FollowPointFraming.y;

            // Apply position
            Transform.position = targetPosition;
        }
    }
}
