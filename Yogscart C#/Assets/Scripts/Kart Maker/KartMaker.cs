using UnityEngine;
using System.Collections;

public enum KartType { Display, Local, Online, Spectator };

public class KartMaker : MonoBehaviour
{

    static CurrentGameData gd; 

    void Start()
    {
        gd = transform.GetComponent<CurrentGameData>();
    }

    public Transform SpawnKart(KartType type, Vector3 pos, Quaternion rot,int c, int h, int k, int w)
    {
        //Spawn Kart and Wheels
        Transform kartBody = (Transform)Instantiate(gd.karts[k].model, Vector3.zero, Quaternion.identity);
        Kart_Skeleton kartSkel = kartBody.GetComponent<Kart_Skeleton>();

        Transform frontlWheel, frontrWheel, backlWheel, backrWheel;

        frontlWheel = (Transform)Instantiate(gd.wheels[w].model, kartSkel.FrontLPosition, Quaternion.Euler(0, 0, 0));
        frontlWheel.parent = kartBody.FindChild("Kart Body");
        frontlWheel.name = "FrontL Wheel";

        frontrWheel = (Transform)Instantiate(gd.wheels[w].model, kartSkel.FrontRPosition, Quaternion.Euler(0, 180, 0));
        frontrWheel.parent = kartBody.FindChild("Kart Body");
        frontrWheel.name = "FrontR Wheel";

        backlWheel = (Transform)Instantiate(gd.wheels[w].model, kartSkel.BackLPosition, Quaternion.Euler(0, 0, 0));
        backlWheel.parent = kartBody.FindChild("Kart Body");
        backlWheel.name = "BackL Wheel";

        backrWheel = (Transform)Instantiate(gd.wheels[w].model, kartSkel.BackRPosition, Quaternion.Euler(0, 180, 0));
        backrWheel.parent = kartBody.FindChild("Kart Body");
        backrWheel.name = "BackR Wheel";

        //Spawn Character and Hat
        Transform characterMesh = (Transform)Instantiate(gd.characters[c].model, Vector3.zero, Quaternion.identity);
        characterMesh.name = "Character";

        Character_Skeleton charSkel = transform.GetComponent<Character_Skeleton>();

        characterMesh.position = kartSkel.SeatPosition - charSkel.SeatPosition;
        characterMesh.parent = kartBody.FindChild("Kart Body");

        if (h != 0)//Don't spawn a hat if hat value equals zero
        {
            Transform hatMesh = (Transform)Instantiate(gd.hats[h].model, charSkel.HatHolder.position, Quaternion.identity);
            hatMesh.parent = charSkel.HatHolder;
            hatMesh.localRotation = Quaternion.Euler(0, 0, 0);
        }

        if(type != KartType.Display)
        {
            GameObject kb = kartBody.gameObject;

            //Setup Rigidbody
            Rigidbody rb = kb.AddComponent<Rigidbody>();
            rb.mass = 1000;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.angularDrag = 0.05f;

            //Set up Kart sounds
            AudioSource aus = kb.AddComponent<AudioSource>();
            aus.clip = kartSkel.engineSound;
            aus.spatialBlend = 1;
            aus.minDistance = 0f;
            aus.maxDistance = 35f;
            aus.rolloffMode = AudioRolloffMode.Linear;
            aus.playOnAwake = false;

            //Set up Kart Body Sounds
            AudioSource kbas = kartBody.FindChild("Kart Body").gameObject.AddComponent<AudioSource>();
            kbas.spatialBlend = 1;
            kbas.minDistance = 0f;
            kbas.maxDistance = 25f;
            kbas.rolloffMode = AudioRolloffMode.Linear;
            kbas.playOnAwake = false;

            //Add Death Catch
            kb.AddComponent<DeathCatch>();
            kb.GetComponent<DeathCatch>().deathParticles = kartBody.FindChild("Kart Body").FindChild("Particles").FindChild("Death Particles").GetComponent<ParticleSystem>();

            //Setup Wheel Colliders
            Transform frontlWheelCollider = (Transform)Instantiate(gd.wheels[w].model, kartSkel.FrontLPosition, Quaternion.Euler(0, 0, 0));
            frontlWheelCollider.name = "FrontL Wheel";
            frontlWheelCollider.parent = kartBody.FindChild("Colliders");
            SetUpWheelCollider(frontlWheelCollider);

            Transform frontrWheelCollider = (Transform)Instantiate(gd.wheels[w].model, kartSkel.FrontRPosition, Quaternion.Euler(0, 180, 0));
            frontrWheelCollider.name = "FrontR Wheel";
            frontrWheelCollider.parent = kartBody.FindChild("Colliders");
            SetUpWheelCollider(frontrWheelCollider);

            Transform backlWheelCollider = (Transform)Instantiate(gd.wheels[w].model, kartSkel.BackLPosition, Quaternion.Euler(0, 0, 180));
            backlWheelCollider.name = "BackL Wheel";
            backlWheelCollider.parent = kartBody.FindChild("Colliders");
            SetUpWheelCollider(backlWheelCollider);

            Transform backrWheelCollider = (Transform)Instantiate(gd.wheels[w].model, kartSkel.BackRPosition, Quaternion.Euler(0, 180, 180));
            backrWheelCollider.name = "BackR Wheel";
            backrWheelCollider.parent = kartBody.FindChild("Colliders");
            SetUpWheelCollider(backrWheelCollider);

            //Add Kart Script
            kartScript ks = kb.AddComponent<kartScript>();

            ks.engineSound = kartSkel.engineSound;

            ks.wheelColliders.Add(frontlWheelCollider.GetComponent<WheelCollider>());
            ks.wheelColliders.Add(frontrWheelCollider.GetComponent<WheelCollider>());
            ks.wheelColliders.Add(backlWheelCollider.GetComponent<WheelCollider>());
            ks.wheelColliders.Add(backrWheelCollider.GetComponent<WheelCollider>());

            ks.wheelMeshes.Add(frontlWheel);
            ks.wheelMeshes.Add(frontrWheel);
            ks.wheelMeshes.Add(backlWheel);
            ks.wheelMeshes.Add(backrWheel);

            //Sort Particles Out
            Transform kp = kartBody.FindChild("Kart Body").FindChild("Particles");

            ks.flameParticles.Add(kp.FindChild("L_Flame").GetComponent<ParticleSystem>());
            ks.flameParticles.Add(kp.FindChild("R_Flame").GetComponent<ParticleSystem>());

            ks.driftParticles.Add(kp.FindChild("L_Sparks").GetComponent<ParticleSystem>());
            ks.driftParticles.Add(kp.FindChild("R_Sparks").GetComponent<ParticleSystem>());

            ks.trickParticles = kp.FindChild("Trick").GetComponent<ParticleSystem>();

            //Add Other Kart Scripts
            kb.AddComponent<PositionFinding>();
            //kb.AddComponent<kartItem>();
            //kb.GetComponent<kartItem>().itemDistance = kartSkel.ItemDrop;

            //Sort out Character Noises

            //Add Animator Script
            kb.AddComponent<kartAnimator>();
            kb.GetComponent<kartAnimator>().ani = characterMesh.GetComponent<Animator>();

            if(type != KartType.Spectator)
            {
                kb.AddComponent<kartInput>();
                //kb.AddComponent<kartInfo>();
            }
        }

        Destroy(kartSkel);
        Destroy(charSkel);

        kartBody.position = pos;
        kartBody.rotation = rot;

        kartBody.gameObject.layer = 8;//Set the Kart's Layer to "Kart" for Kart Collisions 

        return kartBody.transform;

    }

