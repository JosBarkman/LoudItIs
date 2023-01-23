using UnityEngine;

public class AudioWrapper {
    public AudioSource AudioPlayer => _audioPlayer;

    public float MaxVolume => _maxVolume;


    private readonly AudioSource _audioPlayer = null;
    private readonly float _maxVolume = 1f;



    public AudioWrapper( AudioSource pAudioPlayer ) {
        _audioPlayer = pAudioPlayer;
        _maxVolume = _audioPlayer.volume;
    }
}