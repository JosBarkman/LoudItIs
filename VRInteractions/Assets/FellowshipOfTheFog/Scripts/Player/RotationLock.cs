using UnityEngine;


public class RotationLock : MonoBehaviour {
    [SerializeField]
    private Transform _camera = null;

    [SerializeField]
    private Transform _rotationPivot = null;


    private bool _rotationIsLocked = false;



    public void ToggleLock() {
        _rotationIsLocked = !_rotationIsLocked;
        //LockRotation();
    }


    public void SetLock( bool isLocked ) {
        _rotationIsLocked = isLocked;
        if ( _rotationIsLocked ) {
            Debug.Log( "Locked!" );
        }
        else {
            Debug.Log( "Unlocked!" );
        }
        //LockRotation();
    }


    /*private void LockRotation() {
        if ( _rotationIsLocked ) {
            Debug.Log( "Locked!" );
            _yRotation = _camera.eulerAngles.y;
            _rotationPivot.eulerAngles = new Vector3( _rotationPivot.eulerAngles.x, _yRotation, _rotationPivot.eulerAngles.z );
        }
        else {
            Debug.Log( "Unlocked!" );
            _rotationPivot.eulerAngles = _camera.eulerAngles;
            _camera.eulerAngles = new Vector3( _camera.eulerAngles.x, _yRotation, _camera.eulerAngles.z );
        }
    }*/


    private void Update() {
        if ( _rotationIsLocked ) {
            float yRotation = -_camera.eulerAngles.y;
            _rotationPivot.eulerAngles = new Vector3( _rotationPivot.eulerAngles.x, yRotation, _rotationPivot.eulerAngles.z );
        }
    }
}