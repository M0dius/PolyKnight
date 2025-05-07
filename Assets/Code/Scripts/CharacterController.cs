using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController;
using UnityEngine;

public struct PlayerCharacterInputs
{
    public float MoveAxisForward;
    public float MoveAxisRight;
}

public class CharacterController : MonoBehaviour, ICharacterController
{
   public KinematicCharacterMotor Motor;
   
   [Header("Stable Movement")]
   public float MaxStableMoveSpeed = 10f;
   public float StableMovementSharpness = 15;
   public float OrientationSharpness = 10;

   [Header("Air Movement")]
   public float MaxAirMoveSpeed = 10f;
   public float AirAccelerationSpeed = 5f;
   public float Drag = 0.1f;

   [Header("Misc")]
   public Vector3 Gravity = new Vector3(0, -30f, 0);
   public Transform MeshRoot;

   private Vector3 _moveInputVector;
   private Vector3 _lookInputVector;
   
        private void Start()
        {
            // Assign to motor
            Motor.CharacterController = this;
        }
        
        /// <summary>
        /// This is called every frame by MyPlayer in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref PlayerCharacterInputs inputs)
        {
            // Clamp input
            Vector3 moveInputVector = Vector3.ClampMagnitude(new Vector3(inputs.MoveAxisRight, 0f, inputs.MoveAxisForward), 1f);
            
            // Calculate camera direction on the character plane
            Vector3 cameraPlanarDirection = Vector3.ProjectOnPlane(Vector3.forward, Motor.CharacterUp).normalized;
            /*if (cameraPlanarDirection.sqrMagnitude == 0f)
            {
                cameraPlanarDirection = Vector3.ProjectOnPlane(Vector3.up, Motor.CharacterUp).normalized;
            }*/
            
            // Move input
            _moveInputVector = moveInputVector;
            _lookInputVector = cameraPlanarDirection;
        }

        public void BeforeCharacterUpdate(float deltaTime)
        {
            // This is called before the motor does anything
        }
        
        // Only use this function specifically to update character rotation!
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            // This is called when the motor wants to know what its rotation should be right now
            if (_moveInputVector != Vector3.zero && OrientationSharpness > 0f) 
            {
                // Smoothly interpolate from current to target direction
                Vector3 smoothedInputDirection = Vector3.Slerp(Motor.CharacterForward, _moveInputVector, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;
               

                // Set the current rotation (which will be used by the KinematicCharacterMotor)
                currentRotation = Quaternion.LookRotation(smoothedInputDirection, Motor.CharacterUp);
            }
        }
        
        // Only use this function specifically to update character velocity!
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            // This is called when the motor wants to know what its velocity should be right now
            Vector3 targetMovementVelocity = Vector3.zero;
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                // Reorient source velocity on current ground slope (this is because we don't want our smoothing to cause any velocity losses in slope changes)
                currentVelocity =
                    Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) *
                    currentVelocity.magnitude;

                // Calculate target velocity
                Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
                Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized *
                                          _moveInputVector.magnitude;
                targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;

                // Smooth movement Velocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity,
                    1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
            }else
            {
                // Add move input
                if (_moveInputVector.sqrMagnitude > 0f)
                {
                    targetMovementVelocity = _moveInputVector * MaxAirMoveSpeed;

                    // Prevent climbing on unstable slopes with air movement
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        Vector3 perpendicularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                        targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpendicularObstructionNormal);
                    }

                    Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, Gravity);
                    currentVelocity += velocityDiff * (AirAccelerationSpeed * deltaTime);
                }

                // Gravity
                currentVelocity += Gravity * deltaTime;

                // Drag
                currentVelocity *= (1f / (1f + (Drag * deltaTime)));
            }
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            // This is called after the motor has finished everything in its update
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {
            // This is called after when the motor wants to know if the collider can be collided with (or if we just go through it)
            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            // This is called when the motor's ground probing detects a ground hit
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            // This is called when the motor's movement logic detects a hit
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            // This is called after every hit detected in the motor, to give you a chance to modify the HitStabilityReport any way you want
        }

        public void PostGroundingUpdate(float deltaTime)
        {
            // This is called after the motor has finished its ground probing, but before PhysicsMover/Velocity/etc.... handling
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
            // This is called by the motor when it is detecting a collision that did not result from a "movement hit".
        }
}
