using UnityEngine;


public class Fireplace : MonoBehaviour {
    public Vector3 NoiseOffset => _noiseOffset;


    [SerializeField]
    private Vector3 _noiseOffset = Vector3.up * 0.5f;

    [SerializeField, Range( 0f, 2f / 3f )]
    private float _flickerRange = 0.3f;

    [SerializeField, Range( 1f, 3f )]
    private float _flickerSpeed = 1.25f;

    [SerializeField]
    private Vector2 _randomFlickerSpeed = new Vector2( 0f, 0.6f );


    private Light _light = null;
    private float _startingIntensity = 1f;
    private float _startingRange = 1f;
    private float _randomTimeOffsetA = 0f;
    private float _randomTimeOffsetB = 0f;



    private void Awake() {
        Services.ServiceLocator.Subscribe( this );
    }


    private void Start() {
        _light = GetComponentInChildren<Light>();
        if ( null != _light ) {
            _startingIntensity = _light.intensity;
            _startingRange = _light.range;
            _randomTimeOffsetA = Random.Range( 0f, Mathf.PI * 2f );
            _randomTimeOffsetB = Random.Range( 0f, Mathf.PI * 2f );
        }
    }


    private void Update() {
        if ( null != _light && _flickerRange > Mathf.Epsilon ) {
            _randomTimeOffsetA -= Random.Range( _randomFlickerSpeed.x, _randomFlickerSpeed.y ) * Time.deltaTime;
            _randomTimeOffsetB -= Random.Range( _randomFlickerSpeed.x, _randomFlickerSpeed.y ) * Time.deltaTime;
            float intensityOffset = Mathf.Sin( Mathf.Sqrt( Time.time * _flickerSpeed * 0.15f ) + _randomTimeOffsetA * 1.2f ) * Mathf.Cos( ( Time.time * _flickerSpeed * 0.6f ) + ( ( Mathf.Pow( _randomTimeOffsetA, 2f ) + _randomTimeOffsetB * 3f ) * 0.005f ) ) * _flickerRange;

            _light.intensity = _startingIntensity * ( 1f + intensityOffset );
            _light.range = _startingRange * ( 1f + intensityOffset * 0.5f );
        }
    }


    private void OnDestroy() {
        Services.ServiceLocator.UnSubscribe( this );
    }
}
