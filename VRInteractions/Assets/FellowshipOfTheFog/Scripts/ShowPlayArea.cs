using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;



public class ShowPlayArea : MonoBehaviour {
    [SerializeField]
    private Transform _player = null;


    private List<Vector3> _boundaryPoints = new List<Vector3>();



    private void OnDrawGizmos() {
        if ( _boundaryPoints.Count > 0 && null != _player ) {
            Gizmos.color = Color.white;
            for ( int i = 0; i < _boundaryPoints.Count; ++i ) {
                int next = ( i + 1 ) % _boundaryPoints.Count;
                Vector3 point = _player.position + _boundaryPoints[i];
                float height = 3f;
                Vector3 highPoint = point + Vector3.up * height;
                Vector3 nextPoint = _player.position + _boundaryPoints[next];
                Vector3 nextHightPoint = nextPoint + Vector3.up * height;

                Gizmos.DrawLine( point, nextPoint );
                Gizmos.DrawLine( point, highPoint );
                Gizmos.DrawLine( highPoint, nextHightPoint );
            }
        }
    }


    private void Start() {
        List<XRInputSubsystem> inputSubsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetInstances<XRInputSubsystem>( inputSubsystems );

        if ( inputSubsystems.Count > 0 ) {
            XRInputSubsystem inputSubsystem = inputSubsystems[0];
            if ( !inputSubsystem.running ) {
                // Start the subsystem if not started yet
                inputSubsystem.Start();
            }
            if ( inputSubsystem.TryGetBoundaryPoints( _boundaryPoints ) ) {
                Debug.Log( $"Getting Boundaries succeeded: {_boundaryPoints.Count} points found." );
            }
            else {
                Debug.Log( "No Boundary points found." );
            }
        }
        else {
            Debug.Log( "No Subsytems found." );
        }
    }
}