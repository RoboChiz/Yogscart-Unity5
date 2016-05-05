using UnityEngine;
using System.Collections;

public class ItemBox : MonoBehaviour {

    public Transform PhysicsProp;

	void OnTriggerEnter(Collider other)
    {
        StartCoroutine(Hit());
	}
	
	IEnumerator Hit ()
    {
        Transform Particles = (Transform)Instantiate(PhysicsProp, transform.position, transform.rotation);
        float OriginalHeight = Particles.position.y;

        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        yield return new WaitForSeconds(1);

        while (Particles.position.y > OriginalHeight - 1)
        {
            Particles.position -= new Vector3(0,Time.deltaTime / 5f,0);
            yield return null;
        }

        Destroy(Particles.gameObject);

        GetComponent<Renderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }
}
