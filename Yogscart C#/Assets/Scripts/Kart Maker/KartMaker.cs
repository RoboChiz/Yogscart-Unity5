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

    public Transform SpawnKart(KartType type, Vector3 pos, Quaternion rot, int c, int h, int k, int w)
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
            Transform frontLWheelCollider = new GameObject().transform;
            frontLWheelCollider.position = kartSkel.FrontLPosition;
            frontLWheelCollider.name = "Front L Wheel";
            frontLWheelCollider.parent = kartBody.Find("Colliders");
            SetUpWheelCollider(frontLWheelCollider);

            Transform frontRWheelCollider = new GameObject().transform;
            frontRWheelCollider.name = "Front R Wheel";
            frontRWheelCollider.position = kartSkel.FrontRPosition;
            frontRWheelCollider.parent = kartBody.Find("Colliders");
            SetUpWheelCollider(frontRWheelCollider);

            Transform backLWheelCollider = new GameObject().transform;
            backLWheelCollider.position = kartSkel.BackLPosition;
            backLWheelCollider.name = "Back L Wheel";
            backLWheelCollider.parent = kartBody.Find("Colliders");
            SetUpWheelCollider(backLWheelCollider);

            Transform backRWheelCollider = new GameObject().transform;
            backRWheelCollider.position = kartSkel.BackRPosition;
            backRWheelCollider.name = "Back R Wheel";
            backRWheelCollider.parent = kartBody.Find("Colliders");
            SetUpWheelCollider(backRWheelCollider);

            //Add Kart Script
            KartMovement km = kb.AddComponent<KartMovement>();

            km.characterID = c;
            km.engineSound = kartSkel.engineSound;

            //Spawn Particles
            Transform particlesPack = Instantiate(Resources.Load<Transform>("Prefabs/Kart Maker/Bodies/Particles"), actualKartBody.position, actualKartBody.rotation, actualKartBody);

            km.particleSystems = new Dictionary<string, ParticleSystem>();

            string[] particleNames = new string[] { "Death Particles", "L_Flame", "R_Flame", "L_Sparks", "R_Sparks", "Trick", "L_DriftClouds", "R_DriftClouds", "L_StartClouds", "R_StartClouds" };
            int count = 0;
            foreach (string particle in particleNames)
            {
                //Add Particle System to dictioanry of kart's particles
                Transform currentparticle = particlesPack.Find(particle);
                km.particleSystems.Add(particle, currentparticle.GetComponent<ParticleSystem>());

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
            kb.GetComponent<DeathCatch>().deathParticles = km.particleSystems["Death Particles"];

            kb.AddComponent<CowTipping>();

            //Add Other Kart Scripts
            kb.AddComponent<PositionFinding>();
            //Sort out Character Noises

            //Add Animator Script
            kb.AddComponent<kartAnimator>();
            kb.GetComponent<kartAnimator>().ani = characterMesh.GetComponent<Animator>();

            if (type != KartType.Spectator)
            {
                kb.AddComponent<KartInput>();
                //kb.AddComponent<kartInfo>();
            }

            kb.AddComponent<KartItem>();
            kb.GetComponent<KartItem>().itemDistance = kartSkel.ItemDrop;

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
        FauxCollider wheelCollider = collider.gameObject.AddComponent<FauxCollider>();

        wheelCollider.springCoefficent = 40f;
        wheelCollider.dampeningCoefficent = 7f;

        SphereCollider sphere = collider.gameObject.AddComponent<SphereCollider>();
        sphere.radius = 0.06f;
    }
}