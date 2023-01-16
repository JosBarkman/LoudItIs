using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SpectatorCamera : MonoBehaviour
{
    #region Properties

    [Header("Settings")]
    public float horizontalSpeed = 0.15f;
    public float verticalSpeed = 0.15f;
    public float forwardSpeed = 0.15f;

    public float horizontalRotation = 20.0f;
    public float verticalRotation = 20.0f;

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
        Vector3 translate = input.Spectator.Movement.ReadValue<Vector3>() * Time.deltaTime;
        Vector2 rotation = input.Spectator.Rotation.ReadValue<Vector2>() * Time.deltaTime;

        transform.Translate(new Vector3(translate.x * horizontalSpeed, translate.y * verticalSpeed, translate.z * forwardSpeed));

        transform.Rotate(new Vector3(0.0f, rotation.x * horizontalRotation, 0.0f), Space.World);
        transform.Rotate(new Vector3(rotation.y * verticalRotation, 0.0f, 0.0f), Space.Self);
    }

    #endregion
}
