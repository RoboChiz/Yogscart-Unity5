using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : Projectile
{
    public const float travelSpeed = 30f;
    private float desiredY = 0f;
    protected float offset = 1f;
    protected int bounces = 3;

    public override void Setup(Vector3 _direction, bool _actingShield)
    {
        base.Setup(_direction, _actingShield);
        desiredY = transform.position.y;
    }

    // Use this for initialization
    void Start ()
    {
		
	}
	
	// Update is called once per frame
	protected virtual void Update ()
    {
        if (!actingShield)
        {
            //Make sure that it always stays a fix distance above ground
            RaycastHit hit;
            var layerMask = 1 << 10;
            layerMask = ~layerMask;

            if (Physics.Raycast(transform.position + Vector3.up,Vector3.down,out hit, Mathf.Infinity, layerMask))
            {
                desiredY = hit.point.y + offset;
            }

            Vector3 newPosition = transform.position;

            newPosition += new Vector3(direction.x, 0f, direction.z) * travelSpeed * Time.deltaTime;
            newPosition.y = Mathf.Lerp(newPosition.y, desiredY, Time.deltaTime * 20f);

            transform.position = newPosition;

            transform.rotation *= Quaternion.Euler(Vector3.one * Time.deltaTime * 45f);

        }
	}

    void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.GetComponent<kartScript>() != null)
        {
            //Spin the Kart Out
            collision.transform.GetComponent<kartScript>().SpinOut();

            //Get rid of the GameObject
            Destroy(gameObject);
        }
        else if(collision.transform.GetComponent<Egg>()) //If hit another Power Up
        {
            Destroy(gameObject);
        }
        else
        {
            //Bounce the Egg off a Wall
            if(bounces > 0)
            {
                direction = Vector3.Reflect(direction, collision.contacts[0].normal);
                bounces --;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
