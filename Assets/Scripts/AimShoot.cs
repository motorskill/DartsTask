using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using System.IO;
using System.IO.Ports;
using UnityEngine;
using System.Drawing;
using UnityEngine.UI; // Required for UI
using DG.Tweening;
using System.Runtime.Remoting.Messaging;
using UnityEngine.XR;
using System.Configuration; // Import DOTween namespac

public class AimShoot : MonoBehaviour
{

    public Transform CrossHair;
    [SerializeField] Rigidbody CrossRigid;
    [SerializeField] GameObject cylinderPrefab; // Assign the cylinder prefab in the inspector
    [SerializeField] LineRenderer invisVis; // LineRenderer for debugging the invisible vector
    [SerializeField] bool debugInvisibleVector = true; // Toggle visibility of the LineRenderer
    [SerializeField] public Transform InvisTargetCross;
    [SerializeField] Rigidbody InvisTargetRigid;
    [SerializeField] DataOutputAndConfig dataOutputAndConfig;
    Header Conditions;
    [SerializeField] public GameObject featureTarget;
    [SerializeField] Transform fixationPoint;

    // Input vars
    public float horizontalInput;
    public float verticalInput;

    // Aiming Vars
    private float CrossVelocity;
    private Vector3 AimVector;
    private Vector3 currentVelocity; // Current velocity
    public float acceleration = 5f;        // Acceleration rate
    public float deceleration;        // Deceleration rate
    private float previousHinput;
    private float previousHinputSign;
    private float previousVinput;
    private float previousVinputSign;

    // variables to track initial input position
    private bool hasSetInitialPosition = false;
    private float initialX, initialY;

    // Shooting Vars
    private bool nullShoot = false;
    public float shootSpeed = 10f;    // Speed of the cylinder
    public bool OneDart = true;
    private float inputTimeout = 0.15f;
    private float lastInputTime = 0f;
    private float inputRadius = 0.4f;
    private bool firstInput = false;
    private float leastInput = 0.02f;
    private bool generateRandomVec = true;
    private bool initAngle = true;
    private double angle;
    private Rigidbody currentDartRigidbody;

    // Random influence
    private Vector3 invisibleVector; // The "invisible force" vector
    private Vector3 targetVector;    // Target direction for the next shift
    private float currentVectorAngle;
    public float angularRange = 90f; // Maximum angular range (degrees) for shifts
    public float shiftSpeed = 5f;    // Speed of the vector's rotation
    public float forceMagnitude; // Magnitude of the invisible force
    private float timeSinceLastShift = 0f; // Timer for periodic updates
    public float shiftInterval = 2f;  // Time interval for changing the vector


    // Serial port vars
    public SerialPort serialPort;
    public string mostRecentData = ""; // To store the most recent data point
    public object lockObject = new object(); // Lock for thread-safe access

    [SerializeField]
    public string portName = "COM4"; // Replace with your port name
    [SerializeField]
    public int baudRate = 115200;
    private bool DiscardOldInput = true;

    // Timer bool
    public float timeTillShoot;
    private float bufferTime = 3f;
    private float preShootTime = 5f;
    private bool fadeStarted = false; // Track if fade has started
    private bool fadeComplete = false;
    private float initialOpacity = 0f;  // Low opacity (fully transparent)
    private Vector3 initialScale = new Vector3(0.08f, 0.08f, 0.08f); // High scale (adjust as needed)
    [SerializeField]
    GameObject timerCircle;
    [SerializeField]
    private UnityEngine.UI.Image circleTimer_texture;
    [SerializeField]
    private RectTransform rectTransform;
    private float totalShootTime;

    // object reference for random
    private System.Random random = new System.Random();

    // Mouse vars
    private float absoluteMouseX = 0f; // Persistent absolute X position
    private float absoluteMouseY = 0f; // Persistent absolute Y posit

    // bools
    public bool isShooting = false;
    public bool hasShot = false;

    // Test vars
    private float radialStart = 2.5f;

    // onetime bools
    private bool assignTimetillShoot = false;
    private bool eyeOscillating = false;

