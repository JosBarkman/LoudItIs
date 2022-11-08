using UnityEngine;


public class AssignRandomColour : MonoBehaviour {
    [SerializeField]
    private Color[] _options = new Color[6] {
        Color.red,
        Color.green,
        Color.blue,
        Color.cyan,
        Color.magenta,
        Color.yellow
    };



    private void Start() {
        if ( _options.Length == 0 ) return;

        Renderer renderer = GetComponent<Renderer>();
        if ( null != renderer ) {
            Material material = renderer.material;
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            Color randomColour = _options[Random.Range( 0, _options.Length )];

            block.SetColor( "_Color", randomColour );
            renderer.SetPropertyBlock( block );

            Light light = GetComponent<Light>();
            if ( null != light ) {
                light.color = randomColour;
            }
        }
    }
}
