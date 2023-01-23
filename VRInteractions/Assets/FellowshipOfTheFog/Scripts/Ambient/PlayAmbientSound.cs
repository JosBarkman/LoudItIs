using UnityEngine;


public class PlayAmbientSound : MonoBehaviour {
    [SerializeField, Range( 0f, 1f )]
    private float _checkCooldown = 0.5f;

    [SerializeField]
    private Vector2 _minMaxDistance = new Vector2( 0.55f, 6.7f );

    [SerializeField]
    private AudioSource[] _audioPlayers = null;


    private Transform[] _windows = null;
    private float[] _maxVolumes = null;
    private Transform _player = null;
    private float _lastChecked = 0f;



    private void Start() {
        if ( null != _audioPlayers && _audioPlayers.Length > 0 ) {
            GetWindows();
            GetPlayer();
            GetAudioVolumes();
            DisableAudioVolumes();
        }
        else {
            gameObject.SetActive( false );
        }
    }


    private void FixedUpdate() {
        _lastChecked += Time.fixedTime;

        if ( _lastChecked >= _checkCooldown ) {
            _lastChecked %= _checkCooldown;

            float distanceToClosestWindow = GetClosestWindowDistance();
            SetAudioVolume( distanceToClosestWindow );
        }
    }


    #region LocateObjects

    private void GetWindows() {
        Object[] allWindows = Services.ServiceLocator.RetrieveByType( typeof( Window ) );
        _windows = new Transform[allWindows.Length];

        for ( int i = 0; i < allWindows.Length; ++i ) {
            _windows[i] = ( ( Window ) allWindows[i] ).transform.parent;
        }
    }


    private void GetPlayer() {
        Transform playerForm = ( ( Player )Services.ServiceLocator.RetrieveByType( typeof( Player ) )[0] ).transform;
        _player = playerForm.GetComponentInChildren<Camera>().transform;
    }


    private void GetAudioVolumes() {
        _maxVolumes = new float[_audioPlayers.Length];

        for ( int i = 0; i < _audioPlayers.Length; ++i ) {
            AudioSource audioPlayer = _audioPlayers[i];
            _maxVolumes[i] = audioPlayer.volume;
        }
    }

    #endregion


    #region Functionality

    private void DisableAudioVolumes() {
        // The first audio source is excluded since it has to always be present
        if ( _audioPlayers.Length > 1 ) {
            for ( int i = 1; i < _audioPlayers.Length; ++i ) {
                AudioSource audioPlayer = _audioPlayers[i];
                audioPlayer.volume = 0f;
            }
        }
    }
    
    
    private float GetClosestWindowDistance() {
        float distanceToClosestWindow = Mathf.Infinity;

        for ( int i = 0; i < _windows.Length; ++i ) {
            float distance = Vector3.Distance( _windows[i].position, _player.position );
            if ( distance < distanceToClosestWindow ) {
                distanceToClosestWindow = distance;
            }
        }

        return distanceToClosestWindow;
    }


    private void SetAudioVolume( float pClosestDistance ) {
        float distance = Mathf.Clamp( pClosestDistance, _minMaxDistance.x, _minMaxDistance.y );
        float audioStrength = 1f - ( ( distance - _minMaxDistance.x ) / ( _minMaxDistance.y - _minMaxDistance.x ) );

        // The first audio source is excluded since it has to always be present
        for ( int i = 1; i < _audioPlayers.Length; ++i ) {
            AudioSource audioPlayer = _audioPlayers[i];

            audioPlayer.volume = _maxVolumes[i] * audioStrength;
        }
    }

    #endregion
}
