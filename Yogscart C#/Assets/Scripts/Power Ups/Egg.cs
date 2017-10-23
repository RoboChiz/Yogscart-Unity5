using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Egg : Projectile
{
    public const float travelSpeed = 35f;
    protected int bounces = 5;
    public float desiredY = 0f;

    private static AudioClip fireSound, bounceSound;
    private bool playedSound = false;
    private float colliderOff;

    readonly string[] raycastIgnoreTags = new string[] {"Kart", "Crate", "PowerUp" };
    readonly string[] ignoreTags = new string[] { "OffRoad", "Ground"};

    public override void Setup(int _direction, bool _actingShield)
    {
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
	protected virtual void FixedUpdate ()
    {
        if (!actingShield)
        {
            //Make sure that it always stays a fix distance above ground
            RaycastHit hit;

            if (Physics.Raycast(transform.position, Vector3.down, out hit) && hit.transform.GetComponent<Collider>() != null && !hit.transform.GetComponent<Collider>().isTrigger)
            {
                bool ignore = false;
                foreach (string tag in raycastIgnoreTags)
                {
                    if (hit.transform.tag == tag)
                    {
                        ignore = true;
                        break;
                    }
                }

                if (!ignore)
                {
                    desiredY = hit.point.y + 0.5f;
                }
            }
            
            //Move Egg along direction
            Vector3 position = transform.position;
            position += (MathHelper.ZeroYPos(direction) * travelSpeed * Time.deltaTime);

            //Make Egg Travel to required height
            if (desiredY > position.y)
                position.y = Mathf.Clamp(position.y + (Time.deltaTime * 10f), position.y, desiredY);
            else if (desiredY < position.y)
                position.y = Mathf.Clamp(position.y - (Time.deltaTime * 10f), desiredY, position.y);

            transform.position = position;

            //Rotate Egg
            transform.rotation *= Quaternion.Euler(Vector3.one * Time.deltaTime * 45f);

            if (!playedSound)
            {
                GetComponent<AudioSource>().PlayOneShot(fireSound);
                playedSound = true;
            }

            if(colliderOff > 0)
            {
                colliderOff -= Time.deltaTime;
                GetComponent<Collider>().enabled = false;
            }
            else
            {
                GetComponent<Collider>().enabled = true;
            }

        }
	}

    void OnCollisionEnter(Collision collision)
    {
        bool ignore = false;
        foreach (string tag in ignoreTags)
        {
            if (collision.transform.tag == tag)
            {
                ignore = true;
                break;
            }
        }

        if (!ignore)
        {
            //Debug.Log("Collided with " + collision.transform.name);
            if (collision.transform.GetComponent<KartMovement>() != null)
            {
                //Spin the Kart Out
                collision.transform.GetComponent<KartMovement>().SpinOut(true);

                //Make Owner Taunt
                DamagingItem di = GetComponent<DamagingItem>();
                if (di.owner != collision.transform.GetComponent<KartMovement>())
                    di.owner.DoTaunt();

                //Get rid of the GameObject
                Destroy(gameObject);
            }
            else if (collision.gameObject.layer == 9 || collision.gameObject.layer == 10) //If hit another Power Up
            {
                Destroy(gameObject);
            }
            else
            {
                //Bounce the Egg off a Wall
                if (bounces > 0)
                {
                    direction = Vector3.Reflect(direction, collision.contacts[0].normal);
                    bounces--;

                    colliderOff = 0.2f;

                    GetComponent<AudioSource>().PlayOneShot(bounceSound, 3f);
                }
                else
                {
                    Destroy(gameObject);
                }
            }

        }    }
}
