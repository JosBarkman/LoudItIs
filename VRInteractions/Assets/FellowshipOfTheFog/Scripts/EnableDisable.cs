using System;
using System.Collections.Generic;
using UnityEngine;


public class EnableDisable : MonoBehaviour {
    [SerializeField, Range( 0, 0.6f )]
    private float _enablingTime = 0.45f;

    [SerializeField]
    private Transform[] _objectsToTurnOff = new Transform[0];


    private Dictionary<Transform, Vector3> _objects = new Dictionary<Transform, Vector3>();
    private bool _enable = false;
    private float _enablingTimer = 0f;



    private void Start() {
        for ( int i = 0; i < _objectsToTurnOff.Length; ++i ) {
            Transform form = _objectsToTurnOff[i];
            Vector3 size = form.localScale;
            _objects.Add( form, size );
        }
    }


    public void SetActive( bool pEnable ) {
        _enable = pEnable;
        _enablingTimer = _enablingTime;

        if ( !_enable ) {
            foreach ( KeyValuePair<Transform, Vector3> pair in _objects ) {
                Transform form = pair.Key;

                Collider col = form.GetComponent<Collider>();
                if ( null != col ) {
                    col.enabled = false;
                }
            }
        }
    }


    private void Update() {
        if ( _enablingTimer > 0 ) {
            _enablingTimer -= Time.deltaTime;

            foreach ( KeyValuePair<Transform, Vector3> pair in _objects ) {
                Transform form = pair.Key;
                if ( _enable ) {
                    form.localScale = pair.Value * ( ( _enablingTime - _enablingTimer ) / _enablingTime );
                }
                else {
                    form.localScale = pair.Value * ( 1f - ( ( _enablingTime - _enablingTimer ) / _enablingTime ) );
                }

                if ( _enablingTimer <= 0 ) {
                    Collider col = form.GetComponent<Collider>();
                    if ( null != col ) {
                        col.enabled = true;
                    }
                }
            }
        }
    }
}