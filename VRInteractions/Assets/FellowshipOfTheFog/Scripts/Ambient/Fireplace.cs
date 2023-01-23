using UnityEngine;


public class Fireplace : MonoBehaviour {
    public Vector3 NoiseOffset => _noiseOffset;


    [SerializeField]
    private Vector3 _noiseOffset = Vector3.up * 0.5f;


    private void Awake() {
        Services.ServiceLocator.Subscribe( this );
    }

    
    private void OnDestroy() {
        Services.ServiceLocator.UnSubscribe( this );
    }
}
