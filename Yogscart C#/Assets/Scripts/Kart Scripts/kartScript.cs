using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class kartScript : MonoBehaviour
{
    //Lock all kart controls
    public bool locked = true;

    private AudioSourceInfo audioSourceInfo;
    private AudioSource kartAudioSource;

    //Inputs
    public float throttle, steer;
    public bool drift;

    //Physics Stuff
    private bool isFallingValue;
    public bool isFalling
    {
        get
        {
            return isFallingValue;
        }
        private set
        {
            isFallingValue = value;           
        }
    }

    //Kart Stats
    public float modifier = 1f;
    const float  maxSpeedVal = 22f, accelerationVal = 10f, brakeTimeVal = 0.5f, turnSpeedVal = 2f;

    private float lastMaxSpeed, maxSpeed = 22f, acceleration = 10f, brakeTime = 0.5f, turnSpeed = 2f;

    public float MaxSpeed
    {
        get { return maxSpeed; }
    }

    private bool offRoad;
    public int lapisAmount;

    //Driving Stuff
    public float expectedSpeed, actualSpeed;

    public float ActualSpeed { get; private set; }
    public float ExpectedSpeed
    {
        get
        {
            return expectedSpeed;
        }
        set
        {
            expectedSpeed = value;
        }
    }

    //Drifting Stuff
    public int driftSteer { get; private set; }
    public bool driftStarted { get; private set; }
    private bool applyingDrift;
    const float kartbodyRot = 20f; //Amount that the kartbody rotates during drifting
    public float driftTime { get; private set; }
    public float blueTime { get { return 2f; } }
    public float orangeTime { get { return 4f; } }
    public bool onlineMode = false; //Stops local boosting and spinning out. This is handled by the server (These are handled by server)

    //Spinning Out
    private const float spinTime = 0.5f, spunTime = 1.5f;
    private bool spinning, spunOut;

    //Boosting Stuff
    public enum BoostMode { Not, Boost, DriftBoost, Trick };
    private BoostMode isBoosting = BoostMode.Not;
    //Boost at Start
    private bool allowedBoost, spinOut;
    private float startBoostAmount;
    public static int startBoostVal = -1;
    public static bool raceStarted = false;
    private float wheelSpinLerp = 0f;

    public bool spinningOut;

    //Tricking off Ramps
    public bool tricking;
    private bool trickPotential, trickLock;

    //Speed Altering factors
    static private float boostPercent = 0.4f, grassPercent = 0.45f;
    private float boostAddition, maxGrassSpeed;

    //Wheel Transforms
    [HideInInspector]
    public List<WheelCollider> wheelColliders;
    [HideInInspector]
    public List<Transform> wheelMeshes;
    //Wheel Turning / Spinning
    private float wheelSpin;
    private const float wheelSpinScalar = 25f;
    private float[] lerpWheelRot;

    //Particles
    public Dictionary<string, ParticleSystem> particleSystems;

    //Noises
    public AudioClip engineSound;
    public float quietTimer; //Used to make engine quiter after a couple of seconds

    //Collisions
    public bool isColliding = false;

    private Vector3 relativeVelocity;
    private Rigidbody kartRigidbody;

    //Skid Marks
    private static Transform skidMarkTransform;
    public LineRenderer[] currentLineRenderer;
    public float skidTimer;

    //Character Taunts and Hit Noises
    public int characterID;

    //Store Systems as arrays for convinence
    Transform kartBody;
    ParticleSystem[] startCloudParticles, flameParticles, driftParticles, driftCloudParticles;
    ParticleSystem trickParticles;

    // Use this for initialization
    void Start()
    {
        //Calculate speed altering factors based on current max speed
        boostAddition = maxSpeed * boostPercent;
        maxGrassSpeed = maxSpeed * grassPercent;

        kartRigidbody = GetComponent<Rigidbody>();
        kartRigidbody.centerOfMass = new Vector3(0f, -0.5f, 0f);

        audioSourceInfo = GetComponent<AudioSourceInfo>();
        kartAudioSource = GetComponentInChildren<AudioSource>();

        if (skidMarkTransform == null)
            skidMarkTransform = Resources.Load<Transform>("Prefabs/SkidMarks");

        ResetParticles();
    }

    public void ResetParticles()
    {
        //Get Kart Body
        kartBody = transform.FindChild("Kart Body");

        //Get Particles
        startCloudParticles = new ParticleSystem[] { particleSystems["L_StartClouds"], particleSystems["R_StartClouds"] };
        flameParticles = new ParticleSystem[] { particleSystems["L_Flame"], particleSystems["R_Flame"] };
        driftParticles = new ParticleSystem[] { particleSystems["L_Sparks"], particleSystems["R_Sparks"] };
        driftCloudParticles = new ParticleSystem[] { particleSystems["L_DriftClouds"], particleSystems["R_DriftClouds"] };
        trickParticles = particleSystems["Trick"];
    }

    // Update is called once per 60th of a second
    void FixedUpdate()
    {
        float lastTime = Time.deltaTime;

        maxSpeed = maxSpeedVal * modifier;
        acceleration = accelerationVal * modifier;
        brakeTime = brakeTimeVal * modifier;
        //turnSpeed = turnSpeedVal * modifier;


        if (Time.timeScale != 0)
        {

            isFalling = CheckGravity();

            if (!isFalling)
                CalculateExpectedSpeed(lastTime);

            ApplySteering(lastTime);

            ApplyDrift(lastTime);

            float nMaxSpeed = Mathf.Lerp(lastMaxSpeed, maxSpeed - (1f - lapisAmount / 10f), lastTime);
            ExpectedSpeed = Mathf.Clamp(ExpectedSpeed, -nMaxSpeed, nMaxSpeed);

            if (isBoosting != BoostMode.Not)
            {
                nMaxSpeed = maxSpeed + boostAddition;
                ExpectedSpeed = maxSpeed + boostAddition;
            }

            actualSpeed = relativeVelocity.z;

            if (Mathf.Abs(actualSpeed) < 0.1f)
                actualSpeed = 0f;

            bool wallInFront = false;
            if (isFalling)
            {
                Vector3 forward = Vector3.Scale(transform.forward, new Vector3(1, 0f, 1f));
                RaycastHit hit;

                wallInFront = Physics.Raycast(transform.position + transform.up, forward, out hit, 1.5f) && hit.transform.GetComponent<Rigidbody>() == null;

                if (wallInFront && expectedSpeed > 0)
                {
                    expectedSpeed = -1;
                    Debug.Log("Ahhh a wall!!! " + expectedSpeed);
                    CancelBoost();
                }

                //Stop Extreme Boosting when in the Air
                if (!wallInFront && actualSpeed < expectedSpeed)
                {
                    expectedSpeed = actualSpeed;
                }
            }


            float nA = (ExpectedSpeed - actualSpeed) / lastTime;

            if ((!isFalling && !isColliding) || wallInFront)
            {

                float absExp = Mathf.Abs(expectedSpeed), absAct = Mathf.Abs(actualSpeed);

                if (absAct > 1 || (expectedSpeed < -2f && actualSpeed == 0f))
                {
                    Vector3 myForward = transform.forward;
                    myForward.y = 0;

                    kartRigidbody.AddForce(myForward * nA, ForceMode.Acceleration);

                    foreach (WheelCollider collider in wheelColliders)
                    {
                        if (collider.enabled)
                        {
                            collider.motorTorque = 0f;
                            collider.brakeTorque = 0f;
                        }
                    }

                }
                else
                {
                    foreach (WheelCollider collider in wheelColliders)
                    {
                        if (collider.enabled)
                        {
                            if (absExp > absAct)
                            {
                                collider.motorTorque = MathHelper.Sign(expectedSpeed) * 5000f * nA;
                                collider.brakeTorque = 0f;
                            }
                            else if (absExp < absAct)
                            {
                                collider.motorTorque = 0f;
                                collider.brakeTorque = 5000f;
                            }
                        }
                    }
                }
            }

            lastMaxSpeed = nMaxSpeed;

            //Keep kart upwards
            Debug.DrawRay(transform.position, -transform.up * 3f);
            if (isFalling && !Physics.Raycast(transform.position, -transform.up, 3f))
            {
                kartRigidbody.freezeRotation = true;
                Quaternion finalRot = Quaternion.LookRotation(Vector3.Scale(transform.forward, new Vector3(1f, 0f, 1f)), Vector3.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, finalRot, Time.deltaTime * 8f);
            }
            else
            {
                kartRigidbody.freezeRotation = false;
            }

            //Stop Kart Sliding
            if (!isFalling && !isColliding)
            {
                Vector3 currentVelocity = kartRigidbody.velocity, changedVelocity = currentVelocity;

                if (currentVelocity.magnitude > 15f)
                {
                    Vector3 actualForward = transform.forward * Mathf.Sign(actualSpeed);

                    if (Vector3.Angle(Vector3.Cross(actualForward, new Vector3(1, 0, 1)), Vector3.Cross(currentVelocity, new Vector3(1, 0, 1))) > 5f)
                    {
                        float mag = new Vector3(currentVelocity.x, 0f, currentVelocity.z).magnitude;
                        changedVelocity = actualForward * mag;
                        changedVelocity.y = currentVelocity.y;

                        kartRigidbody.velocity = changedVelocity;
                    }
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        lapisAmount = (int)Mathf.Clamp(lapisAmount, 0f, 10f);
        relativeVelocity = transform.InverseTransformDirection(kartRigidbody.velocity);

        if (Time.timeScale != 0)
        {
            RaycastHit hit;
            if (!isFalling && Physics.Raycast(transform.position, -transform.up, out hit, 1) && hit.collider.tag == "OffRoad")
                offRoad = true;
            else
                offRoad = false;

            DoTrick();

            //Make Wheels Rotate if Throttle is down
            float increaseAmount = 0f;

            //Make Smoke from wheels speed up and slow down
            if (!raceStarted)
            {
                if (throttle > 0)
                    wheelSpinLerp = Mathf.Lerp(wheelSpinLerp, 15f, Time.deltaTime * 2f);
                else
                    wheelSpinLerp = Mathf.Lerp(wheelSpinLerp, 0f, Time.deltaTime * 4f);

                foreach (ParticleSystem ps in startCloudParticles)
                {
                    ParticleSystem.EmissionModule emission = ps.emission;
                    emission.rateOverTimeMultiplier = wheelSpinLerp;
                }

                //Spin wheels forward during start
                increaseAmount = Time.deltaTime * wheelSpinLerp;
            }
            else if (throttle > 0 || actualSpeed > 0f)
            {
                //Otherwise speed it depending on Kart Speed
                increaseAmount = Time.deltaTime * actualSpeed;
            }

            if (throttle < 0 || actualSpeed < 0f)
            {
                //Spin wheels forward during start
                if (throttle < 0 && actualSpeed > -5f)
                    increaseAmount = Time.deltaTime * -5f;
                else //Otherwise speed it depending on Kart Speed
                    increaseAmount = Time.deltaTime * actualSpeed;
            }

            wheelSpin = MathHelper.NumClamp(wheelSpin + (increaseAmount * wheelSpinScalar), 0f, 360f);

            if (lerpWheelRot == null)
            {
                lerpWheelRot = new float[2];
            }

            //Move and Rotate each wheel mesh accordingly
            for (int i = 0; i < wheelMeshes.Count; i++)
            {
                //Make wheel turn left and right smoothly
                if (i < 2)
                    lerpWheelRot[i] = Mathf.Lerp(lerpWheelRot[i], wheelColliders[i].steerAngle * 5f, Time.deltaTime * 5f);

                Quaternion wheelRot = Quaternion.Euler(wheelSpin, (i < 2) ? lerpWheelRot[i] : 0f, 0f);

                if (i == 0 || i == 2)
                    wheelMeshes[i].localRotation = wheelRot;
                else
                    wheelMeshes[i].localRotation = wheelRot * Quaternion.Euler(0, 180, 0);

                //Make Wheel stick to road
                Vector3 wheelPos;

                if (wheelColliders[i].isGrounded)
                {
                    Quaternion nullHolder = new Quaternion();
                    wheelColliders[i].GetWorldPose(out wheelPos, out nullHolder);

                    Vector3 toLocal = wheelMeshes[i].parent.InverseTransformPoint(wheelPos);
                    Vector3 localPos = wheelMeshes[i].localPosition;
                    localPos.y = toLocal.y;

                    wheelMeshes[i].localPosition = localPos;
                }
            }

            //Play engine Audio
            if (engineSound != null)
            {
                if (!GetComponent<AudioSource>().isPlaying)
                {
                    GetComponent<AudioSource>().clip = engineSound;
                    GetComponent<AudioSource>().Play();
                    GetComponent<AudioSource>().loop = true;
                }

                if (raceStarted)
                {
                    float percent = ExpectedSpeed / maxSpeed;
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
                    GetComponent<AudioSource>().pitch = normalPitch;

                }
                else
                {
                    float percent = wheelSpinLerp / 15f;
                    audioSourceInfo.idealVolume = Mathf.Lerp(0.1f, 0.4f, percent);
                    GetComponent<AudioSource>().pitch = Mathf.Lerp(0.75f, 1.5f, percent);
                }
            }

            //Calculate Start Boost
            if (!onlineMode && startBoostVal >= 0)
            {
                bool throttleOver = (throttle > 0f);

                if (startBoostVal == 3 && throttleOver)
                {
                    spinOut = true;
                }
                else if (startBoostVal == 2 && throttleOver && !spinOut)
                {
                    allowedBoost = true;
                }
                else if (startBoostVal < 2 && startBoostVal != 0 && throttle == 0)
                {
                    allowedBoost = false;
                }

                if (allowedBoost && throttleOver)
                    startBoostAmount += Time.deltaTime * 0.1f;

                if (startBoostVal == 0 && spinOut)
                    SpinOut();
                else if (startBoostVal == 0 && allowedBoost && startBoostAmount > 0)
                {
                    Boost(startBoostAmount, BoostMode.Trick);
                    allowedBoost = false;
                }

                if (startBoostVal == 0)
                    raceStarted = true;
            }

            if (GetComponent<AI>() == null)
            {
                foreach (ParticleSystem startCloud in startCloudParticles)
                {
                    if (!raceStarted && throttle > 0)
                    {
                        if (!startCloud.isPlaying)
                            startCloud.Play();
                    }
                    else
                    {
                        if (startCloud.isPlaying)
                            startCloud.Stop();
                    }
                }
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

            //Stop Kart from flying off the ground
            RaycastHit groundHit;
            if(Physics.Raycast(transform.position,Vector3.down, out groundHit, 1f))
            {
                Vector3 toGround = transform.position - groundHit.point;          
                if(kartRigidbody.velocity.y > 0.75f && toGround.magnitude > 0.5f)
                {
                    kartRigidbody.AddForce(new Vector3(0f, -(kartRigidbody.velocity.y * 0.6f), 0f), ForceMode.VelocityChange);
                }
            }
        }
    }

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

    void CalculateExpectedSpeed(float lastTime)
    {
        if (throttle == 0 || locked || spunOut)
        {
            float cacceleration = -ExpectedSpeed / brakeTime;
            ExpectedSpeed += (cacceleration * lastTime);

            if (Mathf.Abs(ExpectedSpeed) <= 0.02)
            {
                ExpectedSpeed = 0;
            }
        }
        else
        {
            if (HaveTheSameSign(throttle, ExpectedSpeed) == false)
                ExpectedSpeed += (throttle * acceleration * 2) * lastTime;
            else {

                float percentage;

                if (offRoad && isBoosting != BoostMode.Boost)
                    percentage = (1f / maxGrassSpeed) * Mathf.Abs(ExpectedSpeed);
                else
                    percentage = (1f / maxSpeed) * Mathf.Abs(ExpectedSpeed);

                ExpectedSpeed += (throttle * acceleration * (1f - percentage)) * lastTime;
            }
        }

        // if (Mathf.Abs(actualSpeed) < Mathf.Abs(ExpectedSpeed) - 5)
        //   ExpectedSpeed = actualSpeed;
    }

    void ApplySteering(float lastTime)
    {
        float nSteer = 0;
        if (!driftStarted)
        {
            if (steer != 0)
            {
                nSteer = Mathf.Sign(steer);
                nSteer *= Mathf.Lerp(3f, 0.8f, Mathf.Abs(actualSpeed) / (maxSpeed + boostAddition));
            }
            wheelColliders[0].steerAngle = nSteer * turnSpeed;
            wheelColliders[1].steerAngle = nSteer * turnSpeed;
        }
        else
        {
            nSteer = Mathf.Sign(driftSteer);
            nSteer *= Mathf.Lerp(2f, 0.8f, Mathf.Abs(actualSpeed) / (maxSpeed + boostAddition));

            float nDriftAmount = Mathf.Lerp(1f, 1.5f, Mathf.Abs(driftSteer + steer));

            wheelColliders[0].steerAngle = Mathf.Lerp(wheelColliders[0].steerAngle, (nSteer * turnSpeed) + (steer * nDriftAmount), lastTime * 50f);
            wheelColliders[1].steerAngle = Mathf.Lerp(wheelColliders[1].steerAngle, (nSteer * turnSpeed) + (steer * nDriftAmount), lastTime * 50f);
        }

        if (isFalling)
        {
            wheelColliders[0].steerAngle = 0;
            wheelColliders[1].steerAngle = 0;
        }

    }

    bool CheckGravity()
    {
        bool grounded = true;

        for (int i = 0; i < 4; i++)
        {
            if (!wheelColliders[i].isGrounded)
            {
                grounded = false;
                break;
            }
        }

        if (grounded && Physics.Raycast(transform.position, -transform.up, 1))
            return false;
        else
            return true;
    }

    void ApplyDrift(float lastTime)
    {
        

        if (drift && ExpectedSpeed > maxSpeed * 0.75f && !isFalling && (!offRoad || (offRoad && isBoosting == BoostMode.Boost)))
        {
            if (!applyingDrift && Mathf.Abs(steer) > 0.2 && driftStarted == false)
            {
                driftStarted = true;
                driftSteer = (int)Mathf.Sign(steer);
            }
        }
        else {
            if (driftStarted == true)
            {
                applyingDrift = true;
                driftStarted = false;
                StartCoroutine("ResetDrift");
            }
        }

        if (driftStarted == true)
        {
            driftTime += lastTime * Mathf.Abs(driftSteer + (steer / 2f));
            if (!spinning)
                kartBody.localRotation = Quaternion.Slerp(kartBody.localRotation, Quaternion.Euler(0, kartbodyRot * driftSteer, 0), lastTime * 2);

            for (int f = 0; f < 2; f++)
            {
                if (driftTime >= orangeTime)
                {
                    driftParticles[f].GetComponent<Renderer>().material = Resources.Load<Material>("Particles/Drift Particles/Spark_Orange");
                }
                else if (driftTime >= blueTime)
                {
                    //Turn Particles on if they're not
                    if (!driftParticles[f].GetComponent<ParticleSystem>().isPlaying)
                        driftParticles[f].GetComponent<ParticleSystem>().Play();

                    driftParticles[f].GetComponent<Renderer>().material = Resources.Load<Material>("Particles/Drift Particles/Spark_Blue");
                }

                if (!driftCloudParticles[f].isPlaying)
                    driftCloudParticles[f].Play();
            }

            //Spawn Line Renderer and Do Drift
            if(currentLineRenderer == null)
            {
                //Create New Arrray
                int half = wheelColliders.Count / 2;
                currentLineRenderer = new LineRenderer[half];

                //Create Object for each wheel
                for (int i = 0; i < half; i++)
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
                int half = wheelColliders.Count / 2;
                if (skidTimer <= 0f)
                {                  
                    for (int i = 0; i < half; i++)
                            currentLineRenderer[i].positionCount = currentLineRenderer[i].positionCount + 1;

                    skidTimer = 0.2f;
                }

                //Update position of late Point
                for (int i = 0; i < half; i++)
                {
                    WheelHit hit;
                    bool onGround = wheelColliders[half + i].GetGroundHit(out hit);

                    if (onGround)
                    {
                        Transform wheelMesh = wheelMeshes[half + i];
                        Vector3 hitPoint = wheelMesh.position + (Vector3.down * wheelColliders[half + i].radius);

                        currentLineRenderer[i].SetPosition(currentLineRenderer[i].positionCount - 1, hitPoint + new Vector3(0f, 0.1f, 0f));
                    }
                }
            }
        }
        else {

            //Reset Skid Marks
            if(currentLineRenderer != null)
            {
                foreach(LineRenderer lr in currentLineRenderer)
                {
                    if(lr.positionCount == 0)
                    {
                        DestroyImmediate(lr.gameObject);
                    }
                }

                currentLineRenderer = null;
                skidTimer = 0f;
            }

            if (isFalling || offRoad)
                driftTime = 0;

            if (!spinning)
                kartBody.localRotation = Quaternion.Slerp(kartBody.localRotation, Quaternion.Euler(0, 0, 0), lastTime * 2);

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

    IEnumerator ResetDrift()
    {
        yield return new WaitForSeconds(0.1f);
        applyingDrift = false;
        driftSteer = 0;
    }

    public void Boost(float t, BoostMode type)
    {
        isBoosting = type;
        StopCoroutine("StartBoost");
        StartCoroutine("StartBoost", t);

        //If Kart is networked send this information to server and clients
        if (GetComponent<KartNetworker>() != null && !onlineMode)
        {
            GetComponent<KartNetworker>().SendBoost(t, type);
        }
    }

    IEnumerator StartBoost(float t)
    {
        AudioClip BoostSound = Resources.Load<AudioClip>("Music & Sounds/SFX/boost");
        GetComponent<AudioSource>().PlayOneShot(BoostSound, 3);

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

            if(doNoise)
            {
                Character myCharacter = FindObjectOfType<CurrentGameData>().characters[characterID];
                if(myCharacter.hitSounds != null && myCharacter.hitSounds.Length > 0)
                    kartAudioSource.PlayOneShot(myCharacter.hitSounds[Random.Range(0, myCharacter.hitSounds.Length)]);
            }

            spinningOut = true;
        }
    }

    public void SpinOut()
    {
        SpinOut(false);
    }

    public void DoTaunt()
    {
        Character myCharacter = FindObjectOfType<CurrentGameData>().characters[characterID];

        //Play Sound
        if (myCharacter.tauntSounds != null && myCharacter.tauntSounds.Length > 0)
            kartAudioSource.PlayOneShot(myCharacter.tauntSounds[Random.Range(0, myCharacter.tauntSounds.Length)]);

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
            Animator ani = kartBody.FindChild("Character").GetComponent<Animator>();
            ani.SetBool("Hit", true);

            yield return StartCoroutine("SpinKartBody", Vector3.up);

            ani.SetBool("Hit", false);
            locked = false;

            spunOut = false;
            spinningOut = false;
        }
    }

    bool HaveTheSameSign(float first, float second)
    {
        return (Mathf.Sign(first) == Mathf.Sign(second));
    }

}
