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
using DG.Tweening; // Import DOTween namespac

public class AimShoot : MonoBehaviour
{

    [SerializeField] Transform CrossHair;
    [SerializeField] Rigidbody CrossRigid;
    [SerializeField] GameObject cylinderPrefab; // Assign the cylinder prefab in the inspector
    [SerializeField] LineRenderer invisVis; // LineRenderer for debugging the invisible vector
    [SerializeField] bool debugInvisibleVector = true; // Toggle visibility of the LineRenderer
    [SerializeField] Transform InvisTargetCross;
    [SerializeField] Rigidbody InvisTargetRigid;
    [SerializeField] DataOutputAndConfig dataOutputAndConfig;

    // Input vars
    private float horizontalInput;
    private float verticalInput;
    
    // Aiming Vars
    private float CrossVelocity;
    private Vector3 AimVector;
    private Vector3 currentVelocity; // Current velocity
    public float acceleration = 5f;        // Acceleration rate
    public float deceleration;        // Deceleration rate
    private float previousHinput;
    private float previousVinput;

    // variables to track initial input position
    private bool hasSetInitialPosition = false;
    private float initialX, initialY;

    // Shooting Vars
    private bool nullShoot = false;
    public float shootSpeed = 10f;    // Speed of the cylinder
    private bool OneDart = true;
    private float inputTimeout = 0.15f;
    private float lastInputTime = 0f;
    private float inputRadius = 0.4f;
    private bool firstInput = false;

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
    private string mostRecentData = ""; // To store the most recent data point
    private object lockObject = new object(); // Lock for thread-safe access

    [SerializeField]
    private string portName = "COM4"; // Replace with your port name
    [SerializeField]
    private int baudRate = 115200;

    // Timer bool
    private float timeTillShoot = 6f;
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

    // object reference for random
    private System.Random random = new System.Random();

    // Mouse vars
    private float absoluteMouseX = 0f; // Persistent absolute X position
    private float absoluteMouseY = 0f; // Persistent absolute Y posit

    // bools
    private bool isAiming = false;

    // Test vars
    private float radialStart = 2.5f;

    
    // Start is called before the first frame update
    void Start()
    {
        // // Initialize the serial port
        // serialPort = new SerialPort(portName, baudRate);
        // serialPort.Parity = Parity.None;
        // serialPort.DataBits = 8;
        // serialPort.StopBits = StopBits.One;
        // serialPort.Handshake = Handshake.None;

        // try
        // {
        //     serialPort.Open();
        //     Debug.Log("Serial port opened successfully.");
        // }
        // catch (System.Exception ex)
        // {
        //     Debug.LogError($"Error opening serial port: {ex.Message}");
        // }

        // Set initial starting position based on trial number
        int trialIndex = (PlayerPrefs.GetInt("trialBlock", 1)) % 4;
        Debug.Log("Trial Index: " + trialIndex);
        Vector3 newPosition = CrossHair.transform.position;

        switch (trialIndex)
        {
            case 0: // Down
                newPosition.y -= radialStart;
                break;
            case 1: // Up
                newPosition.y += radialStart;
                break;
            case 2: // Left
                newPosition.x -= radialStart;
                break;
            case 3: // Right
                newPosition.x += radialStart;
                break;
        }

        CrossHair.transform.position = newPosition;
        InvisTargetCross.transform.position = newPosition;

        // Set up the LineRenderer properties
        invisVis.startWidth = 0.05f;
        invisVis.endWidth = 0.05f;
        invisVis.material = new Material(Shader.Find("Sprites/Default"));
        // Generate the initial invisible vector
        // GenerateInvisibleVector(true); // Generate the initial random vector
        Cursor.visible = false;

        // Get references
        circleTimer_texture = timerCircle.GetComponent<UnityEngine.UI.Image>();
        rectTransform = timerCircle.GetComponent<RectTransform>();

        // Set initial opacity
        UnityEngine.Color color = circleTimer_texture.color;
        color.a = initialOpacity;
        circleTimer_texture.color = color;

        // Set initial scale
        rectTransform.localScale = initialScale;

    }

    // Update is called once per frame
    void Update()
    {
        if (horizontalInput != 0 && verticalInput != 0)
        {
            isAiming = true;
        }
        if (isAiming)
        {
            if (!fadeStarted)
            {
                fadeStarted = true;
                FadeInAndShrink(circleTimer_texture, rectTransform, preShootTime);
            }
            if (fadeComplete)
            {
                // if (bufferTime > 0f)
                // {
                //     bufferTime -= Time.deltaTime;
                // }
                // else
                // {
                //     if (timeTillShoot >= 0f)
                //     {
                //         timeTillShoot -= Time.deltaTime;
                //         float relativeTime = timeTillShoot / 6f;
                //         circleTimer_texture.fillAmount = relativeTime;
                //     }
                //     else
                //     {
                        if (OneDart)
                        {
                            Shoot();
                        }
                //     }
                // }
                // Debug.Log("Undraw now");
            }
        }
    }

