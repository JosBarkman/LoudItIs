using System;
using System.Collections;
using UnityEngine;


public class DieInSeconds : MonoBehaviour {
    [SerializeField, Range( 6f, 24f )]
    private float _expirationTime = 15f;


    private Light _light = null;
    private bool _isDying = false;
    private float _maxSize = 1f;
    private float _maxRange = 1f;
    private float _maxIntensity = 1f;



    private void Start() {
        StartCoroutine( Die() );

        _maxSize = Mathf.Pow( transform.localScale.x * transform.localScale.y * transform.localScale.z, 1f / 3f );

        if ( null == _light ) {
            _light = GetComponent<Light>();
            _maxRange = _light.range;
            _maxIntensity = _light.intensity;
        }
    }


    private void Update() {
        if ( _isDying ) {
            transform.localScale *= 0.985f;
            transform.localScale = new Vector3( Mathf.Max( transform.localScale.x - 0.0015f, 0 ), Mathf.Max( transform.localScale.y - 0.0015f, 0 ), Mathf.Max( transform.localScale.z - 0.0015f, 0 ) );

            float size = Mathf.Pow( transform.localScale.x * transform.localScale.y * transform.localScale.z, 1f / 3f );
            float procentualSize = size / _maxSize;

            if ( size <= 0.01f ) {
                Destroy( gameObject );
            }
            else if ( null != _light ) {
                _light.range = _maxRange * procentualSize;
                _light.intensity = _maxIntensity * procentualSize;
            }
        }
    }


    private IEnumerator Die() {
        while ( true ) {
            yield return new WaitForSeconds( _expirationTime );
            _isDying = true;
        }
    }
}