    // Dot textures
    [SerializeField] Texture yellowDot;
    [SerializeField] Texture greenDot;

    // Data collection vars
    // private float timeOffset;
    public bool dataRecordingEnabled = false;
    public string[] parts;
    public int conditionIndex;
    public Transform arrowCoords;
    private ExperimentPhase currentPhase;
    private bool pausePhasetimer = false;

    //Game object for return
    [SerializeField] GameObject tabletVis;
    public TabletVis visScript;

    //Game object for distraction light
    [SerializeField] GameObject distractionLight;
    private bool lightFlicker = true;
    // scene management load only once
    private bool OnetimeReload = true;
    // MRI pulse processing
    public bool experimentStarted = false;
    private int numApostrophesPreExp;
    private bool setApostrophes = false;
    //Subject feedback
    public bool isAiming = false;
    public List<float> MedialateralEyes;
    [SerializeField] AudioSource outOfFixationDing;
    private bool playOnce = true;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the serial port
        serialPort = new SerialPort(portName, baudRate);
        serialPort.Parity = Parity.None;
        serialPort.DataBits = 8;
        serialPort.StopBits = StopBits.One;
        serialPort.Handshake = Handshake.None;

        try
        {
            serialPort.Open();
            Debug.Log("Serial port opened successfully.");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error opening serial port: {ex.Message}");
        }