    void FixedUpdate()
    {

        // // Read the most recent data and log it
        // if (!serialPort.IsOpen)
        // {
        //     try
        //     {
        //         serialPort.Open();
        //         Debug.Log("Serial port opened successfully.");
        //     }
        //     catch (System.Exception ex)
        //     {
        //         Debug.LogError($"Error opening serial port: {ex.Message}");
        //     }
        // }

        Debug.Log($"Normalized Input - Horizontal: {horizontalInput}, Vertical: {verticalInput}");
        Debug.Log($"Normalized Input - previousHorizontal: {previousHinput}, previousVertical: {previousVinput}");

        // if (serialPort != null && serialPort.IsOpen)
        // {
        //    try
        //     {
        //         // Check if there is data in the buffer
        //         if (serialPort.BytesToRead > 0)
        //         {
        //             // Read all available data without blocking
        //             string data = serialPort.ReadLine();
                    
        //             lock (lockObject)
        //             {
        //                 mostRecentData = data; // Save the most recent data
        //             }
                    
        //             // Process the data for x and y
        //             ProcessInputData(mostRecentData);
        //             if (firstInput && ((Math.Abs(horizontalInput - previousHinput) > inputRadius) || (Math.Abs(verticalInput - previousVinput) > inputRadius)) && OneDart)
        //             {
        //                 Shoot();
        //             }

        //             nullShoot = true;

        //             lastInputTime = Time.time;

        //             if (!firstInput)
        //             {
        //                 firstInput = true;
        //             }
        //         }
        //         else if (Time.time - lastInputTime > inputTimeout )
        //         {
        //             Debug.Log("input not caught");
        //             if (nullShoot && OneDart)
        //             {
        //                 Shoot();
        //             }
        //         }
        //     }
        //     catch (System.Exception ex)
        //     {
        //         Debug.LogError($"Error reading from serial port: {ex.Message}");
        //     }
        // }

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

        if (Input.GetButtonDown("Fire1") && OneDart == true)
        {
            Shoot();
        }

        // horizontalInput = (Input.GetAxis("Horizontal")) * 0.75f;
        // verticalInput = (Input.GetAxis("Vertical")) * 0.75f;

        ProcessMouseInput();

        if (firstInput && ((Math.Abs(horizontalInput - previousHinput) > inputRadius) || (Math.Abs(verticalInput - previousVinput) > inputRadius)) && OneDart)
        {
            Shoot();
        }

        MouseMethod();

        // NewMethod();

        // OldMethod();


        previousHinput = horizontalInput;
        previousVinput = verticalInput;       
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
            AimVector += new Vector3 (CrossHair.position.x, CrossHair.position.y, 0f);

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

        // Apply velocity to the cylinder
        Rigidbody rb = cylinder.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direction * shootSpeed;
        }
        else
        {
            Debug.LogError("No Rigidbody attached to the cylinder prefab!");
        }
        // Start coroutine to reset OneDart after destruction
        StartCoroutine(ResetOneDart(cylinder, 5f));

    }

    // Coroutine to reset OneDart
    IEnumerator ResetOneDart(GameObject cylinder, float delay)
    {
        // Wait for the delay time (same as the destruction time)
        yield return new WaitForSeconds(delay);
        // Close the serial port when the application quits/reloads
        // if (serialPort != null && serialPort.IsOpen)
        // {
        //     serialPort.Close();
        //     Debug.Log("Serial port closed.");
        // }

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Destroy the cylinder
        if (cylinder != null)
        {
            Destroy(cylinder);
        }

        // OneDart = true;
    }
    private void ProcessInputData(string data)
    {
        if (string.IsNullOrEmpty(data))
        {return;}
        // Split the input string into components
        string[] parts = data.Split(',');

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

        

        Debug.Log($"Normalized Input - Horizontal: {horizontalInput}, Vertical: {verticalInput}");
    }

    private void FadeInAndShrink(UnityEngine.UI.Image image, RectTransform rectTransform, float duration)
    {
        image.DOFade(1f, duration); // Fade in

        rectTransform.DOScale(new Vector3(0.012f, 0.012f, 1f), duration) // Shrink smoothly
            .OnComplete(() => fadeComplete = true); // Mark fade as complete when done
    }

}