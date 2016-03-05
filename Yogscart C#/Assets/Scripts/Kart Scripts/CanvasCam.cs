using UnityEngine;
using System.Collections;

public class CanvasCam : MonoBehaviour
{	
	// Update is called once per frame
	void Update ()
    {
        if (Camera.main != null)
            transform.rotation = Quaternion.LookRotation(Camera.main.transform.forward);
    }
}
