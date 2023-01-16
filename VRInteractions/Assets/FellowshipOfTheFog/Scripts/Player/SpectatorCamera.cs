using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpectatorCamera : MonoBehaviour
{
    #region Properties

    [Header("Settings")]

    [SerializeField]
    private float horizontalSpeed = 0.15f;
    
    [SerializeField]
    private float verticalSpeed = 0.15f;
    
    [SerializeField]
    private float forwardSpeed = 0.15f;

    [SerializeField]
    private float horizontalRotationSensivity = 20.0f;
    
    [SerializeField]
    private float verticalRotationSensivity = 20.0f;

    [SerializeField]
    private float maxSpeed = 1.0f;

    [SerializeField]
    private float accelerationModifier = .1f;
    
    [SerializeField]
    private float decelerationModifier = .1f;

    private Vector3 movementSpeed = Vector3.zero;
    private PlayerInput input;

    #endregion

    #region Unity Events

    private void OnEnable()
    {
        if (input == null)
        {
            input = new PlayerInput();
        }

        input.Enable();
    }

    private void OnDisable()
    {
        input.Disable();
    }

    private void Update()
    {
        Vector3 translate = input.Spectator.Movement.ReadValue<Vector3>().normalized * Time.deltaTime;

        Vector3 speed = new Vector3(translate.x * horizontalSpeed, translate.y * verticalSpeed, translate.z * forwardSpeed) * accelerationModifier;
        movementSpeed += speed;

        movementSpeed = movementSpeed + movementSpeed * -1.0f * decelerationModifier;

        movementSpeed = Vector3.ClampMagnitude(movementSpeed, maxSpeed);

        transform.Translate(movementSpeed);

        Vector2 rotation = input.Spectator.Rotation.ReadValue<Vector2>() * Time.deltaTime * horizontalRotationSensivity;

        transform.Rotate(Vector3.up * rotation.x, Space.World);
        transform.Rotate(Vector3.right * rotation.y, Space.Self);
    }

    #endregion
}
