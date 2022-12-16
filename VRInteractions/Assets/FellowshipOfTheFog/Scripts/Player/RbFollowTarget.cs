using UnityEngine;


[RequireComponent( typeof( Rigidbody ) )]
public class RbFollowTarget : MonoBehaviour {
    public enum Hand {
        LEFT,
        RIGHT,
        NONE
    }


    [SerializeField]
    private Transform _target = null;

    [SerializeField]
    private Hand _hand = Hand.NONE;

    [SerializeField]
    private LayerMask _mask = 0;

    [SerializeField]
    private Transform[] _handEdges = new Transform[0];


    private Rigidbody _rb = null;



    private void Start() {
        _rb = GetComponent<Rigidbody>();

        if ( null == _rb ) {
            Debug.LogError( $"{name} can't detect RigidBody." );
        }
    }


    private void FixedUpdate() {
        if ( null != _target ) {
            //SetApproach();

            VelocityApproach();
        }
    }


    /*private void SetApproach() {
        bool collided = false;
        for ( int i = 0; i < _handEdges.Length; ++i ) {
            Vector3 handEdge = Vector3.RotateTowards( _handEdges[i].localPosition, _target.forward, Mathf.PI * 2f, Mathf.Infinity );
            Vector3 ourPosition = transform.position + handEdge;
            Vector3 desiredPosition = _target.position + handEdge;

            Ray ray = new Ray( ourPosition, ( desiredPosition - ourPosition ).normalized );
            //Vector3 stopPoint = _target.position;

            if ( Physics.Raycast( ray, out RaycastHit hit, ( desiredPosition - ourPosition ).magnitude, _mask, QueryTriggerInteraction.Ignore ) ) {
                //stopPoint = hit.point;
                collided = true;
            }
            //else {
            //    transform.position = _target.position;
            //}

            //transform.position = stopPoint;
            switch ( _hand ) {
                case Hand.LEFT:
                    transform.rotation = _target.rotation * Quaternion.Euler( -90, 90, 0 );
                    break;
                case Hand.RIGHT:
                    transform.rotation = _target.rotation * Quaternion.Euler( 90, -90, 0 );
                    break;
                default:
                    transform.rotation = _target.rotation;
                    break;
            }
        }

        if ( !collided ) {
            transform.position = _target.position;
        }
    }*/


    private void VelocityApproach() {
        Quaternion targetRotation;
        switch ( _hand ) {
            case Hand.LEFT:
                targetRotation = _target.rotation * Quaternion.Euler( -90, 90, 0 );
                break;
            case Hand.RIGHT:
                targetRotation = _target.rotation * Quaternion.Euler( 90, -90, 0 );
                break;
            default:
                targetRotation = _target.rotation;
                break;
        }

        _rb.velocity = ( _target.position - transform.position ) / Time.fixedDeltaTime;
        _rb.MoveRotation( targetRotation );


        /*Quaternion rotationDiff = targetRotation * Quaternion.Inverse( transform.rotation );
        rotationDiff.ToAngleAxis( out float angleDeg, out Vector3 rotationAxis );

        Vector3 rotationDiffDeg = angleDeg * rotationAxis;

        _rb.angularVelocity = rotationDiffDeg * Mathf.Deg2Rad / Time.fixedDeltaTime;*/
    }
}