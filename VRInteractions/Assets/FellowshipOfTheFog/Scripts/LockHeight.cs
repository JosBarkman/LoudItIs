using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockHeight : MonoBehaviour {
    [SerializeField]
    private float _height = 0.6f;


    private void Start() {
        _height = transform.position.y;
    }


    private void FixedUpdate() {
        transform.position = new Vector3( transform.position.x, _height, transform.position.z );
    }
}