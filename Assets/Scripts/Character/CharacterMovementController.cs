using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class CharacterMovementController : MonoBehaviour
{
    public float acceleration = 2f;
    public float deceleration = 2f;
    public float turnSpeed = 3f;

    [SerializeField] private bool m_IsWalking;
    [SerializeField] private float m_WalkSpeed;
    [SerializeField] private float m_RunSpeed;
    [SerializeField] private float m_DodgeSpeed;
    public float dodgeDuration = 1;
    public float dodgeYOffset = -1;
    public float dodgeYLerpFactor = 5;

    [SerializeField] private float m_HitSpeedFactor;
    [SerializeField] private float m_HitSpeedDuration;
    [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier;
    [SerializeField] private float m_StepInterval;

    [SerializeField]
    private AudioClip[] m_FootstepSounds; // an array of footstep sounds that will be randomly selected from.

    [SerializeField] private AudioClip m_JumpSound; // the sound played when character leaves the ground.
    [SerializeField] private AudioClip m_LandSound; // the sound played when character touches back on ground.

    private CharacterModel characterModel;
    private bool m_Jump;
    private float m_YRotation;
    private float curSpeed;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    public CharacterController m_CharacterController { get; private set; }
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private AudioSource m_AudioSource;
    private float lastJumpTime = float.MinValue;
    [SerializeField] [ReadOnly] private float chargedJumpCharge = 0;
    private float airTime = 0;

    [ReadOnly] public bool isDodging = false;
    private float timeSinceDodgeStart = 0;
    private Vector3 dodgeDir;

    bool isDashing = false;
    private float timeSinceDashStart = 0;
    private Vector3 dashDir;
    private float dashSpeed;
    private float dashDuration = 1;

    public float shieldTurnWindow = 1f;

    private Vector3 originalAvatarPosition;

    private Transform frameOfReference => ThirdPersonCamera.Instance.virtualCamera.transform;

    public bool canMove => characterModel.isAlive && !characterModel.characterAnimEventHandler.isInDivingState &&
                           !characterModel.characterAnimEventHandler.isInFallingState &&
                           !characterModel.characterAnimEventHandler.isInGettingUpState && Time.timeScale > 0.1f;

    private class GizmosData
    {
        public Vector3 raycastDir;
    }

    private GizmosData _gizmosData = new GizmosData();

    private void Awake()
    {
        characterModel = GetComponent<CharacterModel>();
    }

    // Use this for initialization
    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_AudioSource = GetComponent<AudioSource>();

        originalAvatarPosition = characterModel.avatar.localPosition;
        timeSinceDodgeStart = dodgeDuration * 2;
        timeSinceDashStart = 10;

        characterModel.health.OnDamageTaken.AddListener(() =>
        {
            // isDashing = false;
            isDodging = false;
        });

        characterModel.characterAnimEventHandler.AddStateExitListener("dive",
            (key, animator, stateInfo, layer) => { StopDashing(); });
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, transform.forward * 10);

        Gizmos.color = Color.red;
        if (characterModel)
        {
            Gizmos.DrawRay(transform.position, frameOfReference.TransformDirection(
                                                   characterModel.characterInput.Move
                                                       .normalized).normalized * 10);
        }

        // Gizmos.DrawRay(transform.position, _gizmosData.raycastDir * 10);
        // Gizmos.DrawWireSphere(transform.position, 10f);
    }

    // Update is called once per frame
    private void Update()
    {
        if (characterModel.isDead)
        {
            return;
        }

        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            PlayLandingSound();
            m_MoveDir.y = 0f;
            m_Jumping = false;
        }

        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;

        if (!isDodging && characterModel.characterInput.Dodge && m_CharacterController.isGrounded &&
            timeSinceDodgeStart > dodgeDuration * 2)
        {
            isDodging = true;
            var moveDir = characterModel.characterInput.Move;
            dodgeDir = moveDir.magnitude > 0.1
                ? frameOfReference.TransformDirection(moveDir)
                : transform.forward;

            dodgeDir.y = 0;
            dodgeDir.Normalize();

            timeSinceDodgeStart = 0;

            var localDodgeDir = transform.InverseTransformDirection(dodgeDir);
            characterModel.animator.SetFloat("DodgeX", localDodgeDir.x);
            characterModel.animator.SetFloat("DodgeY", localDodgeDir.z);
            characterModel.animator.SetTrigger("Dodge");

            SoundEffectsManager.Instance.Play("DodgeRoll");
        }
    }

    private void FixedUpdate()
    {
        ApplyGravity();

        if (LevelManager.Instance.isGameOver || characterModel.isDead)
        {
            Move(0);
            UpdateFootstepVolume(0);
            return;
        }

        timeSinceDodgeStart += Time.fixedDeltaTime;
        timeSinceDashStart += Time.fixedDeltaTime;

        var dodgeAvatarPosition = originalAvatarPosition;
        dodgeAvatarPosition.y += dodgeYOffset;

        float avatarPositionLerpValue = 0;
        if (isDodging)
        {
            PerformDodge();
            avatarPositionLerpValue = HelperUtilities.Remap(timeSinceDodgeStart, 0, dodgeDuration / 2, 0, 1);

            if (timeSinceDodgeStart >= dodgeDuration)
            {
                isDodging = false;
            }

            UpdateFootstepVolume(0);
        }
        else if (isDashing)
        {
            PerformDash();

            if (dashDuration > 0 && timeSinceDashStart >= dashDuration)
            {
                isDashing = false;
            }

            UpdateMoveAnimation(0, true, false);
            UpdateFootstepVolume(0);
        }
        else
        {
            GetInput(out var speed);

            Move(speed);

            if (m_CharacterController.isGrounded)
            {
                airTime = 0;
            }
            else
            {
                airTime += Time.fixedDeltaTime;
            }

            ProgressStepCycle(speed);

            avatarPositionLerpValue = HelperUtilities.Remap(timeSinceDodgeStart, dodgeDuration,
                dodgeDuration + (dodgeDuration / 2), 1, 0);
        }

        characterModel.avatar.localPosition = Vector3.Lerp(originalAvatarPosition, dodgeAvatarPosition,
            avatarPositionLerpValue);
    }

    public void DashTowards(Vector3 worldPos, float speed, float duration)
    {
        dashDir = worldPos - transform.position;
        dashDir.y = 0;
        dashDir.Normalize();

        timeSinceDashStart = 0;

        dashSpeed = speed;
        dashDuration = duration;

        isDashing = true;
    }

    private void adjustCurSpeed(float speed)
    {
        if (speed > curSpeed)
        {
            curSpeed += acceleration;
            curSpeed = Mathf.Min(curSpeed, speed);
        }
        else if (speed < curSpeed)
        {
            curSpeed -= deceleration;
            curSpeed = Mathf.Max(curSpeed, speed);
        }
    }

    void ApplyGravity()
    {
        if (!m_Jumping && m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;
        }
        else
        {
            //            Debug.Log("Applying Gravity");
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }

        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);
    }

    private void PerformDodge()
    {
        adjustCurSpeed(m_DodgeSpeed);
        m_CollisionFlags = m_CharacterController.Move(dodgeDir * curSpeed * Time.fixedDeltaTime);

        // ApplyGravity();

        var targetRot = Quaternion.LookRotation(dodgeDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * 2 * Time.fixedDeltaTime);
    }

    private void PerformDash()
    {
        adjustCurSpeed(dashSpeed);
        m_CollisionFlags = m_CharacterController.Move(dashDir * curSpeed * Time.fixedDeltaTime);

        // ApplyGravity();

        var targetRot = Quaternion.LookRotation(dashDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * 2 * Time.fixedDeltaTime);
    }

    public void StopDashing()
    {
        isDashing = false;
    }

    private void Move(float speed)
    {
        adjustCurSpeed(speed);
        switch (CinemachineCameraManager.Instance.CurrentState)
        {
            case CinemachineCameraManager.CinemachineCameraState.ThirdPerson:
                TPMove(curSpeed);
                break;
            default:
                return;
        }

        // ApplyGravity();

        // Move Animation
        Vector3 animVector = characterModel.characterInput.Move;
        if (animVector.magnitude > 1)
        {
            animVector.Normalize();
        }

        if (m_IsWalking)
        {
            animVector /= 2;
        }

        float curSpeedFactor = HelperUtilities.Remap(curSpeed, 0, m_IsWalking ? m_WalkSpeed : m_RunSpeed, 0, 1) *
                               animVector.magnitude;
        UpdateMoveAnimation(curSpeedFactor, m_CharacterController.isGrounded, !m_IsWalking);

        UpdateFootstepVolume(HelperUtilities.Remap(curSpeed, 0, m_IsWalking ? m_WalkSpeed : m_RunSpeed, 0, 1) *
                             characterModel.characterInput.Move.magnitude * characterModel.footstepMaxVolume);
    }

    void UpdateMoveAnimation(float forward, bool isGrounded, bool isSprinting)
    {
        characterModel.animator.SetFloat("Forward", forward);

        characterModel.animator.SetBool("IsGrounded", isGrounded);
        characterModel.animator.SetBool("IsSprinting", isSprinting);
    }

    void UpdateFootstepVolume(float vol)
    {
        characterModel.footstepAudio.volume = vol;
    }

    private void TPMove(float speed)
    {
        Vector3 desiredMove = Vector3.zero;
        if (frameOfReference)
        {
            Vector3 forwardDir = frameOfReference.forward;
            Vector3 rightDir = frameOfReference.right;

            forwardDir.y = 0;
            rightDir.y = 0;

            forwardDir.Normalize();
            rightDir.Normalize();

            desiredMove = forwardDir * m_Input.y;
            desiredMove += rightDir * m_Input.x;
        }

        if (desiredMove.magnitude > 1)
        {
            desiredMove.Normalize();
        }

        if (desiredMove.magnitude > 0 && speed > 0 && canMove)
        {
            //            Vector3 lookAtTarget = transform.position +
            //                                   (thirdPersonCamera.virtualCamera.transform.forward * 5);
            Vector3 lookAtTarget = transform.position + (desiredMove * 5);
            lookAtTarget.y = transform.position.y;

            Vector3 targetForward = lookAtTarget - transform.position;
            targetForward.Normalize();

            //            transform.forward = Vector3.Lerp(transform.forward, targetForward, turnSpeed * Time.fixedDeltaTime);

            var targetRot = Quaternion.LookRotation(targetForward);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
        }

        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;
    }

    private void TurnTowardsLockedTarget()
    {
        //            Vector3 lookAtTarget = transform.position +
        //                                   (thirdPersonCamera.virtualCamera.transform.forward * 5);
        // Vector3 lookAtTarget = transform.position + (desiredMove * 5);
        Vector3 lookAtTarget = characterModel.lockedOnTargetPos;
        lookAtTarget.y = transform.position.y;

        Vector3 targetForward = lookAtTarget - transform.position;
        targetForward.Normalize();

        //            transform.forward = Vector3.Lerp(transform.forward, targetForward, turnSpeed * Time.fixedDeltaTime);

        var targetRot = Quaternion.LookRotation(targetForward);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, turnSpeed * Time.fixedDeltaTime);
    }

    private void PlayLandingSound()
    {
        m_AudioSource.clip = m_LandSound;
        m_AudioSource.Play();
        m_NextStep = m_StepCycle + .5f;
    }

    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle +=
                (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

        PlayFootStepAudio();
    }


    private void PlayFootStepAudio()
    {
        if (!m_CharacterController.isGrounded || m_FootstepSounds.Length == 0)
        {
            return;
        }

        // pick & play a random footstep sound from the array,
        // excluding sound at index 0
        int n = Random.Range(1, m_FootstepSounds.Length);
        m_AudioSource.clip = m_FootstepSounds[n];
        m_AudioSource.PlayOneShot(m_AudioSource.clip);
        // move picked sound to index 0 so it's not picked next time
        m_FootstepSounds[n] = m_FootstepSounds[0];
        m_FootstepSounds[0] = m_AudioSource.clip;
    }

    private void GetInput(out float speed)
    {
        CharacterModel.CharacterInput playerInput = characterModel.characterInput;

        // Read input

        bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
        // On standalone builds, walk/run chaseSpeed is modified by a key press.
        // keep track of whether or not the character is walking or running
        // m_IsWalking = characterModel.characterInput.Sprint;
        m_IsWalking = true;
#endif
        // set the desired chaseSpeed to be walking or running
        speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;

        if (characterModel.health.timeSinceLastDamage < m_HitSpeedDuration)
        {
            speed *= m_HitSpeedFactor;
        }

        m_Input = new Vector2(playerInput.Move.x, playerInput.Move.z);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        if (!canMove)
        {
            speed = 0;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }

        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }
}