using UnityEngine;
using System.Collections;

public class Lapis : MonoBehaviour
{

    public Transform physicsProp;

	void OnTriggerEnter(Collider other)
    {
        StartCoroutine(Hit());

        Transform parent = other.transform;
        while(parent.parent != null)
        {
            parent = parent.parent;
        }

        if(parent.GetComponent<kartScript>() != null)
        {
            parent.GetComponent<kartScript>().lapisAmount++;
        }

	}
	
    IEnumerator Hit()
    {
        Transform particles = (Transform)Instantiate(physicsProp, transform.position, transform.rotation);
        float originalHeight = particles.position.y;

        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        yield return new WaitForSeconds(1f);

        while(particles.position.y > originalHeight - 0.5f)
        {
            particles.position -= new Vector3(0,Time.deltaTime * 0.1f,0f);
            yield return null;
        }

        Destroy(particles.gameObject);

        yield return new WaitForSeconds(50f);

        GetComponent<Renderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }

}
