using UnityEngine;
using UnityEngine.InputSystem;

public class EnableGravity : MonoBehaviour {
    [SerializeField]
    private bool _startEnabled = false;


    private bool _enabled = false;



    private void Start() {
        if ( _startEnabled ) {
            SetEnabled( true );
        }
        else {
            SetEnabled( false );
        }
    }


    private void Update() {
        if ( !_enabled && Keyboard.current[Key.Space].wasPressedThisFrame) {
            SetEnabled( true );
        }
    }


    private void SetEnabled( bool pEnable ) {
        _enabled = pEnable;

        Rigidbody rb = GetComponent<Rigidbody>();
        if ( null != rb ) {
            rb.useGravity = _enabled;
        }
    }
}
