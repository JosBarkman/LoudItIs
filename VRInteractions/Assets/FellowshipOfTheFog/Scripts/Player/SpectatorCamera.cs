using UnityEngine;
using UnityEngine.InputSystem;


public class SpectatorCamera : MonoBehaviour {
    #region Properties

    [Header( "Settings" )]

    [SerializeField, Range( 0f, 1f )]
    private float _horizontalSpeed = 0.15f;

    [SerializeField, Range( 0f, 1f )]
    private float _verticalSpeed = 0.15f;

    [SerializeField, Range( 0f, 1f )]
    private float _forwardSpeed = 0.15f;

    [SerializeField, Range( 10f, 30f )]
    private float _horizontalRotationSensivity = 20.0f;

    [SerializeField, Range( 10f, 30f )]
    private float _verticalRotationSensivity = 20.0f;

    [SerializeField, Range( .5f, 2f )]
    private float _maxSpeed = 1.0f;

    [SerializeField, Range( 0f, .5f )]
    private float _accelerationModifier = .1f;

    [SerializeField, Range( 0f, .5f )]
    private float _decelerationModifier = .1f;

    private Vector3 _movementSpeed = Vector3.zero;
    private PlayerInput _input;
    #endregion



    #region Unity Events

    private void OnEnable() {
        if ( _input == null ) {
            _input = new PlayerInput();
        }

        _input.Enable();
    }


    private void OnDisable() {
        _input.Disable();
    }


    private void FixedUpdate() {
        Vector3 translate = _input.Spectator.Movement.ReadValue<Vector3>().normalized * Time.fixedDeltaTime;

        Vector3 speed = new Vector3( translate.x * _horizontalSpeed, translate.y * _verticalSpeed, translate.z * _forwardSpeed ) * _accelerationModifier;
        _movementSpeed += speed;

        _movementSpeed = _movementSpeed + _movementSpeed * -1.0f * _decelerationModifier;

        _movementSpeed = Vector3.ClampMagnitude( _movementSpeed, _maxSpeed );

        transform.Translate( _movementSpeed );

        Vector2 rotation = _input.Spectator.Rotation.ReadValue<Vector2>() * Time.fixedDeltaTime;

        transform.Rotate( Vector3.up * rotation.x * _horizontalRotationSensivity, Space.World );
        transform.Rotate( Vector3.right * rotation.y * _verticalRotationSensivity, Space.Self );
    }

    #endregion
}