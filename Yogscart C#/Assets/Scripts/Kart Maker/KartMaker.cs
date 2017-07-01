using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

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
        Transform kartBody = Instantiate(gd.karts[k].model, Vector3.zero, Quaternion.identity);
        Transform actualKartBody = kartBody.Find("Kart Body"); ;

        Kart_Skeleton kartSkel = kartBody.GetComponent<Kart_Skeleton>();

        Transform frontlWheel, frontrWheel, backlWheel, backrWheel;

        frontlWheel = Instantiate(gd.wheels[w].model, kartSkel.FrontLPosition, Quaternion.Euler(0, 0, 0));
        frontlWheel.parent = actualKartBody;
        frontlWheel.name = "FrontL Wheel";

        frontrWheel = Instantiate(gd.wheels[w].model, kartSkel.FrontRPosition, Quaternion.Euler(0, 180, 0));
        frontrWheel.parent = actualKartBody;
        frontrWheel.name = "FrontR Wheel";

        backlWheel = Instantiate(gd.wheels[w].model, kartSkel.BackLPosition, Quaternion.Euler(0, 0, 0));
        backlWheel.parent = actualKartBody;
        backlWheel.name = "BackL Wheel";

        backrWheel = Instantiate(gd.wheels[w].model, kartSkel.BackRPosition, Quaternion.Euler(0, 180, 0));
        backrWheel.parent = actualKartBody;
        backrWheel.name = "BackR Wheel";

        //Spawn Character and Hat
        Transform characterMesh = Instantiate(gd.characters[c].model, Vector3.zero, Quaternion.identity);
        characterMesh.name = "Character";

        Character_Skeleton charSkel = characterMesh.GetComponent<Character_Skeleton>();
        characterMesh.position = kartSkel.SeatPosition - charSkel.SeatPosition;
        characterMesh.parent = actualKartBody;

        if (h != 0)//Don't spawn a hat if hat value equals zero
        {
            Transform hatMesh = Instantiate(gd.hats[h].model, charSkel.HatHolder.position, Quaternion.identity);
            hatMesh.parent = charSkel.HatHolder;
            hatMesh.localRotation = Quaternion.Euler(0, 0, 0);
        }

        //Setup IK Animation
        DrivingIK ikComponent = characterMesh.gameObject.AddComponent<DrivingIK>();
        ikComponent.leftHandTarget = kartSkel.leftHandTarget;
        ikComponent.rightHandTarget = kartSkel.rightHandTarget;
        ikComponent.leftFootTarget = kartSkel.leftFootTarget;
        ikComponent.rightFootTarget = kartSkel.rightFootTarget;
        ikComponent.steeringWheel = actualKartBody.Find("Steering Wheel");

        if (type == KartType.Online)
        {
            kartBody.gameObject.AddComponent<NetworkIdentity>();
        }

        if (type != KartType.Display)
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

            AudioSourceInfo ausI = kb.AddComponent<AudioSourceInfo>();
            ausI.idealVolume = 0.05f;
            ausI.audioType = AudioSourceInfo.AudioType.SFX;

            //Set up Kart Body Sounds
            AudioSource kbas = actualKartBody.gameObject.AddComponent<AudioSource>();
            kbas.spatialBlend = 1;
            kbas.minDistance = 0f;
            kbas.maxDistance = 100f;
            kbas.rolloffMode = AudioRolloffMode.Linear;
            kbas.playOnAwake = false;
            kbas.dopplerLevel = 0f;
            kbas.spread = 0f;

            ausI = kbas.gameObject.AddComponent<AudioSourceInfo>();
            ausI.idealVolume = 1f;
            ausI.audioType = AudioSourceInfo.AudioType.SFX;

            //Setup Wheel Colliders
            Transform frontlWheelCollider = (Transform)Instantiate(gd.wheels[w].model, kartSkel.FrontLPosition, Quaternion.Euler(0, 0, 0));
            frontlWheelCollider.name = "FrontL Wheel";
            frontlWheelCollider.parent = kartBody.Find("Colliders");
            SetUpWheelCollider(frontlWheelCollider);

            Transform frontrWheelCollider = (Transform)Instantiate(gd.wheels[w].model, kartSkel.FrontRPosition, Quaternion.Euler(0, 180, 0));
            frontrWheelCollider.name = "FrontR Wheel";
            frontrWheelCollider.parent = kartBody.Find("Colliders");
            SetUpWheelCollider(frontrWheelCollider);

            Transform backlWheelCollider = (Transform)Instantiate(gd.wheels[w].model, kartSkel.BackLPosition, Quaternion.Euler(0, 0, 180));
            backlWheelCollider.name = "BackL Wheel";
            backlWheelCollider.parent = kartBody.Find("Colliders");
            SetUpWheelCollider(backlWheelCollider);

            Transform backrWheelCollider = (Transform)Instantiate(gd.wheels[w].model, kartSkel.BackRPosition, Quaternion.Euler(0, 180, 180));
            backrWheelCollider.name = "BackR Wheel";
            backrWheelCollider.parent = kartBody.Find("Colliders");
            SetUpWheelCollider(backrWheelCollider);

            //Add Kart Script
            KartScript ks = kb.AddComponent<KartScript>();

            ks.characterID = c;
            ks.engineSound = kartSkel.engineSound;

            ks.wheelColliders = new List<WheelCollider>();
            ks.wheelColliders.Add(frontlWheelCollider.GetComponent<WheelCollider>());
            ks.wheelColliders.Add(frontrWheelCollider.GetComponent<WheelCollider>());
            ks.wheelColliders.Add(backlWheelCollider.GetComponent<WheelCollider>());
            ks.wheelColliders.Add(backrWheelCollider.GetComponent<WheelCollider>());

            ks.wheelBackUps = new List<SphereCollider>();
            ks.wheelBackUps.Add(frontlWheelCollider.gameObject.AddComponent<SphereCollider>());
            ks.wheelBackUps.Add(frontrWheelCollider.gameObject.AddComponent<SphereCollider>());
            ks.wheelBackUps.Add(backlWheelCollider.gameObject.AddComponent<SphereCollider>());
            ks.wheelBackUps.Add(backrWheelCollider.gameObject.AddComponent<SphereCollider>());

            //Used to stop wheels going through the ground
            foreach(SphereCollider sc in ks.wheelBackUps)
            {
                sc.radius = frontlWheelCollider.GetComponent<WheelCollider>().radius * 0.75f;
                sc.material = kb.GetComponentInChildren<BoxCollider>().material;
            }

            ks.wheelMeshes = new List<Transform>();
            ks.wheelMeshes.Add(frontlWheel);
            ks.wheelMeshes.Add(frontrWheel);
            ks.wheelMeshes.Add(backlWheel);
            ks.wheelMeshes.Add(backrWheel);

            //Spawn Particles
            Transform particlesPack = Instantiate(Resources.Load<Transform>("Prefabs/Kart Maker/Bodies/Particles"), actualKartBody.position, actualKartBody.rotation, actualKartBody);

            ks.particleSystems = new Dictionary<string, ParticleSystem>();

            string[] particleNames = new string[] { "Death Particles", "L_Flame", "R_Flame", "L_Sparks", "R_Sparks", "Trick", "L_DriftClouds", "R_DriftClouds", "L_StartClouds", "R_StartClouds" };
            int count = 0;
            foreach (string particle in particleNames)
            {
                //Add Particle System to dictioanry of kart's particles
                Transform currentparticle = particlesPack.Find(particle);
                ks.particleSystems.Add(particle, currentparticle.GetComponent<ParticleSystem>());

                switch (count)
                {
                    case 0: currentparticle.position = kartSkel.deathParticlesPos; break;
                    case 1: currentparticle.position = kartSkel.rFlame; break;
                    case 2: currentparticle.position = kartSkel.lFlame; break;
                    case 3: currentparticle.position = kartSkel.rSparks; break;
                    case 4: currentparticle.position = kartSkel.lSparks; break;
                    case 5: currentparticle.position = kartSkel.trick; break;
                    case 6: currentparticle.position = kartSkel.lDriftClouds; break;
                    case 7: currentparticle.position = kartSkel.rDriftClouds; break;
                    case 8: currentparticle.position = kartSkel.lStartClouds; break;
                    case 9: currentparticle.position = kartSkel.rStartClouds; break;
                }

                count++;
            }

            //Add Death Catch
            kb.AddComponent<DeathCatch>();
            kb.GetComponent<DeathCatch>().deathParticles = ks.particleSystems["Death Particles"];

            kb.AddComponent<CowTipping>();

            //Add Other Kart Scripts
            kb.AddComponent<PositionFinding>();
            //Sort out Character Noises

            //Add Animator Script
            kb.AddComponent<kartAnimator>();
            kb.GetComponent<kartAnimator>().ani = characterMesh.GetComponent<Animator>();

            if(type != KartType.Spectator)
            {
                kb.AddComponent<kartInput>();
                //kb.AddComponent<kartInfo>();
            }

            kb.AddComponent<kartItem>();
            kb.GetComponent<kartItem>().itemDistance = kartSkel.ItemDrop;

            kb.AddComponent<KartCollider>();
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
        WheelCollider masterWheelCollider = Resources.Load<Transform>("Prefabs/Kart Maker/Wheels/Wheel Collider Base").GetComponent<WheelCollider>();

        wheelCollider.mass = masterWheelCollider.mass;

        if (collider.GetComponent<Wheel_Skeleton>() != null)
        {
            wheelCollider.radius = collider.GetComponent<Wheel_Skeleton>().wheelRadius;
            Destroy(collider.GetComponent<Wheel_Skeleton>());
        }
        else
            wheelCollider.radius = 0.2f;


        wheelCollider.wheelDampingRate = masterWheelCollider.wheelDampingRate;
        wheelCollider.suspensionDistance = masterWheelCollider.suspensionDistance;
        wheelCollider.forceAppPointDistance = masterWheelCollider.forceAppPointDistance;

        JointSpring suspensionSpring = new JointSpring();
        suspensionSpring.spring = masterWheelCollider.suspensionSpring.spring;
        suspensionSpring.damper = masterWheelCollider.suspensionSpring.damper;
        suspensionSpring.targetPosition = masterWheelCollider.suspensionSpring.targetPosition;
        wheelCollider.suspensionSpring = suspensionSpring;

        WheelFrictionCurve forwardFriction = new WheelFrictionCurve();
        forwardFriction.extremumSlip = masterWheelCollider.forwardFriction.extremumSlip;
        forwardFriction.extremumValue = masterWheelCollider.forwardFriction.extremumValue;
        forwardFriction.asymptoteSlip = masterWheelCollider.forwardFriction.asymptoteSlip;
        forwardFriction.asymptoteValue = masterWheelCollider.forwardFriction.asymptoteValue;
        forwardFriction.stiffness = masterWheelCollider.forwardFriction.stiffness;
        wheelCollider.forwardFriction = forwardFriction;

        WheelFrictionCurve sidewaysFriction = new WheelFrictionCurve();
        sidewaysFriction.extremumSlip = masterWheelCollider.sidewaysFriction.extremumSlip; ;
        sidewaysFriction.extremumValue = masterWheelCollider.sidewaysFriction.extremumValue; ;
        sidewaysFriction.asymptoteSlip = masterWheelCollider.sidewaysFriction.asymptoteSlip; ;
        sidewaysFriction.asymptoteValue = masterWheelCollider.sidewaysFriction.asymptoteValue;
        sidewaysFriction.stiffness = masterWheelCollider.sidewaysFriction.stiffness;
        wheelCollider.sidewaysFriction = sidewaysFriction;

        Destroy(collider.GetComponent<MeshFilter>());
        Destroy(collider.GetComponent<MeshRenderer>());

    }
}
