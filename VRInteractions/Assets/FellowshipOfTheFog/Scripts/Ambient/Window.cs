using UnityEngine;


public class Window : MonoBehaviour {
    private void Awake() {
        Services.ServiceLocator.Subscribe( this );
    }

    
    private void OnDestroy() {
        Services.ServiceLocator.UnSubscribe( this );
    }
}
