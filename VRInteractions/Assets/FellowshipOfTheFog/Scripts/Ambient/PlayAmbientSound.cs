using UnityEngine;


public class PlayAmbientSound : MonoBehaviour {
    [SerializeField, Range( 0f, 0.32f )]
    private float _checkCooldown = 0.09f;

    [SerializeField]
    private Vector2 _minMaxDistance = new Vector2( 0.55f, 6.7f );

    [SerializeField]
    private AudioSource[] _windowAudioPlayers = null;

    [SerializeField]
    private AudioSource[] _fireplaceAudioPlayers = null;


    private AudioWrapper[] _windowAudio = null;
    private AudioWrapper[] _fireplaceAudio = null;
    private Window[] _windows = null;
    private Fireplace[] _fireplaces = null;
    private Transform _player = null;
    private float _lastChecked = 0f;



    private void Start() {
        if ( null == _windowAudioPlayers || null == _fireplaceAudioPlayers ) {
            gameObject.SetActive( false );
            return;
        }

        GetWindows();
        GetFireplaces();
        GetPlayer();
        GenerateAudioWrappers();
        DisableAudioVolumes();
    }


    private void FixedUpdate() {
        _lastChecked += Time.fixedDeltaTime;

        if ( _lastChecked >= _checkCooldown ) {
            _lastChecked %= _checkCooldown;

            float distanceToClosestWindow = GetClosestWindowDistance();
            SetWindowVolume( distanceToClosestWindow );

            float distanceToClosestFireplace = GetClosestFireplaceDistance();
            SetFireplaceVolume( distanceToClosestFireplace );
        }
    }


    #region LocateObjects

    private void GetWindows() {
        Object[] allWindows = Services.ServiceLocator.RetrieveByType( typeof( Window ) );
        _windows = new Window[allWindows.Length];

        for ( int i = 0; i < allWindows.Length; ++i ) {
            _windows[i] = ( Window ) allWindows[i];
        }
    }


    private void GetFireplaces() {
        Object[] allFireplaces = Services.ServiceLocator.RetrieveByType( typeof( Fireplace ) );
        _fireplaces = new Fireplace[allFireplaces.Length];

        for ( int i = 0; i < allFireplaces.Length; ++i ) {
            _fireplaces[i] = ( Fireplace )allFireplaces[i];
        }
    }


    private void GetPlayer() {
        Transform playerForm = ( ( Player )Services.ServiceLocator.RetrieveByType( typeof( Player ) )[0] ).transform;
        _player = playerForm.GetComponentInChildren<Camera>().transform;
    }


    private void GenerateAudioWrappers() {
        _windowAudio = new AudioWrapper[_windowAudioPlayers.Length];
        _fireplaceAudio = new AudioWrapper[_fireplaceAudioPlayers.Length];


        for ( int i = 0; i < _windowAudioPlayers.Length; ++i ) {
            AudioSource audioPlayer = _windowAudioPlayers[i];
            _windowAudio[i] = new AudioWrapper( audioPlayer );
        }
        for ( int i = 0; i < _fireplaceAudioPlayers.Length; ++i ) {
            AudioSource audioPlayer = _fireplaceAudioPlayers[i];
            _fireplaceAudio[i] = new AudioWrapper( audioPlayer );
        }
    }

    #endregion


    #region Functionality

    private void DisableAudioVolumes() {
        // The first audio source is excluded since it has to always be present
        if ( _windowAudio.Length > 1 ) {
            for ( int i = 1; i < _windowAudio.Length; ++i ) {
                AudioSource audioPlayer = _windowAudio[i].AudioPlayer;
                audioPlayer.volume = 0f;
            }
        }
        if ( _fireplaceAudio.Length > 0 ) {
            for ( int i = 0; i < _fireplaceAudio.Length; ++i ) {
                AudioSource audioPlayer = _fireplaceAudio[i].AudioPlayer;
                audioPlayer.volume = 0f;
            }
        }
    }
    
    
    private float GetClosestWindowDistance() {
        float distanceToClosestWindow = Mathf.Infinity;

        for ( int i = 0; i < _windows.Length; ++i ) {
            Window window = _windows[i];
            float distance = Vector3.Distance( window.transform.position + window.NoiseOffset, _player.position );
            if ( distance < distanceToClosestWindow ) {
                distanceToClosestWindow = distance;
            }
        }

        return distanceToClosestWindow;
    }


    private float GetClosestFireplaceDistance() {
        float distanceToClosestFireplace = Mathf.Infinity;

        for ( int i = 0; i < _fireplaces.Length; ++i ) {
            Fireplace fireplace = _fireplaces[i];
            float distance = Vector3.Distance( fireplace.transform.position + fireplace.NoiseOffset, _player.position );
            if ( distance < distanceToClosestFireplace ) {
                distanceToClosestFireplace = distance;
            }
        }

        return distanceToClosestFireplace;
    }


    private void SetWindowVolume( float pClosestDistance ) {
        float distance = Mathf.Clamp( pClosestDistance, _minMaxDistance.x, _minMaxDistance.y );
        float audioStrength = 1f - ( ( distance - _minMaxDistance.x ) / ( _minMaxDistance.y - _minMaxDistance.x ) );

        // The first audio source is excluded since it has to always be present
        for ( int i = 1; i < _windowAudio.Length; ++i ) {
            AudioWrapper audioWrapper = _windowAudio[i];
            audioWrapper.AudioPlayer.volume = audioWrapper.MaxVolume * audioStrength;
        }
    }


    private void SetFireplaceVolume( float pClosestDistance ) {
        float distance = Mathf.Clamp( pClosestDistance, _minMaxDistance.x, _minMaxDistance.y );
        float audioStrength = 1f - ( ( distance - _minMaxDistance.x ) / ( _minMaxDistance.y - _minMaxDistance.x ) );

        for ( int i = 0; i < _fireplaceAudio.Length; ++i ) {
            AudioWrapper audioWrapper = _fireplaceAudio[i];
            audioWrapper.AudioPlayer.volume = audioWrapper.MaxVolume * audioStrength;
        }
    }

    #endregion
}
