using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using StarterAssets;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
*/

[RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
[RequireComponent(typeof(PlayerInput))]
#endif
public class MyThirdPersonController : MonoBehaviour
{
    [Header("Player")]
    [Tooltip("Move speed of the character in m/s")]
    public float MoveSpeed = 2.0f;

    [Tooltip("Sprint speed of the character in m/s")]
    public float SprintSpeed = 5.335f;

    [Tooltip("How fast the character turns to face movement direction")]
    [Range(0.0f, 3f)]
    public float DefaultRotationSmoothTime = 0.12f;

    [Tooltip("Acceleration and deceleration")]
    public float SpeedChangeRate = 10.0f;

    public AudioClip LandingAudioClip;
    public AudioClip[] FootstepAudioClips;
    [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

    [Space(10)]
    [Tooltip("The height the player can jump")]
    public float JumpHeight = 1.2f;

    [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
    public float DefaultGravity = -15.0f;

    [Space(10)]
    [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
    public float JumpTimeout = 0.50f;

    [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
    public float FallTimeout = 0.15f;
    
    [Tooltip("Freefall terminal speed. Should be negative.")]
    public float MaxFallSpeed;

    [Tooltip("Jump upwards portion terminal speed")]
    public float MaxRiseSpeed;

    [Header("Player Grounded")]
    [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
    public bool Grounded = true;

    [Tooltip("Useful for rough ground")]
    public float GroundedOffset = -0.14f;

    [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
    public float GroundedRadius = 0.28f;

    [Tooltip("What layers the character uses as ground")]
    public LayerMask GroundLayers;

    [Header("Cinemachine")]
    [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
    public GameObject CinemachineCameraTarget;

    [Tooltip("How far in degrees can you move the camera up")]
    public float TopClamp = 70.0f;

    [Tooltip("How far in degrees can you move the camera down")]
    public float BottomClamp = -30.0f;

    [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
    public float CameraAngleOverride = 0.0f;

    [Tooltip("For locking the camera position on all axis")]
    public bool LockCameraPosition = false;

    [Header("Flight")]
    public GameObject glider;
    public WindState WindState;
    public float FlightStartSpeed = 5f;
    public float FlightGravity;
    public float FlightMinMoveSpeed = 2f;
    public float FlightMaxMoveSpeed = 12f;
    public float FlightMaxDiveAccel = 10f;
    public float FlightMaxRiseSlow = -8f;
    [Tooltip("Below this speed, gravity takes over.")]
    public float MinSpeedForRiseCapacity = 3;
    [Tooltip("Up to this speed, rise capacity is improved.")]
    public float MaxSpeedForRiseCapacity = 10;
    public float FlightYRotationSmoothTime;
    public float FlightXRotationSmoothTime;
    public float FlightZRotationSmoothTime;
    public float FlightGravityRotationSmoothTime = 0.3f;
    public float FlightMaxPitch = 32.5f;
    public float FlightMaxRoll = 45f;
    public float FlightPitchForNeutralSpeed = 15f;


    // cinemachine
    private float _cinemachineTargetYaw;
    private float _cinemachineTargetPitch;

    // player
    private float _speed;
    private float _animationBlend;
    private float _targetYawRotation = 0.0f;
    private float _targetPitchRotation = 0.0f;
    private float _targetRollRotation = 0.0f;
    private float _yRotationVelocity;
    private float _xRotationVelocity;
    private float _zRotationVelocity;
    private float _yFlightRotationVelocity;
    private float _xFlightRotationVelocity;
    private float _zFlightRotationVelocity;
    //only used outside flight
    private float _verticalVelocity;
    private bool _flying;

    // timeout deltatime
    private float _jumpTimeoutDelta;
    private float _fallTimeoutDelta;

    // animation IDs
    private int _animIDSpeed;
    private int _animIDGrounded;
    private int _animIDJump;
    private int _animIDFreeFall;
    private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
    private PlayerInput _playerInput;
#endif
    private Animator _animator;
    private CharacterController _controller;
    private InputReader _input;
    private GameObject _mainCamera;

    private const float _threshold = 0.01f;

    private bool _hasAnimator;

    private bool IsCurrentDeviceMouse
    {
        get
        {
#if ENABLE_INPUT_SYSTEM
            return _playerInput.currentControlScheme == "KeyboardMouse";
#else
			return false;
#endif
        }
    }

    private void Awake()
    {
        // get a reference to our main camera
        if (_mainCamera == null)
        {
            _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
        }
    }

    private void Start()
    {
        _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

        _hasAnimator = TryGetComponent(out _animator);
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<InputReader>();
#if ENABLE_INPUT_SYSTEM
        _playerInput = GetComponent<PlayerInput>();
#else
		Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

        AssignAnimationIDs();

        // reset our timeouts on start
        _jumpTimeoutDelta = JumpTimeout;
        _fallTimeoutDelta = FallTimeout;

        _flying = false;
    }

    private void Update()
    {
        _hasAnimator = TryGetComponent(out _animator);
        bool wasFlying = _flying;

        GroundedCheck();
        if (Grounded)
            _input.fly = false;

        _flying = _input.fly;
        glider.SetActive(_flying);

        if (!_flying)
        {
            JumpAndGravity();
            GroundMove();
        }
        else
        {
            bool flightStart = _flying && !wasFlying;
            FlightMove(flightStart);
        }
    }

    private void LateUpdate()
    {
        CameraRotation();
    }

    private void AssignAnimationIDs()
    {
        _animIDSpeed = Animator.StringToHash("Speed");
        _animIDGrounded = Animator.StringToHash("Grounded");
        _animIDJump = Animator.StringToHash("Jump");
        _animIDFreeFall = Animator.StringToHash("FreeFall");
        _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
    }

    private void GroundedCheck()
    {
        // set sphere position, with offset
        Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
            transform.position.z);
        Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
            QueryTriggerInteraction.Ignore);

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetBool(_animIDGrounded, Grounded);
        }
    }

    private void CameraRotation()
    {
        // if there is an input and camera position is not fixed
        if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
        {
            //Don't multiply mouse input by Time.deltaTime;
            float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

            _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
            _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
        }

        // clamp our rotations so our values are limited 360 degrees
        _cinemachineTargetYaw = Clamp360Angle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
        _cinemachineTargetPitch = Clamp360Angle(_cinemachineTargetPitch, BottomClamp, TopClamp);

        // Cinemachine will follow this target
        CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
            _cinemachineTargetYaw, 0.0f);
    }

    private void GroundMove()
    {
        //SPEED
        float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
        // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        // if there is no input, set the target speed to 0
        if (_input.move == Vector2.zero) targetSpeed = 0.0f;

        float currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;
        float speedOffset = 0.1f;
        float inputMagnitude = _input.analogMovement ? _input.move.magnitude : 1f;

        // accelerate or decelerate to target speed
        if (currentHorizontalSpeed < targetSpeed - speedOffset ||
            currentHorizontalSpeed > targetSpeed + speedOffset)
        {
            // creates curved result rather than a linear one giving a more organic speed change
            // note T in Lerp is clamped, so we don't need to clamp our speed
            _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude,
                Time.deltaTime * SpeedChangeRate);

            // round speed to 3 decimal places
            _speed = Mathf.Round(_speed * 1000f) / 1000f;
        }
        else
        {
            _speed = targetSpeed;
        }

        //ROTATION
        // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
        //YAW + PITCH + ROLL
        //Smooth pitch and roll towards 0 (in case we just landed)
        float xRotation = Mathf.SmoothDampAngle(transform.eulerAngles.x, 0f, ref _xRotationVelocity, DefaultRotationSmoothTime);
        float zRotation = Mathf.SmoothDampAngle(transform.eulerAngles.z, 0f, ref _zRotationVelocity, DefaultRotationSmoothTime);
        float yRotation;
        //Smooth yaw towards movement direction
        if (_input.move != Vector2.zero)
        {
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;
            _targetYawRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + _mainCamera.transform.eulerAngles.y;
            yRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetYawRotation, ref _yRotationVelocity, DefaultRotationSmoothTime);
        }
        else
            yRotation = transform.eulerAngles.y;

        // rotate to face input direction relative to camera position
        transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);

        Vector3 targetDirection = Quaternion.Euler(0.0f, _targetYawRotation, 0.0f) * Vector3.forward;
        
        //DO MOVE
        _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) + new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

        //ANIMATOR
        _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
        }

    }

    private void FlightMove(bool flightStart)
    {
        //ROTATE
        float riseCapacity = RiseCapacityForCurrentSpeed();
        float xRotation;
        if (riseCapacity > 0)
        {
            //Allow user control over pitch

            float maxUpwardsPitch = -Mathf.Lerp(0f, FlightMaxPitch, riseCapacity);
            if (Mathf.Approximately(_input.move.y, 0))
                //keep current pitch but clamp to updated rise capacity
                _targetPitchRotation = ClampEulerAngle(transform.eulerAngles.x, maxUpwardsPitch, FlightMaxPitch);
            else if (_input.move.y >= 0)
                //dive
                _targetPitchRotation = Mathf.Lerp(0, FlightMaxPitch, _input.move.y);
            else
                //rise as much as possible
                _targetPitchRotation = Mathf.Lerp(maxUpwardsPitch, 0, _input.move.y + 1);
            
            xRotation = Mathf.SmoothDampAngle(transform.eulerAngles.x, _targetPitchRotation, ref _xFlightRotationVelocity, FlightXRotationSmoothTime);
        }
        else
        {
            //gravity takes over
            //todo: can still dive even faster if desired
            _targetPitchRotation = 85f;
            
            xRotation = Mathf.SmoothDampAngle(transform.eulerAngles.x, _targetPitchRotation, ref _xFlightRotationVelocity, FlightGravityRotationSmoothTime);
        }


        _targetYawRotation =  this.transform.eulerAngles.y + (90f * _input.move.x);
        _targetRollRotation = _input.move.x * -FlightMaxRoll;
        float yRotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetYawRotation, ref _yFlightRotationVelocity, FlightYRotationSmoothTime);
        float zRotation = Mathf.SmoothDampAngle(transform.eulerAngles.z, _targetRollRotation, ref _zFlightRotationVelocity, FlightZRotationSmoothTime);

        transform.rotation = Quaternion.Euler(xRotation, yRotation, zRotation);

        //ACCELERATE

        Vector3 currentForward = transform.rotation * Vector3.forward;
        float deltaFromDown = Vector3.Angle(currentForward, Vector3.down);
        float pitchAcceleration;
        float deltaFromDownForNeutralSpeed = 90 - FlightPitchForNeutralSpeed;
        if (deltaFromDown <= deltaFromDownForNeutralSpeed)
            pitchAcceleration = Mathf.Lerp(FlightMaxDiveAccel, 0, deltaFromDown / deltaFromDownForNeutralSpeed);
        else
        {
            float deltaFromNeutral = deltaFromDown - deltaFromDownForNeutralSpeed;
            float maxDeltaFromNeutral = FlightMaxPitch + 90 - deltaFromDownForNeutralSpeed;
            pitchAcceleration = Mathf.Lerp(0, FlightMaxRiseSlow, deltaFromNeutral / maxDeltaFromNeutral);
        }

        float previousSpeed;
        if (flightStart)
        {
            previousSpeed = FlightStartSpeed;
        } else
        {
            previousSpeed = _controller.velocity.magnitude;
        }

        float newSpeed = previousSpeed + (pitchAcceleration * Time.deltaTime);        
        newSpeed = Mathf.Clamp(newSpeed, FlightMinMoveSpeed, FlightMaxMoveSpeed);

        _controller.Move(currentForward * newSpeed * Time.deltaTime);

        //update vertical velocity to be used when switching out of flight mode
        _verticalVelocity = _controller.velocity.y;

        //ANIMATE
        _animationBlend = Mathf.Lerp(_animationBlend, newSpeed, Time.deltaTime * SpeedChangeRate);
        if (_animationBlend < 0.01f) _animationBlend = 0f;

        // update animator if using character
        if (_hasAnimator)
        {
            _animator.SetFloat(_animIDSpeed, _animationBlend);
            _animator.SetFloat(_animIDMotionSpeed, 1f);
        }
    }

    private float RiseCapacityForCurrentSpeed()
    {
        //float currentHorizontalSpeed = Vector3.ProjectOnPlane(_controller.velocity, Vector3.up).magnitude;
        float currentSpeed = _controller.velocity.magnitude;
        
        if (currentSpeed <= MinSpeedForRiseCapacity)
            return 0;
        else if (MaxSpeedForRiseCapacity <= MinSpeedForRiseCapacity)
            return 1;
        else
        {
            float capacity = (currentSpeed - MinSpeedForRiseCapacity) / (MaxSpeedForRiseCapacity - MinSpeedForRiseCapacity);
            return Mathf.Clamp(capacity, 0f, 1f);
        }
    }

    private void JumpAndGravity()
    {
        if (Grounded)
        {
            // reset the fall timeout timer
            _fallTimeoutDelta = FallTimeout;

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDJump, false);
                _animator.SetBool(_animIDFreeFall, false);
            }

            // stop our velocity dropping infinitely when grounded
            if (_verticalVelocity < 0.0f)
            {
                _verticalVelocity = -2f;
            }

            // Jump
            if (_input.jump && _jumpTimeoutDelta <= 0.0f)
            {
                // the square root of H * -2 * G = how much velocity needed to reach desired height
                _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * DefaultGravity);

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, true);
                }

                //MODIF YOANN: Consume input to avoid staying in jump animation if jumping at slope (doesn't unground)
                _input.jump = false;
                //MODIF YOANN : set timeout immediately to avoid spamming jumps to prevent land animation
                _jumpTimeoutDelta = JumpTimeout;
            }

            // jump timeout
            if (_jumpTimeoutDelta >= 0.0f)
            {
                _jumpTimeoutDelta -= Time.deltaTime;
            }
        }
        else
        {            
            // reset the jump timeout timer
            _jumpTimeoutDelta = JumpTimeout;

            // fall timeout
            if (_fallTimeoutDelta >= 0.0f)
            {
                _fallTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDFreeFall, true);
                }
            }

            // if we are not grounded, do not jump
            _input.jump = false;

        }

        _verticalVelocity += DefaultGravity * Time.deltaTime;
        _verticalVelocity = Mathf.Clamp(_verticalVelocity, MaxFallSpeed, MaxRiseSpeed);        
    }

    private static float Clamp360Angle(float lfAngle, float lfMin, float lfMax)
    {
        if (lfAngle < -360f) lfAngle += 360f;
        if (lfAngle > 360f) lfAngle -= 360f;
        return Mathf.Clamp(lfAngle, lfMin, lfMax);
    }

    //https://gist.github.com/johnsoncodehk/2ecb0136304d4badbb92bd0c1dbd8bae
    //Does some voodoo to angles returned by transform.euleurangles to allow clamping them
    //needed because transform.euleurangles returns variable representation
    public static float ClampEulerAngle(float angle, float min, float max)
    {
        float start = (min + max) * 0.5f - 180;
        float floor = Mathf.FloorToInt((angle - start) / 360) * 360;
        return Mathf.Clamp(angle, min + floor, max + floor);
    }

    private void OnDrawGizmosSelected()
    {
        Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
        Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

        if (Grounded) Gizmos.color = transparentGreen;
        else Gizmos.color = transparentRed;

        // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
        Gizmos.DrawSphere(
            new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
            GroundedRadius);
    }

    private void OnFootstep(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            if (FootstepAudioClips.Length > 0)
            {
                var index = Random.Range(0, FootstepAudioClips.Length);
                AudioSource.PlayClipAtPoint(FootstepAudioClips[index], transform.TransformPoint(_controller.center), FootstepAudioVolume);
            }
        }
    }

    private void OnLand(AnimationEvent animationEvent)
    {
        if (animationEvent.animatorClipInfo.weight > 0.5f)
        {
            AudioSource.PlayClipAtPoint(LandingAudioClip, transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }
    }
}
