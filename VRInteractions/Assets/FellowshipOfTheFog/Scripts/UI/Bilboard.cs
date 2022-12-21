using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bilboard : MonoBehaviour
{
    #region Unity Events

    void Update()
    {
        transform.LookAt(Camera.main.transform, Vector3.up);
    }

    #endregion
}
