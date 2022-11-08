using UnityEngine;


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
        if ( !_enabled && Input.GetKeyDown( KeyCode.Space ) ) {
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
