using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;


public class CustomSlideMove : MonoBehaviour {
    [SerializeField, Range( 0, 0.5f )]
    private float _dashSpeed = 0.1f;


    private Vector3 _desiredLocation = default( Vector3 );



    public void OnFire( InputAction.CallbackContext context ) {
        if ( context.started ) { }
    }
    
    
    public void SetDesiredLocation( XRRayInteractor pInteractor ) {
        pInteractor.TryGetHitInfo( out Vector3 targetedPosition, out Vector3 normal, out int posInLine, out bool isValid );
        if ( isValid ) {
            _desiredLocation = targetedPosition;
        }
    }
    
    
    private void FixedUpdate() {
        Vector3 dislocation = Vector3.Lerp( transform.position, _desiredLocation, _dashSpeed * Time.fixedDeltaTime );
        transform.position += dislocation;
    }
}