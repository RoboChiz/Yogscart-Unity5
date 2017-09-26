using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

[RequireComponent(typeof(Rigidbody))]
public class KartMovement : MonoBehaviour
{
    //Inputs
    public float throttle, steer;
    public bool drift;

    //Kart Variables
    public bool locked = true;
    public bool isFalling = false;

    //Speed Controls
    public float speedModifier = 1f; //Used to make cars faster for harder difficultes

    //To be made const after testing
    const float maxSpeedVal = 20f, maxGrassSpeedVal = 10f, maxBoostSpeedVal = 28f, accelerationVal = 10f, brakeTimeVal = 0.5f, turnAcceleration = 4f, maxTurnSpeed = 1f, turnStop = 10f;
    public const float blueTime = 1.5f, orangeTime = 3f;
    private const float spinTime = 0.5f, spunTime = 1.5f;

    //Visual Constants
    const float upOffset = 0.2f, forwardOffset = 0.5f;
    const float wheelRotateSpeed = 3f, kartBodySlide = 5f;

    //Shows current speed values
    public float expectedSpeed = 0f;
    public float actualSpeed { get; private set; }
    public int lapisAmount = 0;

    //Controls how turning is affected at different speeds
    public AnimationCurve speedAffectCurve = new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.1f, 0.1f), new Keyframe(0.5f, 1f), new Keyframe(1, 0.75f));

    //Drifting 
    private const float driftTurn = 1.15f, intoRange = 0.6f, outOfRange = -0.5f, driftTimeAddTurnIn = 2f, driftTimeAddTurnOut = 0.7f;
    public enum DriftMode { Not, Drifting };
    public DriftMode isDrifting = DriftMode.Not;
    public int driftSteer = 0; //What direction we are drifting
    private float driftTime = 0f; //How long have we been drifting

    //Boosting
    Coroutine lastBoost;

    //Skid Marks
    private static Transform skidMarkTransform;
    private LineRenderer[] currentLineRenderer;
    private float skidTimer;

    //Kart Spinning
    private bool spinning = false, spunOut = false;

    [HideInInspector]
    public bool spinningOut = false;

    //Tricking off Ramps
    [HideInInspector]
    public bool tricking;
    private bool trickPotential, trickLock;

    [HideInInspector]
    public bool onlineMode = false;

    //What state of boost we are in
    public enum BoostMode { Not, Boost, DriftBoost, Trick };
    private BoostMode isBoosting = BoostMode.Not;

    //Values after modifing
    private float maxSpeed, maxGrassSpeed, maxBoostSpeed, acceleration, brakeTime;

    //Values gotten from other objects
    private KartWheel[] wheels;

    private bool offRoad;
    private Rigidbody kartRigidbody;

    //Store Systems as arrays for convinence
    public Transform kartBody { get; private set; }
    ParticleSystem[] startCloudParticles, flameParticles, driftParticles, driftCloudParticles;
    ParticleSystem trickParticles;

    //Boost at Start
    private float startBoostAmount, wheelSpinExtra, wheelSpinPercent;
    public static int startBoostVal = -1;
    private int lastStartBoostVal = -1;
    public static bool raceStarted = false, beQuiet = true;
    private bool spinOut = false, allowedStartBoost;

    //Particles
    public Dictionary<string, ParticleSystem> particleSystems;

    //Audio Source
    private AudioSourceInfo audioSourceInfo;
    private AudioSource kartAudioSource, myAudioSource;
    public AudioClip engineSound;
    public float quietTimer; //Used to make engine quiter after a couple of seconds

    [System.NonSerialized]
    private CharacterSoundPack soundPack;

    //Used for Character Specific Taunts
    public int characterID, hatID;

    private Coroutine kartBodySliding = null;

    private kartInfo kartInfoComp;
    private float chroming = 0f;

    public List<Camera> toProcess;

    void Awake()
    {
        //Get Kart Body
        kartBody = transform.Find("Kart Body");
        toProcess = new List<Camera>();
    }

    void Start()
    {
        kartRigidbody = GetComponent<Rigidbody>();
        kartInfoComp = GetComponent<kartInfo>();

        SetupKart();
    }

    /// <summary>
    /// Sets up the kart, meaning you have one frame to spawn everything it needs
    /// </summary>
    public void SetupKart()
    {
        //Get Wheels
        FauxCollider[] wheelColliders = GetComponentsInChildren<FauxCollider>();

        //Get Wheel Meshes
        Wheel_Skeleton[] wheelSkeletons = GetComponentsInChildren<Wheel_Skeleton>();

        if (wheelColliders.Length != wheelSkeletons.Length)
            throw new System.Exception("There are not the same amount of wheels and colliders");

        int wheelLength = wheelSkeletons.Length;
        wheels = new KartWheel[wheelLength];

        for (int i = 0; i < wheelLength; i++)
        {
            //Create a new Kart Wheel
            wheels[i] = new KartWheel(wheelColliders[i], wheelSkeletons[i]);

            //Set the Faux Wheels suspension to be the wheels radius plus a bit
            wheels[i].collider.suspensionDistance = 0.15f + wheels[i].radius;
        }

        //Get Kart Body
        kartBody = transform.Find("Kart Body");

        //Get Particles
        //Debug Delete later!
        try
        {
            startCloudParticles = new ParticleSystem[] { particleSystems["L_StartClouds"], particleSystems["R_StartClouds"] };
            flameParticles = new ParticleSystem[] { particleSystems["L_Flame"], particleSystems["R_Flame"] };
            driftParticles = new ParticleSystem[] { particleSystems["L_Sparks"], particleSystems["R_Sparks"] };
            driftCloudParticles = new ParticleSystem[] { particleSystems["L_DriftClouds"], particleSystems["R_DriftClouds"] };
            trickParticles = particleSystems["Trick"];
        }
        catch
        {
            Transform particle = transform.Find("Kart Body").Find("Particles");

            startCloudParticles = new ParticleSystem[] { particle.Find("L_StartClouds").GetComponent<ParticleSystem>(), particle.Find("R_StartClouds").GetComponent<ParticleSystem>() };
            flameParticles = new ParticleSystem[] { particle.Find("L_Flame").GetComponent<ParticleSystem>(), particle.Find("R_Flame").GetComponent<ParticleSystem>() };
            driftParticles = new ParticleSystem[] { particle.Find("L_Sparks").GetComponent<ParticleSystem>(), particle.Find("R_Sparks").GetComponent<ParticleSystem>() };
            driftCloudParticles = new ParticleSystem[] { particle.Find("L_DriftClouds").GetComponent<ParticleSystem>(), particle.Find("R_DriftClouds").GetComponent<ParticleSystem>() };
            trickParticles = particle.Find("Trick").GetComponent<ParticleSystem>();
        }

        //Get Skid Marks
        if (skidMarkTransform == null)
            skidMarkTransform = Resources.Load<Transform>("Prefabs/SkidMarks");

        //Setup Audio Stuff
        audioSourceInfo = GetComponent<AudioSourceInfo>();
        myAudioSource = GetComponent<AudioSource>();
        kartAudioSource = transform.Find("Kart Body").GetComponent<AudioSource>();

        //Load Custom Audio Packs for Characters
        soundPack = FindObjectOfType<CurrentGameData>().GetCustomSoundPack(characterID, hatID);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Time.timeScale != 0)
        {
            //Get Multiplier versions
            maxSpeed = maxSpeedVal * speedModifier;
            maxGrassSpeed = maxGrassSpeedVal * speedModifier;
            maxBoostSpeed = maxBoostSpeedVal * speedModifier;
            acceleration = accelerationVal * speedModifier;
            brakeTime = brakeTimeVal * speedModifier;

            //Add Boost
            if (isBoosting != BoostMode.Not)
            {
                maxSpeed = maxBoostSpeed;
                expectedSpeed = maxBoostSpeed;
            }

            //Get Rigidbody Stuff
            Vector3 relativeVelocity = transform.InverseTransformDirection(kartRigidbody.velocity);
            actualSpeed = relativeVelocity.z;

            //Lower Centre of Mass to stop flipping
            kartRigidbody.centerOfMass = Vector3.down * 2f;

            //Check if we are falling or offroad, and get floo
            offRoad = false;
            isFalling = true;

            Vector3 averagePlaneNormal = Vector3.zero;
            int normalCount = 0;

            foreach (KartWheel wheel in wheels)
            {
                if (wheel.collider.surfaceImpactTag == "OffRoad")
                    offRoad = true;

                if (wheel.collider.groundHit)
                {
                    averagePlaneNormal += wheel.collider.surfaceImpactNormal;
                    normalCount++;
                    isFalling = false;
                }
            }

            averagePlaneNormal /= (float)normalCount;

            //Get speed to travel forward at
            if (!isFalling)
                CalculateExpectedSpeed();

            //Make the vehicle turn
            float finalSteer = DoSteering();

            //Get Forward direction from road plane
            Vector3 forwardDir = Vector3.ProjectOnPlane(transform.forward, averagePlaneNormal).normalized;

            if (finalSteer != 0f)
                kartRigidbody.AddTorque(transform.up * finalSteer, ForceMode.Acceleration);
            else //Apply a Torque to stop rotation
                kartRigidbody.AddTorque((-kartRigidbody.angularVelocity / turnStop) / Time.fixedDeltaTime, ForceMode.Acceleration);

            //Apply sideways force to stop slip
            kartRigidbody.AddForce(-transform.right * relativeVelocity.x, ForceMode.VelocityChange);

            //Apply forward force
            if (!isFalling)
            {
                float nA = (expectedSpeed - actualSpeed) / Time.fixedDeltaTime;
                kartRigidbody.AddForce(forwardDir * nA, ForceMode.Acceleration);
                //kartRigidbody.AddForceAtPosition(forwardDir * nA, transform.position + kartRigidbody.centerOfMass - (transform.up * upOffset) + (transform.forward * forwardOffset), ForceMode.Acceleration);
            }

            ApplyDrift();
            DoTrick();

            //Make Kart Level out
            if(isFalling)
            {
                float angle = MathHelper.Angle(transform.up, Vector3.up);

                if (Mathf.Abs(angle) > 12f)
                {
                    Vector3 x = Vector3.Cross(transform.up, Vector3.up);
                    float theta = Mathf.Asin(x.magnitude);
                    Vector3 w = x.normalized * theta / Time.fixedDeltaTime;

                    Quaternion q = transform.rotation * kartRigidbody.inertiaTensorRotation;
                    Vector3 T = q * Vector3.Scale(kartRigidbody.inertiaTensor, (Quaternion.Inverse(q) * w));

                    kartRigidbody.AddTorque(T / 3f, ForceMode.Impulse);
                }
            }

            //Stop Kart from drivng into walls
            RaycastHit hit;
            string[] ignoreTags = new string[] { "OffRoad", "Ground", "Kart", "Crate", "PowerUp" };

            int layerMask = ~((1 << 8) | (1 << 9) | (1 << 10) | (1 << 11));

            for (int i = -1; i <= 1; i += 2)
            {
                Debug.DrawRay(transform.position, transform.forward * (2f * i), Color.red);
                if (Physics.Raycast(transform.position, transform.forward * i, out hit, 2f, layerMask) && hit.transform.GetComponent<Collider>() != null && !hit.transform.GetComponent<Collider>().isTrigger)
                {
                    bool ignore = false;
                    foreach (string tag in ignoreTags)
                    {
                        if (hit.transform.tag == tag)
                        {
                            ignore = true;
                            break;
                        }
                    }

                    if (!ignore)
                    {
                        expectedSpeed = -i * 5f;
                    }
                }
            }
        }
    }

    void Update()
    {
        lapisAmount = (int)Mathf.Clamp(lapisAmount, 0f, 10f);

        if (Time.timeScale != 0)
        {
            //Wheel Logic
            for (int i = 0; i < wheels.Length; i++)
            {
                KartWheel wheel = wheels[i];

                //Make Wheel Meshes Stick to Ground
                if (wheel.collider.groundHit)
                {
                    Vector3 hitPoint = wheel.collider.surfaceImpactPoint;
                    Vector3 toLocal = wheel.transform.parent.InverseTransformPoint(hitPoint);

                    wheel.targetPos.y = toLocal.y + wheel.radius;
                }

                Vector3 euler = Vector3.zero;

                //Make Wheel steer into steer direction
                if (i < 2)
                {
                    if (wheel.wheelAngle > steer)
                        wheel.wheelAngle = Mathf.Clamp(wheel.wheelAngle - (Time.deltaTime * wheelRotateSpeed), steer, wheel.wheelAngle);
                    else if (wheel.wheelAngle < steer)
                        wheel.wheelAngle = Mathf.Clamp(wheel.wheelAngle + (Time.deltaTime * wheelRotateSpeed), wheel.wheelAngle, steer);

                    euler.y = wheel.wheelAngle * 15f;
                }

                //Make Wheel spin forward
                wheel.wheelSpin = MathHelper.NumClamp(wheel.wheelSpin + (Time.deltaTime * (actualSpeed * 15f)), 0f, 360f);
                euler.x = wheel.wheelSpin;

                if (i % 2 == 0)
                    wheel.transform.localRotation = Quaternion.Euler(euler);
                else
                    wheel.transform.localRotation = Quaternion.Euler(euler) * Quaternion.Euler(0, 180, 0);

                //Move the wheel to it's target local
                wheel.transform.localPosition = Vector3.Lerp(wheel.transform.localPosition, wheel.targetPos, Time.deltaTime * 6f);
            }


            //Boost Particles
            foreach (ParticleSystem flame in flameParticles)
            {
                if (isBoosting == BoostMode.Not)
                {
                    if (flame.isPlaying)
                        flame.Stop();
                }
                else
                {
                    if (!flame.isPlaying)
                        flame.Play();
                }
            }

            //Calculate Start Boost
            if (!onlineMode && startBoostVal >= 0)
            {
                bool throttleOver = (throttle > 0f);

                if (startBoostVal == 3 && throttleOver)
                    spinOut = true;
                else if (startBoostVal == 2 && throttleOver && !spinOut)
                    allowedStartBoost = true;
                else if (startBoostVal < 2 && startBoostVal != 0 && throttle == 0)
                    allowedStartBoost = false;

                if (allowedStartBoost && throttleOver)
                    startBoostAmount += Time.deltaTime * 0.1f;

                if (startBoostVal == 0 && spinOut)
                    SpinOut();
                else if (startBoostVal == 0 && allowedStartBoost && startBoostAmount > 0)
                {
                    Boost(startBoostAmount, BoostMode.Trick);
                    allowedStartBoost = false;
                }

                if (startBoostVal == 0)
                    raceStarted = true;
            }

            if(startBoostVal != lastStartBoostVal)
            {
                gameObject.BroadcastMessage("StartBoostVal", startBoostVal, SendMessageOptions.DontRequireReceiver);
                lastStartBoostVal = startBoostVal;
            }

            //Play engine Audio
            if (engineSound != null && !beQuiet)
            {
                if (!myAudioSource.isPlaying)
                {
                    myAudioSource.clip = engineSound;
                    myAudioSource.Play();
                    myAudioSource.loop = true;
                }

                if (raceStarted)
                {
                    float percent = expectedSpeed / maxSpeed;
                    float normalVolume = Mathf.Lerp(0.1f, 0.25f, percent), normalPitch = Mathf.Lerp(0.75f, 1.5f, percent);
                    float quieterVolume = 0.15f;

                    //Make quiet timer increase 
                    if (percent > 0.75f)
                        quietTimer += Time.deltaTime;
                    else
                        quietTimer = 0f;

                    float currentVolume = normalVolume;
                    //After five seconds of max throttle make the car quieter
                    if (quietTimer > 5f)
                        currentVolume = quieterVolume;

                    audioSourceInfo.idealVolume = Mathf.Lerp(audioSourceInfo.idealVolume, currentVolume, Time.deltaTime * 3f);
                    myAudioSource.pitch = normalPitch;

                }
                else
                {
                    float percent = wheelSpinPercent / 15f;
                    audioSourceInfo.idealVolume = Mathf.Lerp(0.1f, 0.4f, percent);
                    myAudioSource.pitch = Mathf.Lerp(0.75f, 1.5f, percent);
                }
            }
            else
            {
                audioSourceInfo.idealVolume = 0f;
            }

            //Make Smoke from wheels speed up and slow down
            if (!raceStarted)
            {
                if (throttle > 0)
                    wheelSpinPercent = Mathf.Lerp(wheelSpinPercent, 15f, Time.deltaTime * 2f);
                else
                    wheelSpinPercent = Mathf.Lerp(wheelSpinPercent, 0f, Time.deltaTime * 4f);

                foreach (ParticleSystem ps in startCloudParticles)
                {
                    if (!ps.isPlaying)
                        ps.Play();

                    ParticleSystem.EmissionModule emission = ps.emission;
                    emission.rateOverTimeMultiplier = wheelSpinPercent;
                }

                //Spin wheels forward during start
                wheelSpinExtra = Time.deltaTime * wheelSpinPercent * 20f;

                foreach (KartWheel wheel in wheels)
                    wheel.wheelSpin = MathHelper.NumClamp(wheel.wheelSpin + wheelSpinExtra, 0f, 360f);
            }
            else
            {
                foreach (ParticleSystem ps in startCloudParticles)
                {
                    if (ps.isPlaying)
                        ps.Stop();
                }
            }

            if (toProcess != null)
            {
                if (isBoosting != BoostMode.Not)
                {
                    chroming = Mathf.Clamp(chroming + (Time.deltaTime * 2.5f), 0f, 1f);
                }
                else
                {
                    chroming = Mathf.Clamp(chroming - (Time.deltaTime * 2.5f), 0f, 1f);
                }

                //Do cool Chromatic Aberration Effect on boost
                if (FindObjectOfType<EffectsManager>().GetUseChromaticAberration())
                {
                    foreach (Camera camera in toProcess)
                    {
                        PostProcessingBehaviour postProcess = camera.GetComponent<PostProcessingBehaviour>();

                        if (postProcess != null)
                        {
                            ChromaticAberrationModel.Settings cab = postProcess.profile.chromaticAberration.settings;
                            cab.intensity = chroming;
                            postProcess.profile.chromaticAberration.settings = cab;
                        }
                    }
                }
            }

        }
        else //Make the Kart quiet
        {
            audioSourceInfo.idealVolume = 0f;
        }
    }

    /// <summary>
    /// Calculates the new speed we want to travel at
    /// </summary>
    void CalculateExpectedSpeed()
    {
        if (throttle == 0 || locked)
        {
            float cacceleration = -expectedSpeed / brakeTime;
            expectedSpeed += (cacceleration * Time.fixedDeltaTime);

            if (Mathf.Abs(expectedSpeed) <= 0.1f)
                expectedSpeed = 0;
        }
        else
        {
            if (MathHelper.HaveTheSameSign(throttle, expectedSpeed) == false)
                expectedSpeed += (throttle * acceleration * 2) * Time.fixedDeltaTime;
            else
            {
                float percentage;

                if (offRoad && isBoosting != BoostMode.Boost)
                    percentage = (1f / maxGrassSpeed) * Mathf.Abs(expectedSpeed);
                else
                    percentage = (1f / maxSpeed) * Mathf.Abs(expectedSpeed);

                expectedSpeed += (throttle * acceleration * (1f - percentage)) * Time.fixedDeltaTime;
            }
        }
    }

    /// <summary>
    /// Calculates the final steer amount
    /// </summary>
    /// <returns></returns>
    private float DoSteering()
    {
        float finalSteer = 0f;

        if (isDrifting == DriftMode.Not)
        {
            finalSteer = (float)MathHelper.Sign(steer) * MathHelper.Sign(actualSpeed);

            //Stop Torque from spinning crazily
            kartRigidbody.maxAngularVelocity = maxTurnSpeed;
        }
        else
        {
            finalSteer = (float)MathHelper.Sign(driftSteer) * MathHelper.Sign(actualSpeed);
            //Make turn more tight
            finalSteer *= driftTurn;

            float driftAdjust = 0;

            //Steering into a turn is more useful then steering away from it
            float steerSign = MathHelper.Sign(steer), driftSteerSign = MathHelper.Sign(driftSteer);

            if (driftSteerSign == steerSign)
                driftAdjust += intoRange * driftSteerSign;
            else if (driftSteerSign == -steerSign)
                driftAdjust += outOfRange * driftSteerSign;

            finalSteer += driftAdjust;

            kartRigidbody.maxAngularVelocity = maxTurnSpeed * finalSteer;
        }


        finalSteer *= turnAcceleration;
        finalSteer *= speedAffectCurve.Evaluate(Mathf.Abs(expectedSpeed / maxSpeed));

        if (isFalling)
            finalSteer = 0;

        return finalSteer;
    }

    /// <summary>
    /// Does drifting logic
    /// </summary>
    void ApplyDrift()
    {
        //Set to Input is we have a valid input
        bool inputValid = drift && expectedSpeed > maxSpeed * 0.5f;

        //If we can start a drift
        if (inputValid && !isFalling && (!offRoad || (offRoad && isBoosting == BoostMode.Boost)))
        {
            if (isDrifting == DriftMode.Not && Mathf.Abs(steer) > 0.2)
            {
                isDrifting = DriftMode.Drifting;
                driftSteer = (int)Mathf.Sign(steer);
            }
        }
        else if (isDrifting != DriftMode.Not)
        {
            isDrifting = DriftMode.Not;
        }

        if (isDrifting == DriftMode.Drifting)
        {
            //Increment Drift Time
            driftTime += Time.fixedDeltaTime * Mathf.Lerp(driftTimeAddTurnOut, driftTimeAddTurnIn, Mathf.Abs((driftSteer + steer))/2f);

            //Rotate the kart to face the drift direction
            kartBody.localRotation = Quaternion.Slerp(kartBody.localRotation, Quaternion.Euler(0, kartBodySlide * driftSteer, 0), Time.fixedDeltaTime * 2);

            //Make Drift Particles change Colour
            foreach (ParticleSystem driftParticle in driftParticles)
            {
                if (driftTime > orangeTime)
                    driftParticle.GetComponent<Renderer>().material = Resources.Load<Material>("Particles/Drift Particles/Spark_Orange");
                else if (driftTime > blueTime)
                {
                    driftParticle.GetComponent<Renderer>().material = Resources.Load<Material>("Particles/Drift Particles/Spark_Blue");

                    if (!driftParticle.isPlaying)
                        driftParticle.Play();
                }
            }

            //Make Cloud Particles Start
            foreach (ParticleSystem cloudParticle in driftCloudParticles)
            {
                if (!cloudParticle.isPlaying)
                    cloudParticle.Play();
            }

            //Spawn Line Renderer and Do Drift
            if (currentLineRenderer == null)
            {
                //Create New Arrray
                currentLineRenderer = new LineRenderer[2];

                //Create Object for each wheel
                for (int i = 0; i < 2; i++)
                {
                    Transform newLineRenderer = Instantiate(skidMarkTransform, Vector3.zero, Quaternion.Euler(90f, 0f, 0f));
                    currentLineRenderer[i] = newLineRenderer.GetComponent<LineRenderer>();
                }
            }
            else
            {
                //Reduce Skid Timer
                skidTimer -= Time.deltaTime;

                //Add new point to Skid every 0.2 seconds
                if (skidTimer <= 0f)
                {
                    for (int i = 0; i < 2f; i++)
                        currentLineRenderer[i].positionCount = currentLineRenderer[i].positionCount + 1;

                    skidTimer = 0.2f;
                }

                //Update position of late Point
                for (int i = 0; i < 2; i++)
                {
                    KartWheel wheel = wheels[2 + i];
                    bool onGround = wheels[2 + i].collider.groundHit;

                    if (onGround)
                    {
                        Vector3 hitPoint = wheel.collider.surfaceImpactPoint;
                        currentLineRenderer[i].SetPosition(currentLineRenderer[i].positionCount - 1, hitPoint + new Vector3(0f, 0.1f, 0f));
                    }
                }
            }
        }
        else
        {
            //Reset Skid Marks
            if (currentLineRenderer != null)
            {
                foreach (LineRenderer lr in currentLineRenderer)
                {
                    if (lr.positionCount == 0)
                        DestroyImmediate(lr.gameObject);
                }

                currentLineRenderer = null;
                skidTimer = 0f;
            }

            if (!spinning)
                kartBody.localRotation = Quaternion.Slerp(kartBody.localRotation, Quaternion.Euler(0, 0, 0), Time.fixedDeltaTime);

            if (throttle > 0)
            {
                if (driftTime >= orangeTime)
                {
                    if (!onlineMode)
                        Boost(1f, BoostMode.DriftBoost);
                }
                else if (driftTime >= blueTime)
                {
                    if (!onlineMode)
                        Boost(0.5f, BoostMode.DriftBoost);
                }
            }

            driftTime = 0f;
            driftSteer = 0;

            for (int j = 0; j < 2; j++)
            {
                driftParticles[j].Stop();
                driftCloudParticles[j].Stop();
            }
        }
    }

    //Allows Kart to do Tricks
    void DoTrick()
    {
        if (isFalling || locked)
        {
            if (!Physics.Raycast(transform.position, -transform.up, 1.5f) && trickPotential)
            {
                tricking = true;
                trickPotential = false;
                StartCoroutine("SpinKartBody", Vector3.right);
                trickParticles.Play();
            }
        }
        else
        {
            if (!trickPotential && drift && !trickLock)
            {
                trickPotential = true;
                trickLock = true;
                StartCoroutine("CancelTrickPotential");
            }

            if (tricking)
            {
                if (!onlineMode)
                    Boost(0.25f, BoostMode.Trick);

                tricking = false;
            }
        }

        if (drift == false)
            trickLock = false;
    }

    public void Boost(float t, BoostMode type)
    {
        bool playAudio = true;
        if (isBoosting != BoostMode.Not)
            playAudio = false;

        isBoosting = type;

        if (lastBoost != null)
            StopCoroutine(lastBoost);

        lastBoost = StartCoroutine(StartBoost(t, playAudio));

        //If Kart is networked send this information to server and clients
        if (GetComponent<KartNetworker>() != null && !onlineMode)
            GetComponent<KartNetworker>().SendBoost(t, (KartMovement.BoostMode)type);
    }

    IEnumerator StartBoost(float t, bool playAudio)
    {
        if (playAudio)
        {
            AudioClip BoostSound = Resources.Load<AudioClip>("Music & Sounds/SFX/boost");
            myAudioSource.PlayOneShot(BoostSound, 1.5f);
        }

        yield return new WaitForSeconds(t);

        isBoosting = BoostMode.Not;
    }

    void CancelBoost()
    {
        StopCoroutine("Boost");
        isBoosting = BoostMode.Not;
    }

    IEnumerator CancelTrickPotential()
    {
        yield return new WaitForSeconds(0.5f);
        trickPotential = false;
    }

    IEnumerator SpinKartBody(Vector3 dir)
    {
        spinning = true;
        float startTime = Time.time;

        float time = 0;

        if (dir == Vector3.up)
            time = spunTime;
        else
            time = spinTime;

        while (Time.time - startTime < time)
        {
            kartBody.Rotate((dir * 360f * Time.deltaTime) / time);
            yield return null;
        }

        spinning = false;
    }

    public void SpinOut(bool doNoise)
    {
        if (!onlineMode)
        {
            StartCoroutine("StartSpinOut");

            //If Kart is networked send this information to server and clients
            if (GetComponent<KartNetworker>() != null)
                GetComponent<KartNetworker>().spinOut++;

            if (doNoise)
            {
                if (soundPack.hitSounds != null && soundPack.hitSounds.Length > 0)
                    kartAudioSource.PlayOneShot(soundPack.hitSounds[Random.Range(0, soundPack.hitSounds.Length)]);
            }

            spinningOut = true;
        }
    }
    public void SpinOut() { SpinOut(false); }

    public void DoTaunt()
    {
        //Play Sound
        if (soundPack.tauntSounds != null && soundPack.tauntSounds.Length > 0)
            kartAudioSource.PlayOneShot(soundPack.tauntSounds[Random.Range(0, soundPack.tauntSounds.Length)]);

        //Do Animation
    }

    //For use by Kart Networker as Kart will not spin locally otherwise
    public void localSpinOut()
    {
        StartCoroutine("StartSpinOut");
    }

    IEnumerator StartSpinOut()
    {
        if (!spunOut)
        {
            spunOut = true;
            CancelBoost();
            locked = true;

            //Play Sound
            Animator ani = kartBody.Find("Character").GetComponent<Animator>();
            ani.SetBool("Hit", true);

            yield return StartCoroutine("SpinKartBody", Vector3.up);

            ani.SetBool("Hit", false);
            locked = false;

            spunOut = false;
            spinningOut = false;
        }
    }

    public void SlideKartBody()
    {
        if (kartBodySliding != null)
            StopCoroutine(kartBodySliding);

        kartBodySliding = StartCoroutine(ActualSlideKartBody());
    }

    private IEnumerator ActualSlideKartBody()
    {
        float startTime = Time.time, travelTime = 0.25f;
        Vector3 startVal = kartBody.localPosition;

        while (Time.time - startTime < travelTime)
        {
            kartBody.localPosition = Vector3.Lerp(startVal, Vector3.zero, (Time.time - startTime) / travelTime);
            yield return null;
        }

        kartBody.localPosition = Vector3.zero;

        kartBodySliding = null;
    }

    public class KartWheel
    {
        public FauxCollider collider { get; private set; }
        public Transform transform { get; private set; }
        public Vector3 targetPos;
        public Vector3 position { get { return transform.position; } }

        public float wheelAngle, wheelSpin;
        public float radius { get; private set; }

        public KartWheel(FauxCollider _collider, Wheel_Skeleton skeleton)
        {
            collider = _collider;
            radius = skeleton.wheelRadius;
            wheelSpin = 0f;
            wheelAngle = 0f;

            transform = skeleton.transform;
            targetPos = transform.localPosition;
        }
    }

}
