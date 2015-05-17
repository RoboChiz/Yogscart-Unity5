using UnityEngine;
using System.Collections;

public class TextureScroll : MonoBehaviour {

    public int materialIndex = 0;
    public Vector2 uvAnimationRate = new Vector2( 0.0f, -0.3f );
    public string textureName = "_MainTex";

    Vector2 uvOffset = Vector2.zero;

    void LateUpdate() 
    {
        uvOffset += ( uvAnimationRate * Time.deltaTime );
		
		// Set vector components modulo 1 to prevent large offsets (avoids bugs)
		uvOffset.x = uvOffset.x % 1;
		uvOffset.y = uvOffset.y % 1;
		
        if( GetComponent<Renderer>().enabled )
        {
            GetComponent<Renderer>().materials[ materialIndex ].SetTextureOffset( textureName, uvOffset );
        }
    }
}
