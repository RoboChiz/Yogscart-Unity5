using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : Projectile
{
    public const float travelSpeed = 45f;
    private float desiredY = 0f, colliderOff = 0f;
    protected float offset = 1f;
    protected int bounces = 5;
    protected bool overrideYPos;

    private static AudioClip fireSound, bounceSound;
    private bool playedSound = false;

    public override void Setup(Vector3 _direction, bool _actingShield)
    {
        _direction.y = 0f;

        base.Setup(_direction, _actingShield);
        desiredY = transform.position.y;
    }

    // Use this for initialization
    void Start ()
    {
        if (fireSound == null)
            fireSound = Resources.Load<AudioClip>("Music & Sounds/clucky shoot egg");

        if (bounceSound == null)
            bounceSound = Resources.Load<AudioClip>("Music & Sounds/Clucky_Bounce");
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

            newPosition += direction * travelSpeed * Time.deltaTime;

            if (overrideYPos)
            {
                if (desiredY < newPosition.y)
                    newPosition.y = Mathf.Lerp(newPosition.y, desiredY, Time.deltaTime * 20f);
                else
                    newPosition.y = desiredY;
            }

            transform.position = newPosition;

            transform.rotation *= Quaternion.Euler(Vector3.one * Time.deltaTime * 45f);

            if (!playedSound)
            {
                GetComponent<AudioSource>().PlayOneShot(fireSound);
                playedSound = true;
            }

            if(colliderOff > 0)
            {
                colliderOff -= Time.deltaTime;
            }
            else
                GetComponent<SphereCollider>().enabled = true;

        }
	}

    void OnCollisionEnter(Collision collision)
    {
        if(collision.transform.GetComponent<kartScript>() != null)
        {
            //Spin the Kart Out
            collision.transform.GetComponent<kartScript>().SpinOut(true);

            //Make Owner Taunt
            DamagingItem di = GetComponent<DamagingItem>();
            if (di.owner != collision.transform.GetComponent<kartScript>())         
                di.owner.DoTaunt();

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

                colliderOff = 0.25f;
                GetComponent<SphereCollider>().enabled = false;

                GetComponent<AudioSource>().PlayOneShot(bounceSound, 3f);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