        SetInitialCrosshairPosition();
        int trialIndex = PlayerPrefs.GetInt("current_trial", 1);
        InitTimerUI();
        SetupLineRenderer();
        // Cursor.visible = false;
        // if (trialIndex == 1)
        // {
        //     ResetTime();
        // }
    }

    // Update is called once per frame
    void Update()
    {
        if (!experimentStarted)
            return;
        if (dataRecordingEnabled)
        {
            if (dataOutputAndConfig.current_trial == 2 && visScript.firstRunReturn && !visScript.returnComplete)
            {
                dataOutputAndConfig.LogDataEntry("Return");
            }
            else
                dataOutputAndConfig.LogDataEntry(currentPhase.name);
        }
    }

    private float lastApostropheTime = 0f;
    private float apostropheCooldown = 0.2f;
    void FixedUpdate()
    {

        // // Debug.Log("Target Vector = " + targetVector);
        // timeSinceLastShift += Time.fixedDeltaTime;

        // // Periodically update the target vector
        // if (timeSinceLastShift >= shiftInterval)
        // {
        //     GenerateInvisibleVector(false);
        //     timeSinceLastShift = 0f;
        //     // // Smoothly rotate the invisible vector towards the target vector
        //     // invisibleVector = Vector3.Lerp(invisibleVector, targetVector, shiftSpeed * Time.fixedDeltaTime);

        // }

        // Smoothly rotate the invisible vector towards the target vector
        // invisibleVector = Vector3.Lerp(invisibleVector, targetVector, shiftSpeed * Time.fixedDeltaTime).normalized * forceMagnitude;

        // input scaling for mouse
        horizontalInput = (Input.GetAxis("Horizontal")) * 0.75f;
        verticalInput = (Input.GetAxis("Vertical")) * 0.75f;


        //  if (dataOutputAndConfig.current_trial == 2 && !setApostrophes)
        // {
        //     numApostrophesPreExp = 20;
        // }
        // if (dataOutputAndConfig.current_trial != 2 && !setApostrophes)
        // {
        //     numApostrophesPreExp = 1;
        // }
        // setApostrophes = true;
        

        // if (!experimentStarted)
        // {
        //     // Wait for the user to press the "'" key
        //     if (Input.GetKeyDown(KeyCode.Quote) && (Time.time - lastApostropheTime) > apostropheCooldown) // KeyCode.Quote corresponds to the "'" key
        //     {
        //         //int pulsesReceived = PlayerPrefs.GetInt("PulseCount");
        //         //pulsesReceived += Input.GetKeyDown(KeyCode.Quote) ? 1 : 0;
        //         lastApostropheTime = Time.time;
        //         int pulsesReceived = PlayerPrefs.GetInt("PulseCount");
        //         pulsesReceived += 1;
        //         PlayerPrefs.SetInt("PulseCount", pulsesReceived);
        //         Debug.Log($"Pulse received through apostrophe press {numApostrophesPreExp}/{pulsesReceived}");
        //         numApostrophesPreExp -= 1;
        //         if (numApostrophesPreExp == 19 && PlayerPrefs.GetInt("current_trial") == 2)
        //         {
        //             ResetTime();
        //         }
        //         if (numApostrophesPreExp <= 0)
        //             {
        //                 StartExperiment();
        //             }
        //     }
        //     return; // Skip the rest of the update loop until the experiment starts
        // }

        if (!dataOutputAndConfig.phaseManager.phaseRunning)
            return;

        if (!pausePhasetimer)
            dataOutputAndConfig.phaseManager.phaseTimer -= Time.fixedDeltaTime;

        currentPhase = dataOutputAndConfig.phaseManager.experimentPhases[dataOutputAndConfig.phaseManager.currentPhaseIndex];

        HandleCurrentPhase(currentPhase);

        currentPhase.onUpdate?.Invoke();

        if (dataOutputAndConfig.phaseManager.phaseTimer <= 0f)
        {
            currentPhase.onExit?.Invoke();
            if (!(dataOutputAndConfig.phaseManager.currentPhaseIndex == dataOutputAndConfig.phaseManager.experimentPhases.Count - 1))
            {
                dataOutputAndConfig.phaseManager.currentPhaseIndex++;
            }
            dataOutputAndConfig.phaseManager.StartNextPhase();
        }
    }

    private void MouseMethod()
    {
        // Compute input intensity for acceleration scaling
        float inputIntensity = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));

        // Compute the target velocity based on input
        AimVector = new Vector3(horizontalInput * 0.5f, verticalInput * 0.5f, 0.4f);
        AimVector += new Vector3(InvisTargetCross.position.x, InvisTargetCross.position.y, 0f);

        // Adjust invisible crosshair movement based on input intensity
        float invisAcceleration = Mathf.Lerp(0f, 7f, inputIntensity);
        currentVelocity = Vector3.MoveTowards(InvisTargetCross.position, AimVector, invisAcceleration * Time.fixedDeltaTime);
        InvisTargetRigid.MovePosition(currentVelocity);

        // Adjust crosshair movement speed based on distance
        float distanceToInvisCross = Vector3.Distance(CrossHair.position, InvisTargetCross.position);
        float crosshairSpeed = Mathf.Lerp(0f, 5f, distanceToInvisCross / 2f);

        Vector3 CrossVelocity = Vector3.MoveTowards(CrossHair.position, InvisTargetCross.position, crosshairSpeed * Time.fixedDeltaTime);
        CrossRigid.MovePosition(CrossVelocity);
    }

    private void NewMethod()
    {
        // Snap horizontal input
        if (Mathf.Abs(horizontalInput) < leastInput)
        {
            horizontalInput = Mathf.Approximately(horizontalInput, 0f) ? previousHinputSign * leastInput : Mathf.Sign(horizontalInput) * leastInput;
        }

        if (Mathf.Abs(verticalInput) < leastInput)
        {
            verticalInput = Mathf.Approximately(verticalInput, 0f) ? previousVinputSign * leastInput : Mathf.Sign(verticalInput) * leastInput;
        }

        // Compute input intensity for acceleration scaling
        float inputIntensity = Mathf.Clamp01(Mathf.Abs(horizontalInput) + Mathf.Abs(verticalInput));

        // Compute the target velocity based on input
        AimVector = new Vector3(horizontalInput * 0.5f, verticalInput * 0.5f, 2.4f);
        AimVector += new Vector3(InvisTargetCross.position.x, InvisTargetCross.position.y, 0f);

        // Clamp to box
        AimVector.x = Mathf.Clamp(AimVector.x, -1.8f, 1.8f);
        AimVector.y = Mathf.Clamp(AimVector.y, -0.8f, 2.8f);

        // Adjust invisible crosshair movement based on input intensity
        float invisAcceleration = Mathf.Lerp(0f, 7f, inputIntensity);
        currentVelocity = Vector3.MoveTowards(InvisTargetCross.position, AimVector, invisAcceleration * Time.fixedDeltaTime);
        InvisTargetRigid.MovePosition(currentVelocity);

        // Adjust crosshair movement speed based on distance
        float distanceToInvisCross = Vector3.Distance(CrossHair.position, InvisTargetCross.position);
        float crosshairSpeed = Mathf.Lerp(0f, 5f, distanceToInvisCross / 2f);

        Vector3 CrossVelocity = Vector3.MoveTowards(CrossHair.position, InvisTargetCross.position, crosshairSpeed * Time.fixedDeltaTime);
        CrossRigid.MovePosition(CrossVelocity);
    }

    private void OldMethod()
    {
        // Compute the target velocity based on input
        AimVector = new Vector3(horizontalInput, verticalInput, 0.4f);
        AimVector += new Vector3(CrossHair.position.x, CrossHair.position.y, 0f);
        AimVector += invisibleVector;

        if (AimVector.magnitude > 0.5f)
        {
            // Move current velocity toward target velocity
            currentVelocity = Vector3.MoveTowards(CrossHair.position, AimVector, acceleration * Time.fixedDeltaTime);
            CrossRigid.MovePosition(currentVelocity);

            previousHinput = horizontalInput;
            previousVinput = verticalInput;
        }
        else
        {
            previousHinput *= deceleration;
            previousVinput *= deceleration;

            AimVector = new Vector3(previousHinput, previousVinput, 0.4f);
            AimVector += new Vector3(CrossHair.position.x, CrossHair.position.y, 0f);

            currentVelocity = Vector3.MoveTowards(CrossHair.position, AimVector, acceleration * Time.fixedDeltaTime);
            CrossRigid.MovePosition(currentVelocity);
        }

        // Update the LineRenderer to visualize the invisible vector
        if (debugInvisibleVector && invisVis != null)
        {
            invisVis.SetPosition(0, CrossHair.position); // Start at the crosshair
            invisVis.SetPosition(1, CrossHair.position + invisibleVector * 30f); // End at the invisible vector's position
        }
    }

    void GenerateInvisibleVector(bool initial)
    {
        if (initial)
        {
            // Generate a random angle
            double angle = random.NextDouble() * Math.PI * 2; // Random angle in radians (0 to 2π)

            // Convert to a normalized vector
            invisibleVector = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0f) * forceMagnitude;
        }
        else
        {
            // Get the current angle of the invisible vector
            currentVectorAngle = Mathf.Atan2(invisibleVector.y, invisibleVector.x) * Mathf.Rad2Deg;

            // Calculate a random angle shift within the range
            float angleShift = (float)((random.NextDouble() * (angularRange * 2)) - angularRange);
            float newAngle = currentVectorAngle + angleShift;

            // Convert the new angle to a direction vector
            float radians = newAngle * Mathf.Deg2Rad;
            targetVector = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f).normalized * forceMagnitude;
        }

        // Debug.Log($"Generated Invisible Vector: {invisibleVector}, Target Vector: {targetVector}");
    }

    void GenerateRandomInputVector(bool initial)
    {
        if (initial)
        {
            // Generate a random angle
            angle = random.NextDouble() * Math.PI * 2; // Random angle in radians (0 to 2π)
        }

        // Convert to a normalized vector
        Vector3 randomVector = new Vector3((float)Math.Cos(angle), (float)Math.Sin(angle), 0f) * 0.105f;
        horizontalInput = randomVector.x;
        verticalInput = randomVector.y;
    }

    void Shoot()
    {
        OneDart = false;
        if (cylinderPrefab == null || CrossHair == null)
        {
            Debug.LogError("Cylinder prefab or target is not assigned!");
            return;
        }

        // Spawn the cylinder at the camera's position and orientation
        GameObject cylinder = Instantiate(
            cylinderPrefab,
            Camera.main.transform.position,
            Camera.main.transform.rotation
        );

        // Get the direction towards the target
        Vector3 direction = (CrossHair.position - Camera.main.transform.position).normalized;

        cylinder.transform.LookAt(transform.position);

        // Activate the GameObject (make it visible and functional)
        cylinder.SetActive(true);

        arrowCoords = cylinder.transform;
        dataOutputAndConfig.arrowIsNull = false;

        // Apply velocity to the cylinder
        currentDartRigidbody = cylinder.GetComponent<Rigidbody>();
        if (currentDartRigidbody != null)
        {
            currentDartRigidbody.velocity = direction * shootSpeed;
        }
        else
        {
            Debug.LogError("No Rigidbody attached to the cylinder prefab!");
        }
        hasShot = true;
        // Start coroutine to reset OneDart after destruction
        StartCoroutine(ResetOneDart(cylinder, 5f));
    }

    // Coroutine to reset OneDart
    IEnumerator ResetOneDart(GameObject cylinder, float delay)
    {
        // Wait for the delay time (same as the destruction time)
        yield return new WaitForSeconds(delay);
        // Close the serial port when the application quits/reloads
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Serial port closed.");
        }

        // if (eyeLinkManager != null)
        // {
        //     eyeLinkManager.StopRecording();
        //     eyeLinkManager.FreeFSAMPLE();
        // }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Destroy the cylinder
        if (cylinder != null)
        {
            Destroy(cylinder);
        }

        // OneDart = true;
    }
    private void PreProcessInputData()
    {
        // Read the most recent data and log it
        if (!serialPort.IsOpen)
        {
            try
            {
                serialPort.Open();
                Debug.Log("Serial port opened successfully.");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error opening serial port: {ex.Message}");
            }
        }

        Debug.Log($"Normalized Input - Horizontal: {horizontalInput}, Vertical: {verticalInput}");
        Debug.Log($"Normalized Input - previousHorizontal: {previousHinput}, previousVertical: {previousVinput}");

        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                // Check if there is data in the buffer
                if (serialPort.BytesToRead > 0)
                {
                    // Read all available data without blocking
                    string data = serialPort.ReadLine();

                    lock (lockObject)
                    {
                        mostRecentData = data; // Save the most recent data
                    }

                    // Process the data for x and y
                    ProcessInputData(mostRecentData);
                    // if (firstInput && ((Math.Abs(horizontalInput - previousHinput) > inputRadius) || (Math.Abs(verticalInput - previousVinput) > inputRadius)) && OneDart)
                    // {
                    //     Shoot();
                    // }

                    nullShoot = true;

                    lastInputTime = Time.time;

                    if (!firstInput && isShooting)
                    {
                        firstInput = true;
                    }
                }
                else if (Time.time - lastInputTime > inputTimeout) // add another variable for shooting during shooting phase not aim phase
                {
                    Debug.Log("input not caught");
                    if (nullShoot && OneDart && firstInput)
                    {
                        Shoot();
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error reading from serial port: {ex.Message}");
            }
        }
    }
    private void ProcessInputData(string data)
    {
        if (string.IsNullOrEmpty(data))
        { return; }
        // Split the input string into components
        parts = data.Split(',');

        if (parts.Length >= 5)
        {
            // Parse x and y values
            if (float.TryParse(parts[2].Trim(), out float xRaw) &&
                float.TryParse(parts[3].Trim(), out float yRaw))
            {
                if (!hasSetInitialPosition)
                {
                    // Set initial position dynamically
                    initialX = xRaw;
                    initialY = yRaw;
                    hasSetInitialPosition = true;
                }

                // Compute offsets dynamically
                float xTranslated = xRaw - initialX;
                float yTranslated = yRaw - initialY;

                float distX = 40f;
                float distY = 60f;

                // Normalize inputs to range -1:1
                horizontalInput = xTranslated / distX;
                verticalInput = yTranslated / distY;



            }
            else
            {
                Debug.LogError("Error parsing x or y values from input data.");
            }
        }
        else
        {
            Debug.LogError("Input data is not in the expected format.");
        }
    }
    private void ProcessMouseInput()
    {

        // Accumulate raw movement into absolute positions
        absoluteMouseX += Input.GetAxis("Mouse X") * 2f;
        absoluteMouseY += Input.GetAxis("Mouse Y") * 2f;

        if (!hasSetInitialPosition)
        {
            // Set initial position dynamically
            initialX = absoluteMouseX;
            initialY = absoluteMouseY;
            hasSetInitialPosition = true;
            if (!firstInput)
            {
                firstInput = true;
            }
        }

        // Compute offsets dynamically
        float xTranslated = absoluteMouseX - initialX;
        float yTranslated = absoluteMouseY - initialY;

        // Adjust scaling factors to match expected range
        float distX = 40f;  // Adjust to control sensitivity
        float distY = 60f;

        // Normalize inputs to range -1:1
        horizontalInput = xTranslated / distX;
        verticalInput = yTranslated / distY;



        // Debug.Log($"Normalized Input - Horizontal: {horizontalInput}, Vertical: {verticalInput}");
    }

    private void FadeInAndShrink(UnityEngine.UI.Image image, RectTransform rectTransform, float duration)
    {
        image.DOFade(1f, duration); // Fade in

        rectTransform.DOScale(new Vector3(0.012f, 0.012f, 1f), duration) // Shrink smoothly
            .OnComplete(() => fadeComplete = true); // Mark fade as complete when done
    }
    void SetInitialCrosshairPosition()
    {
        Vector3 newPosition = CrossHair.transform.position;
        // newPosition.y -= 1f; // Down by default
        newPosition.z = 2.4f;
        CrossHair.transform.position = newPosition;
        InvisTargetCross.transform.position = newPosition;
    }

    private void InitTimerUI()
    {
        // circleTimer_texture = timerCircle.GetComponent<UnityEngine.UI.Image>();
        // rectTransform = timerCircle.GetComponent<RectTransform>();

        // UnityEngine.Color color = circleTimer_texture.color;
        // color.a = initialOpacity;
        // circleTimer_texture.color = color;

        // rectTransform.localScale = new Vector3(0.012f, 0.012f, 1f);
    }

    void SetupLineRenderer()
    {
        invisVis.startWidth = 0.05f;
        invisVis.endWidth = 0.05f;
        invisVis.material = new Material(Shader.Find("Sprites/Default"));
    }
    void UpdateTimerCircle()
    {
        timeTillShoot -= Time.deltaTime;
        float fill = Mathf.Clamp01(timeTillShoot / 6f);
        circleTimer_texture.fillAmount = fill;
    }

    bool InputExceededThreshold()
    {
        return Mathf.Abs(horizontalInput - previousHinput) > inputRadius ||
               Mathf.Abs(verticalInput - previousVinput) > inputRadius;
    }

    private void SetCrosshairVisible(bool visible)
    {
        // Toggle the LineRenderer
        LineRenderer lr = CrossHair.GetComponent<LineRenderer>();
        if (lr != null)
            lr.enabled = visible;

        // Toggle child UI images
        UnityEngine.UI.Image[] childImages = CrossHair.GetComponentsInChildren<UnityEngine.UI.Image>(includeInactive: true);
        foreach (UnityEngine.UI.Image img in childImages)
        {
            img.enabled = visible;
        }
    }


    private bool returnStarted = false;
    private void HandleCurrentPhase(ExperimentPhase phase)
    {
        switch (phase.name)
        {
            case "ITI":
                if ((dataOutputAndConfig.current_trial - 1) == 1) // for first trial only
                {
                    releaseSerialPort();
                    //visScript.ResetValues();
                    EnableTabletVis(true);
                    StartCoroutine(WaitForReturnHome());
                }
                SetCrosshairVisible(false);
                lastInputTime = Time.time;
                break;

            case "Ready":

                // for first trial return case
                visScript.ResetValues();

                if (conditionIndex % 2 == 0 && !eyeOscillating)
                {
                    StartQuietEye(true);
                    eyeOscillating = true;
                }
                SetCrosshairVisible(true);
                lastInputTime = Time.time;
                break;

            case "Aim":
                isAiming = true;
                if (DiscardOldInput && serialPort.IsOpen)
                {
                    serialPort.DiscardInBuffer();
                    Debug.Log("Discarded buffer for input");
                    DiscardOldInput = false;
                }
                lastInputTime = Time.time;
                if (conditionIndex > 4)
                {
                    StartDistract(true);
                }
                if (generateRandomVec)
                {
                    GenerateRandomInputVector(initAngle);
                    initAngle = false;
                    StartCoroutine(DisableRandomVectorAfterFixedUpdates(50));
                }
                else
                {
                    PreProcessInputData();
                }
                // ProcessMouseInput();

                // MouseMethod();

                NewMethod();

                // OldMethod();

                if (!Mathf.Approximately(Mathf.Sign(previousHinput), 0f))
                {
                    previousHinputSign = Mathf.Sign(previousHinput);
                }
                if (!Mathf.Approximately(Mathf.Sign(previousVinput), 0f))
                {
                    previousVinputSign = Mathf.Sign(previousVinput);
                }

                previousHinput = horizontalInput;
                previousVinput = verticalInput;

                StartCoroutine(YellowFixationDot());

                break;

            // case "Go Cue":
            //     // Optional: visual cue logic
            //     break;

            case "Shoot":
                isAiming = false;
                isShooting = true;
                if (!assignTimetillShoot)
                {
                    timeTillShoot = phase.duration;
                    totalShootTime = phase.duration;
                    assignTimetillShoot = true;
                }

                PreProcessInputData();
                // ProcessMouseInput();

                // MouseMethod();

                NewMethod();

                // OldMethod();


                if (!Mathf.Approximately(Mathf.Sign(previousHinput), 0f))
                {
                    previousHinputSign = Mathf.Sign(previousHinput);
                }
                if (!Mathf.Approximately(Mathf.Sign(previousVinput), 0f))
                {
                    previousVinputSign = Mathf.Sign(previousVinput);
                }

                previousHinput = horizontalInput;
                previousVinput = verticalInput;

                if (Input.GetKeyDown(KeyCode.Mouse0) && OneDart)
                {
                    Shoot();
                }

                StartCoroutine(GreenFixationDot());

                break;

            // case "Feedback":
            //     // Optional: display feedback cue
            //     break;

            case "Return":
                lightFlicker = false;
                distractionLight.SetActive(false);
                
                if (!returnStarted)
                {
                    returnStarted = true;
                    visScript.ResetValues();
                    EnableTabletVis(true);
                }
                releaseSerialPort();
                SetCrosshairVisible(false);

                // Start polling for returnComplete
                StartCoroutine(WaitForReturnHome());

                // if conditions
                Debug.Log("Medialateral List: " + string.Join(", ", MedialateralEyes));
                float stdXeye;
                // GetStd(MedialateralEyes, out stdXeye);
                // if (playOnce && (((conditionIndex % 2 == 0) && stdXeye < 30) || (!(conditionIndex % 2 == 0) && stdXeye > 20)))
                // {
                //     outOfFixationDing.Play();
                //     playOnce = false;
                // }

                break;

            case "End":
                StartCoroutine(RestartScene());
                break;
        }
    }

    public static void GetStd(List<float> data, out float stdDev)
    {
        float mean = data.Average();
        stdDev = 0f;

        if (data == null || data.Count == 0)
            return;

        float sumSqDif = 0f;
        int n = data.Count;

        for (int i = 0; i < n; i++)
        {
            float dif = data[i] - mean;
            sumSqDif += dif * dif;
        }

        stdDev = Mathf.Sqrt(sumSqDif / n);
        Debug.Log("standard deviation of eyes x: " + stdDev);
    }

    void StartQuietEye(bool enable)
    {
        if (enable)
        {
            StartCoroutine(QuietEyeOscillate());
        }
    }

    private bool isFlickering = false; // added by nk
    void StartDistract(bool enable)
    {
        if (enable && !isFlickering) // nk added &&! isFlickering to if statement
        {
            StartCoroutine(flickerLight());
            isFlickering = true;
        }
    }

    IEnumerator QuietEyeOscillate()
    {
        float moveSpeed = -.005f;
        while (true)
        {
            if (Math.Abs(fixationPoint.transform.position.x) >= .47f)
            {
                moveSpeed = -moveSpeed;
            }

            fixationPoint.transform.position = new Vector3(fixationPoint.transform.position.x + moveSpeed, fixationPoint.localPosition.y, fixationPoint.localPosition.z);
            yield return new WaitForSeconds(.01f);
        }
    }

    IEnumerator YellowFixationDot()
    {
        yield return new WaitForFixedUpdate();
        // Make dot green when able to shoot
        MeshRenderer fixationMesh = fixationPoint.GetComponent<MeshRenderer>();
        Material fixationMaterial = fixationMesh.material;
        fixationMaterial.mainTexture = yellowDot;
    }

    IEnumerator GreenFixationDot()
    {
        yield return new WaitForFixedUpdate();
        // Make dot green when able to shoot
        MeshRenderer fixationMesh = fixationPoint.GetComponent<MeshRenderer>();
        Material fixationMaterial = fixationMesh.material;
        fixationMaterial.mainTexture = greenDot;
    }

    private IEnumerator DisableRandomVectorAfterFixedUpdates(int count)
    {
        for (int i = 0; i < count; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        generateRandomVec = false;
    }

    private IEnumerator RestartScene()
    {
        while (hasShot && currentDartRigidbody != null && currentDartRigidbody.velocity.magnitude > 0.1f) // has shot and dart (cylinder) moving
        {
            yield return new WaitForFixedUpdate();
        }
        if (serialPort != null && serialPort.IsOpen)
        {
            serialPort.Close();
            Debug.Log("Serial port closed.");
        }
        // if (eyeLinkManager != null)
        // {
        //     eyeLinkManager.FreeFSAMPLE();
        // }
        if (OnetimeReload)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            OnetimeReload = false;
        }
    }


    public float GetAdjustedTime()
    {
        return Time.time - PlayerPrefs.GetFloat("timeOffset"); // Use this instead of Time.time
    }

    void ResetTime()
    {
        PlayerPrefs.SetFloat("timeOffset", Time.time); // Store the current time as the new "zero"
    }

    private void EnableTabletVis(bool enable)
    {
        tabletVis.SetActive(enable);
    }

    private IEnumerator WaitForReturnHome()
    {
        while (!visScript.returnComplete)
        {
            pausePhasetimer = true;
            yield return null; // wait for next frame
        }

        Debug.Log("Return complete. Unpausing phase timer.");

        pausePhasetimer = false;
        if (currentPhase.name == "Return")
        {
            // Advance to next phase manually after they successfully return home
            dataOutputAndConfig.phaseManager.phaseTimer = 0f;
        }
        EnableTabletVis(false);
    }
    private void releaseSerialPort()
    {
        //Allows other scripts to read from serial port
        if (serialPort != null)
        {
            try
            {
                if (serialPort.IsOpen)
                {
                    serialPort.Close();
                    Debug.Log("Serial port closed successfully for AimShoot.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error closing serial port for AimShoot: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning("SerialPort was null when trying to release.");
        }
    }

    public float flickerHz = 1f; // added by nk
    IEnumerator flickerLight()
    {

        float interval = 1f / (flickerHz * 10f); // added by nk

        while (lightFlicker)
        {
            distractionLight.SetActive(!distractionLight.activeSelf);
            yield return new WaitForSecondsRealtime(interval); // flicker every 0.1 seconds. change back to 0.5f if nk addition fails
        }
    }

    public class LightBlinkTest : MonoBehaviour
    {
        public GameObject distractionLight; 
    }
    // void Start()
    // {
    //     if (lightFlicker)
    //     {
    //         StartCoroutine(flickerLight());
    //     }
    // }

    private void StartExperiment()
    {
        experimentStarted = true;
        Cursor.visible = false;
        // reset clock every block
        Debug.Log("Experiment started with the ' key press.");
    }
}