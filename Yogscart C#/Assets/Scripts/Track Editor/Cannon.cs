using UnityEngine;
using System.Collections;

public class Cannon : MonoBehaviour {

    public Transform entryPoint, exitPoint;
    public float travelTime = 3f;

    void OnTriggerEnter(Collider other)
    {
        Transform parent = other.transform;

        while (parent.parent != null && parent.GetComponent<KartMovement>() == null)
            parent = parent.parent;

        if (parent.GetComponent<KartMovement>() != null)
        {
            StartCoroutine(FlyKart(parent));
        }
    }

    IEnumerator FlyKart(Transform kart)
    {
        kart.GetComponent<KartMovement>().locked = true;
        kart.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        float startTime = Time.time;
        Vector3 startPosition = kart.position;
        Quaternion startRotation = kart.rotation;
        Vector3 offset = entryPoint.position - startPosition;
        Vector3 endPos = exitPoint.position + (exitPoint.rotation * offset);

        while (Time.time - startTime < travelTime)
        {
            kart.position = Vector3.Slerp(startPosition, endPos, (Time.time - startTime) / travelTime);
            kart.rotation = Quaternion.Slerp(startRotation, exitPoint.rotation, (Time.time - startTime) / travelTime);
            yield return null;
        }

        kart.position = endPos;
        kart.rotation = exitPoint.rotation;

        kart.GetComponent<KartMovement>().locked = false;
        kart.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }
}
