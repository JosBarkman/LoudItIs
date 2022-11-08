using UnityEngine;


public class ExpandRetractRing : MonoBehaviour {
    [SerializeField, Range( 0, 0.5f )]
    private float _amount = 0.1f;

    [SerializeField, Range( 0, 3f )]
    private float _timeScale = 1.4f;


    private Vector3 _startScale = default( Vector3 );



    private void Start() {
        _startScale = transform.localScale;
    }


    private void Update() {
        transform.localScale = _startScale * ( 1f + Mathf.Sin( Time.time * _timeScale ) * _amount );
    }
}