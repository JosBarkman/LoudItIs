using UnityEngine;


[RequireComponent( typeof( Rigidbody ) )]
public class RbFollowTarget : MonoBehaviour {
    [SerializeField]
    private Transform _target;


    private Rigidbody _rb;



    private void Start() {
        _rb = GetComponent<Rigidbody>();

        if ( null == _rb ) {
            Debug.LogError( $"{name} can't detect RigidBody." );
        }
    }


    void FixedUpdate() {
        if ( null != _target ) {
            _rb.velocity = ( _target.position - transform.position ) / Time.fixedDeltaTime;

            Quaternion rotationDiff = _target.rotation * Quaternion.Inverse( transform.rotation );
            rotationDiff.ToAngleAxis( out float angleDeg, out Vector3 rotationAxis );

            Vector3 rotationDiffDeg = angleDeg * rotationAxis;

            _rb.angularVelocity = rotationDiffDeg * Mathf.Deg2Rad / Time.fixedDeltaTime;
        }
    }
}
