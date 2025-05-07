using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using KinematicCharacterController;

public class Player : MonoBehaviour
{
    public CustomCharacterCamera OrbitCamera;
    public Transform CameraFollowPoint;
    public CharacterController Character;
    
    private const string HorizontalInput = "Horizontal";
    private const string VerticalInput = "Vertical";

    private void Start()
    {
        // Lock the cursor
        Cursor.lockState = CursorLockMode.Locked;

        // Tell camera to follow transform
        OrbitCamera.SetFollowTransform(CameraFollowPoint);
    }

    private void Update()
    {
        HandleCharacterInput();
    }
    
    private void LateUpdate()
    {
        HandleCameraInput();
    }

    private void HandleCameraInput()
    {
        // Apply inputs to the camera
        OrbitCamera.UpdateWithInput(Time.deltaTime, Vector3.zero);
    }
    
    private void HandleCharacterInput()
    {
        PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

        // Build the CharacterInputs struct
        characterInputs.MoveAxisForward = Input.GetAxisRaw(VerticalInput);
        characterInputs.MoveAxisRight = Input.GetAxisRaw(HorizontalInput);

        // Apply inputs to character
        Character.SetInputs(ref characterInputs);
    }
}
