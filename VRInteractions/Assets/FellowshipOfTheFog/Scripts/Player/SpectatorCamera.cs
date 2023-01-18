using System;
using UnityEngine;
using UnityEngine.InputSystem;


public class SpectatorCamera : MonoBehaviour {
    #region Properties

    [Header( "Settings" )]

    [SerializeField, Range( 1f, 4f )]
    private float _horizontalSpeed = 10f / 3f;

    [SerializeField, Range( 1f, 4f )]
    private float _verticalSpeed = 10f / 3f;

    [SerializeField, Range( 1f, 4f )]
    private float _forwardSpeed = 10f / 3f;

    [SerializeField, Range( 0f, 1f )]
    private float _horizontalRotationSensivity = 0.15f;

    [SerializeField, Range( 0f, 1f )]
    private float _verticalRotationSensivity = 0.125f;

    [SerializeField, Range( 40f, 80f )]
    private float _maxSpeed = 66.6667f;

    [SerializeField, Range( 0f, .5f )]
    private float _accelerationModifier = .15f;

    [SerializeField, Range( 0f, .5f )]
    private float _decelerationModifier = .15f;

    private PlayerInput _playerInput;
    private Vector3 _movementSpeed = Vector3.zero;
    private Vector3 _movementInput = Vector3.zero;
    private Vector2 _rotationInput = Vector2.zero;
    private bool _sprintActivated = false;
    private bool _stealthActivated = false;

    #endregion



    #region Unity Events

    private void OnEnable() {
        if ( _playerInput == null ) {
            _playerInput = new PlayerInput();
            _playerInput.Spectator.Sprint.performed += OnSprint;
            _playerInput.Spectator.Stealth.performed += OnStealth;
        }

        _playerInput.Enable();
    }


    private void OnDisable() {
        _playerInput.Spectator.Sprint.performed -= OnSprint;
        _playerInput.Disable();
    }


    public void OnSprint( InputAction.CallbackContext context ) {
        if ( context.performed ) {
            _sprintActivated = !_sprintActivated;
        }
    }
    

    public void OnStealth( InputAction.CallbackContext context ) {
        if ( context.performed ) {
            _stealthActivated = !_stealthActivated;
        }
    }


    private void Update() {
        PlayerInput.SpectatorActions spectator = _playerInput.Spectator;
        _movementInput = spectator.Movement.ReadValue<Vector3>().normalized * Time.smoothDeltaTime;
        _rotationInput = spectator.Rotation.ReadValue<Vector2>();
    }


    private void FixedUpdate() {
        Vector3 acceleration = new Vector3( _movementInput.x * _horizontalSpeed, _movementInput.y * _verticalSpeed, _movementInput.z * _forwardSpeed ) * _accelerationModifier;
        if ( _stealthActivated ) {
            _movementSpeed += acceleration / 3f;
        }
        else if ( _sprintActivated ) {
            _movementSpeed += acceleration * 2.5f;
        }
        else {
            _movementSpeed += acceleration;
        }
        _movementSpeed = Vector3.ClampMagnitude( _movementSpeed, _maxSpeed );
        _movementInput = Vector3.zero;

        transform.Translate( _movementSpeed );

        transform.Rotate( Vector3.up * _rotationInput.x * _horizontalRotationSensivity, Space.World );
        transform.Rotate( Vector3.right * _rotationInput.y * _verticalRotationSensivity, Space.Self );
        _rotationInput = Vector2.zero;

        _movementSpeed *= 1f - _decelerationModifier;
    }

    #endregion
}