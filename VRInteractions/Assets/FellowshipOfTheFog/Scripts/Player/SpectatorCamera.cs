using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpectatorCamera : MonoBehaviour
{
    #region Properties

    [Header("Settings")]
    public float horizontalSpeed = 0.15f;
    public float verticalSpeed = 0.15f;
    public float forwardSpeed = 0.15f;

    public float horizontalRotation = 20.0f;
    public float verticalRotation = 20.0f;

    #endregion

    #region Unity Events

    private void Update()
    {
        Vector3 translate = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Height"), Input.GetAxis("Vertical")).normalized * Time.deltaTime;
        Vector3 rotation = new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), 0.0f).normalized * Time.deltaTime;

        transform.Translate(new Vector3(translate.x * horizontalSpeed, translate.y * verticalSpeed, translate.z * forwardSpeed) );
        transform.Rotate(new Vector3(0.0f, rotation.y * horizontalRotation, 0.0f), Space.World);
        transform.Rotate(new Vector3(rotation.x * verticalRotation, 0.0f, 0.0f), Space.Self);
    }

    #endregion
}
