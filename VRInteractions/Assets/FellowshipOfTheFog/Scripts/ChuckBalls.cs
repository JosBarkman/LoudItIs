using System.Linq;
using UnityEngine;


public class ChuckBalls : MonoBehaviour {
    [SerializeField]
    private GameObject _ballPrefab = null;

    [SerializeField]
    private Vector2 _minMaxStrength = new Vector2( 365f, 455f );

    [SerializeField]
    private Vector2 _minMaxTimeBetweenShots = new Vector2( 0.5f, 4f );

    [SerializeField]
    private Transform _barrel = null;


    private bool _cannonActive = false;
    private float _cooldown = 0f;



    private void Start() {
        if ( null == _barrel ) {
            _barrel = ( Transform )GetComponentsInChildren<Transform>().Where( t => t != transform ).ToArray()[0];
        }
    }


    private void Update() {
        if ( Input.GetKeyDown( KeyCode.Space ) ) {
            _cannonActive = !_cannonActive;
            _cooldown = _minMaxTimeBetweenShots.y;
        }
    }


    private void FixedUpdate() {
        Shoot();
    }


    private void Shoot() {
        if ( _cannonActive && null != _ballPrefab && null != _barrel ) {
           if ( _cooldown <= 0f ) {
                // FIRE!
                ChuckBall();
                _cooldown += _minMaxTimeBetweenShots.y;
                _minMaxTimeBetweenShots.y = Mathf.Max( _minMaxTimeBetweenShots.y * 0.9f - 0.1f, _minMaxTimeBetweenShots.x );
           }
           else {
                _cooldown -= Time.fixedDeltaTime;
            }
        }
    }


    private void ChuckBall() {
        Transform ball = Instantiate( _ballPrefab ).transform;
        ball.position = _barrel.position + _barrel.up * ( _barrel.lossyScale.y + ball.lossyScale.y * 0.5f );
        Rigidbody rb = ball.GetComponent<Rigidbody>();
        float randomStrength = Random.Range( _minMaxStrength.x, _minMaxStrength.y );
        Vector2 randomDeviation = new Vector2( Random.Range( -_minMaxStrength.x, _minMaxStrength.x ) * 0.05f, Random.Range( -_minMaxStrength.x, _minMaxStrength.x ) * 0.05f );
        rb.AddForce( _barrel.up * randomStrength + _barrel.forward * randomDeviation.x + _barrel.right * randomDeviation.y );
    }
}
