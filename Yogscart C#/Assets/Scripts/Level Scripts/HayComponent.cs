using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HayComponent : MonoBehaviour
{
    private ParticleSystem myParticleSystem;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;


    // Use this for initialization
    void Start ()
    {
        myParticleSystem = GetComponentInChildren<ParticleSystem>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }

    void OnTriggerEnter(Collider other)
    {
        myParticleSystem.Play();
        meshRenderer.enabled = false;
        meshCollider.enabled = false;
      
        StartCoroutine(SlideUp());
    }

    private IEnumerator SlideUp()
    {
        transform.position -= Vector3.up * 2f;

        yield return new WaitForSeconds(15f);

        meshRenderer.enabled = true;

        float startTime = Time.time, travelTime = 5f;
        Vector3 startPosition = transform.position;

        while(Time.time - startTime < travelTime)
        {
            transform.position = startPosition + Vector3.Lerp(Vector3.zero, Vector3.up * 2f, (Time.time - startTime) / travelTime);
            yield return null;
        }
    
        meshCollider.enabled = true;
        transform.position = startPosition + (Vector3.up * 2f);
    }
}
