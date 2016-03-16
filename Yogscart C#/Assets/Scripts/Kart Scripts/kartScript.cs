using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class kartScript : MonoBehaviour
{

    //Lock all kart controls
    public bool locked = true;

    //Inputs
    public float throttle, steer;
    public bool drift;

    //Physics Stuff
    private bool isFalling, isColliding;

    //Kart Stats
    public float maxSpeed = 20f;
    private float lastMaxSpeed, acceleration = 10f, brakeTime = 0.5f, turnSpeed = 2f, driftAmount = 1f;

    private bool offRoad;
    public int lapisAmount;

    //Driving Stuff
    private float expectedSpeed, actualSpeed;

    public float ActualSpeed
    {
        get { return actualSpeed; }
        set { }
    }
    public float ExpectedSpeed
    {
        get
        {
            return expectedSpeed;
        }
        set
        {
            if (!isColliding)
                expectedSpeed = value;
        }
    }

    //Drifting Stuff
    private int driftSteer;
    private bool driftStarted, applyingDrift;
    const float kartbodyRot = 20f; //Amount that the kartbody rotates during drifting
    private float driftTime, blueTime = 2f, orangeTime = 4f;

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

    //Tricking off Ramps
    private bool tricking, trickPotential, trickLock;

    //Speed Altering factors
    static private float boostPercent = 0.4f, grassPercent = 0.45f;
    private float boostAddition, maxGrassSpeed;

    //Wheel Transforms
    public List<WheelCollider> wheelColliders;
    public List<Transform> wheelMeshes;
    private List<Vector3> wheelStartPos;

    //Particles
    public List<ParticleSystem> flameParticles, driftParticles;
    public ParticleSystem trickParticles;

    //Noises
    public AudioClip engineSound;

    //Collisions
    const float snapTime = 0.1f, pushSpeed = 2f, pushTime = 0.5f;
    private Vector3 touchingKart, relativeVelocity;

    //Stop Flipping
    private bool snapping;

    // Use this for initialization
    void Start()
    {
        //Setup the start positions of the wheels, these will be used when wheels aren't touching the ground
        wheelStartPos = new List<Vector3>();
        for (int i = 0; i < wheelMeshes.Count; i++)
        {
            wheelStartPos.Add(wheelMeshes[i].localPosition);
        }

        //Calculate speed altering factors based on current max speed
        boostAddition = maxSpeed * boostPercent;
        maxGrassSpeed = maxSpeed * grassPercent;
        driftAmount = turnSpeed / 2f;
    }

    // Update is called once per 60th of a second
    void FixedUpdate()
    {
        float lastTime = Time.fixedDeltaTime;
        lastTime = Mathf.Clamp(lastTime, 0f, 0.034f);

        if (Time.timeScale != 0)
        {
            CalculateExpectedSpeed(lastTime);

            if (!isFalling)
            {
                ApplySteering(lastTime);
            }
            else
            {
                wheelColliders[0].steerAngle = 0;
                wheelColliders[1].steerAngle = 0;
            }

            ApplyDrift(lastTime);

            float nMaxSpeed = Mathf.Lerp(lastMaxSpeed, maxSpeed - (1f - lapisAmount / 10f), lastTime);
            ExpectedSpeed = Mathf.Clamp(ExpectedSpeed, -nMaxSpeed, nMaxSpeed);

            if (isBoosting != BoostMode.Not)
            {
                nMaxSpeed = maxSpeed + boostAddition;
                ExpectedSpeed = maxSpeed + boostAddition;
            }

            actualSpeed = relativeVelocity.z;

            float nA = (ExpectedSpeed - actualSpeed) / lastTime;
            if (!isFalling && !isColliding && Mathf.Abs(nA) > 0.1f)
            {
                GetComponent<Rigidbody>().AddForce(transform.forward * nA, ForceMode.Acceleration);
            }

            lastMaxSpeed = nMaxSpeed;

            //Keep kart upwards
            if(isFalling)
            {
                transform.localRotation = Quaternion.Slerp(transform.localRotation, Quaternion.Euler(0, transform.localRotation.eulerAngles.y, 0), lastTime * 5f);

               /* if(Vector3.Angle(transform.forward,GetComponent<Rigidbody>().velocity) > 20f)
                {
                    Vector3 horiVel = GetComponent<Rigidbody>().velocity;
                    horiVel.y = 0f;
                    float scale = horiVel.magnitude;

                    horiVel = Vector3.Lerp(horiVel, transform.forward * scale, lastTime * 5f);

                    GetComponent<Rigidbody>().velocity = new Vector3(horiVel.x, GetComponent<Rigidbody>().velocity.y, horiVel.z);
                }*/
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        lapisAmount = (int)Mathf.Clamp(lapisAmount, 0f, 10f);
        relativeVelocity = transform.InverseTransformDirection(GetComponent<Rigidbody>().velocity);

        if (Time.timeScale != 0)
        {
            isFalling = CheckGravity();

            RaycastHit hit;
            if (Physics.Raycast(transform.position, -transform.up, out hit, 1) && hit.collider.tag == "OffRoad")
                offRoad = true;
            else
                offRoad = false;

            DoTrick();

            for (int i = 0; i < wheelMeshes.Count; i++)
            {
                Vector3 wheelPos;
                Quaternion wheelRot;

                wheelColliders[i].GetWorldPose(out wheelPos, out wheelRot);

                if (i == 0 || i == 2)
                    wheelMeshes[i].rotation = wheelRot;
                else
                    wheelMeshes[i].rotation = wheelRot * Quaternion.Euler(0, 180, 0);

                wheelMeshes[i].localPosition = wheelStartPos[i];

                Vector3 nPos = wheelMeshes[i].position;
                nPos.y = wheelPos.y;
                wheelMeshes[i].position = nPos;
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
                GetComponent<AudioSource>().volume = Mathf.Lerp(0.05f, 0.4f, ExpectedSpeed / maxSpeed);
                GetComponent<AudioSource>().pitch = Mathf.Lerp(0.75f, 1.5f, ExpectedSpeed / maxSpeed);
            }

            //Calculate Start Boost
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
            else if (startBoostVal == 0 && allowedBoost)
                Boost(startBoostAmount, BoostMode.Trick);

            //Boost Particles
            if(isBoosting == BoostMode.Not)
            {
                for(int i = 0; i < flameParticles.Count; i++)
                {
                    if (flameParticles[i].isPlaying)
                        flameParticles[i].Stop();
                }
            }
            else
            {
                for (int i = 0; i < flameParticles.Count; i++)
                {
                    if (!flameParticles[i].isPlaying)
                        flameParticles[i].Play();
                }
            }
        }
    }

    IEnumerator KartCollision(Transform otherKart)
    {
        //Put kart collisions effects here
        Vector3 compareVect = otherKart.transform.position - transform.position;

        if (touchingKart == Vector3.zero)
        {
            if (Vector3.Angle(compareVect, transform.right) > 90) //Decides where way will push us away from the kart
                touchingKart = transform.right;
            else
                touchingKart = -transform.right;
        }

        transform.position += (touchingKart * (2f - compareVect.magnitude));

        var startTime = Time.time;
        while (Time.time - startTime <= pushTime)
        {
            relativeVelocity.x = pushSpeed * touchingKart.x;
            GetComponent<Rigidbody>().velocity = transform.TransformDirection(relativeVelocity);
            yield return null;
        }
    }

    void CancelCollision()
    {
        touchingKart = Vector3.zero;

        //Remove the horizontal velocity
        relativeVelocity.x = 0;
        GetComponent<Rigidbody>().velocity = transform.TransformDirection(relativeVelocity);
        GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }

    void DoTrick()
    {
        if (isFalling || locked)
        {
            if (trickPotential)
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
                ExpectedSpeed = 0;
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

        if (Mathf.Abs(actualSpeed) < Mathf.Abs(ExpectedSpeed) - 5)
            ExpectedSpeed = actualSpeed;
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
            wheelColliders[0].steerAngle = Mathf.Lerp(wheelColliders[0].steerAngle, (driftSteer * turnSpeed) + (steer * driftAmount), lastTime * 500f);
            wheelColliders[1].steerAngle = Mathf.Lerp(wheelColliders[1].steerAngle, (driftSteer * turnSpeed) + (steer * driftAmount), lastTime * 500f);
        }

        if (isColliding || isFalling)
        {
            wheelColliders[0].steerAngle = 0;
            wheelColliders[1].steerAngle = 0;
        }

    }

    bool CheckGravity()
    {
        bool grounded = false;
        for (int i = 0; i < 4; i++)
        {
            if (wheelColliders[i].isGrounded)
            {
                grounded = true;
                break;
            }
        }

        if (grounded || Physics.Raycast(transform.position, -transform.up, 1))
            return false;
        else
            return true;
    }

    void ApplyDrift(float lastTime)
    {
        Transform KartBody = transform.FindChild("Kart Body");

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
                KartBody.localRotation = Quaternion.Slerp(KartBody.localRotation, Quaternion.Euler(0, kartbodyRot * driftSteer, 0), lastTime * 2);

            for (int f = 0; f < 2; f++)
            {
                if (driftTime >= orangeTime)
                {
                    driftParticles[f].GetComponent<Renderer>().material = Resources.Load<Material>("Particles/Drift Particles/Spark_Orange");
                }
                else if (driftTime >= blueTime)
                {
                    driftParticles[f].GetComponent<ParticleSystem>().Play();
                    driftParticles[f].GetComponent<Renderer>().material = Resources.Load<Material>("Particles/Drift Particles/Spark_Blue");
                }
            }

        }
        else {

            if (isFalling || offRoad)
                driftTime = 0;

            if (!spinning)
                KartBody.localRotation = Quaternion.Slerp(KartBody.localRotation, Quaternion.Euler(0, 0, 0), lastTime * 2);

            if (throttle > 0)
            {
                if (driftTime >= orangeTime)
                {
                    Boost(1f, BoostMode.DriftBoost);
                }
                else if (driftTime >= blueTime)
                {
                    Boost(0.5f, BoostMode.DriftBoost);
                }
            }

            driftTime = 0f;
            driftParticles[0].GetComponent<ParticleSystem>().Stop();
            driftParticles[1].GetComponent<ParticleSystem>().Stop();
        }

    }

    IEnumerator ResetDrift()
    {
        yield return new WaitForSeconds(0.1f);
        applyingDrift = false;
    }

    public void Boost(float t, BoostMode type)
    {
        isBoosting = type;
        StopCoroutine("StartBoost");
        StartCoroutine("StartBoost", t);
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
            transform.FindChild("Kart Body").Rotate((dir * 360f * Time.deltaTime) / time);
            yield return null;
        }

        spinning = false;
    }

    void SpinOut()
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
            Animator ani = transform.FindChild("Kart Body").FindChild("Character").GetComponent<Animator>();
            ani.SetBool("Hit", true);

            yield return StartCoroutine("SpinKartBody", Vector3.up);

            ani.SetBool("Hit", false);
            locked = false;

            spunOut = false;

        }
    }

    /*IEnumerator SnapUp()
    {
        if (!snapping)
        {
            snapping = true;

            Quaternion startRot = transform.rotation;
            float startTime = Time.time;

            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

            while (Time.time - startTime < snapTime)
            {
                transform.rotation = Quaternion.Lerp(startRot, Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0), (Time.realtimeSinceStartup - startTime) / snapTime);
                yield return null;
            }

            GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

            snapping = false;
        }
    }*/

    bool HaveTheSameSign(float first, float second)
    {
        return (Mathf.Sign(first) == Mathf.Sign(second));
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody == null)
            StartCoroutine("Collided", collision);
    }

    IEnumerator Collided(Collision collision)
    {
        RaycastHit hit;
        if (!Physics.Raycast(transform.position, transform.right * 4f) && !Physics.Raycast(transform.position, -transform.right * 4f))
        {
            if (Physics.Raycast(transform.position, transform.forward * Mathf.Sign(ExpectedSpeed), out hit))
            {
                if (hit.collider == collision.collider)
                {
                    ExpectedSpeed /= 2f;
                    ExpectedSpeed = -ExpectedSpeed;
                }
            }
        }

        isColliding = true;
        yield return new WaitForSeconds(0.2f);
        isColliding = false;
    }
}