    void SetUpWheelCollider(Transform collider)
    {
        WheelCollider wheelCollider = collider.gameObject.AddComponent<WheelCollider>();

        wheelCollider.mass = 100f;

        if (collider.GetComponent<Wheel_Skeleton>() != null)
        {
            wheelCollider.radius = collider.GetComponent<Wheel_Skeleton>().wheelRadius;
            Destroy(collider.GetComponent<Wheel_Skeleton>());
        }
        else
            wheelCollider.radius = 0.15f;


        wheelCollider.wheelDampingRate = 0.01f;
        wheelCollider.suspensionDistance = 0.3f;
        wheelCollider.forceAppPointDistance = 1f;

        JointSpring suspensionSpring = new JointSpring();
        suspensionSpring.spring = 100000f;
        suspensionSpring.damper = 5000f;
        suspensionSpring.targetPosition = 0.9f;
        wheelCollider.suspensionSpring = suspensionSpring;

        WheelFrictionCurve forwardFriction = new WheelFrictionCurve();
        forwardFriction.extremumSlip = 0.1f;
        forwardFriction.extremumValue = 1f;
        forwardFriction.asymptoteSlip = 0.8f;
        forwardFriction.asymptoteValue = 0.5f;
        forwardFriction.stiffness = 1;
        wheelCollider.forwardFriction = forwardFriction;

        WheelFrictionCurve sidewaysFriction = new WheelFrictionCurve();
        sidewaysFriction.extremumSlip = 0.8f;
        sidewaysFriction.extremumValue = 3f;
        sidewaysFriction.asymptoteSlip = 1.5f;
        sidewaysFriction.asymptoteValue = 2.25f;
        sidewaysFriction.stiffness = 3;
        wheelCollider.sidewaysFriction = sidewaysFriction;

        Destroy(collider.GetComponent<MeshFilter>());
        Destroy(collider.GetComponent<MeshRenderer>());

    }
}
