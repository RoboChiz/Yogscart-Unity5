using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class FaceToCamera : MonoBehaviour
{
    void Awake() { LookAtCamera(); }
    void Update () { LookAtCamera(); }

    void LookAtCamera()
    {
        //Find Camera and Face it
        transform.LookAt(Camera.main.transform);
    }
}
