using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FaceToCamera : MonoBehaviour
{
    public Transform forceCamera;
    void Awake() { LookAtCamera(); }
    void Update () { LookAtCamera(); }

    void LookAtCamera()
    {
        //Find Camera and Face it
        if (forceCamera == null)
        {
            if (Camera.main != null)
                transform.LookAt(Camera.main.transform);
            else
                gameObject.SetActive(false);
        }
        else
            transform.LookAt(forceCamera);
    }
}
