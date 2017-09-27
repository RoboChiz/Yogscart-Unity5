using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PigMovement : MonoBehaviour
{
    public List<Transform> points;
    public float pigSpeed = 5f;

    private int currentPoint;
    private bool grazing = false;

    private Animator ani;

    public List<AudioClip> hitNoises;
    private float lastHit = 0f;

    void Start()
    {
        ani = GetComponent<Animator>();
    }

	// Update is called once per frame
	void Update ()
    {
        //Ensure a second has passed before pigs make noise
        lastHit = Mathf.Clamp(lastHit - Time.deltaTime, 0f, 1f);

        for (int i = 0; i < points.Count; i++)
        {
            Debug.DrawLine(points[i].position, points[MathHelper.NumClamp(i + 1, 0, points.Count)].position, Color.red);
        }

		if(!grazing)
        {
            Vector3 myPos = transform.position, theirPos = points[currentPoint].position;
            myPos.y = 0f;
            theirPos.y = 0f;

            Vector3 dir = theirPos - myPos;

            if(dir.magnitude > 0.5f)
            {
                Vector3 move = dir.normalized * pigSpeed;
                GetComponent<Rigidbody>().velocity = new Vector3(move.x, GetComponent<Rigidbody>().velocity.y, move.z);

                ani.SetBool("moving", true);

                transform.rotation = Quaternion.Lerp(transform.rotation,
                    Quaternion.LookRotation(dir) * Quaternion.Euler(0f, 90f, 0f),
                    Time.deltaTime * 2f);
            }
            else
            {
                currentPoint = MathHelper.NumClamp(currentPoint + 1, 0, points.Count);

                int grazeRandom = Random.Range(0, 10);
                if (grazeRandom >= 5)
                    StartCoroutine("Graze");

                ani.SetBool("moving", false);
            }
        }
	}

    private IEnumerator Graze()
    {
        grazing = true;

        yield return new WaitForSeconds(10f);

        grazing = false;
    }

    void OnKartHit(KartMovement _kart)
    {
        if (lastHit <= 0f)
        {
            //Play a Pig Sound
            GetComponent<AudioSource>().PlayOneShot(hitNoises[Random.Range(0, hitNoises.Count)], 2f);
            lastHit = 1f;
        }
    }
}
