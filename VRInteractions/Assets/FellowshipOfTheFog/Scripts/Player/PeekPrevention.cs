using UnityEngine;


public class PeekPrevention : MonoBehaviour {
    [SerializeField]
    private float _fadeSpeed = 0f;

    [SerializeField, Range( 0.05f, 0.25f )]
    private float _range = 0.15f;

    [SerializeField]
    private LayerMask _mask = 0;


    private Material _camFadeMaterial = null;
    private bool _isFaded = false;



    private void Awake() {
        _camFadeMaterial = GetComponent<Renderer>().material;
    }


    private void Update() {
        if ( Physics.CheckSphere( transform.position, _range, _mask, QueryTriggerInteraction.Ignore ) ) {
            CameraFade( 1f );
            _isFaded = true;
        }
        else {
            if ( _isFaded ) {
                CameraFade( 0f );
            }
        }
    }


    private void CameraFade( float pTargetAlpha ) {
        float fadeVal = Mathf.MoveTowards( _camFadeMaterial.GetFloat( "_Alpha" ), pTargetAlpha, Time.deltaTime * _fadeSpeed );

        if ( fadeVal <= Mathf.Epsilon ) {
            _camFadeMaterial.SetFloat( "_Alpha", 0 );
            _isFaded = false;
        }
        else {
            _camFadeMaterial.SetFloat( "_Alpha", fadeVal );
        }
    }


    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere( transform.position, _range );
    }
}