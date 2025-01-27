using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;
using UnityEngine;

public class AimShoot : MonoBehaviour
{

    [SerializeField] Transform CrossHair;
    [SerializeField] Rigidbody CrossRigid;
    [SerializeField] GameObject cylinderPrefab; // Assign the cylinder prefab in the inspector
    [SerializeField] LineRenderer invisVis; // LineRenderer for debugging the invisible vector
    [SerializeField] bool debugInvisibleVector = true; // Toggle visibility of the LineRenderer

    // Aiming Vars
    private float CrossVelocity;
    private Vector3 AimVector;
    private Vector3 currentVelocity; // Current velocity
    public float acceleration = 5f;        // Acceleration rate
    public float deceleration;        // Deceleration rate
    private float previousHinput;
    private float previousVinput;

    // Shooting Vars
    public float shootSpeed = 10f;    // Speed of the cylinder
    private bool OneDart = true;

    // Random influence
    private Vector3 invisibleVector; // The "invisible force" vector
    private Vector3 targetVector;    // Target direction for the next shift
    private float currentVectorAngle;
    public float angularRange = 90f; // Maximum angular range (degrees) for shifts
    public float shiftSpeed = 5f;    // Speed of the vector's rotation
    public float forceMagnitude; // Magnitude of the invisible force
    private float timeSinceLastShift = 0f; // Timer for periodic updates
    public float shiftInterval = 2f;  // Time interval for changing the vector

    
    // Start is called before the first frame update
    void Start()
    {
        // Set up the LineRenderer properties
        invisVis.startWidth = 0.05f;
        invisVis.endWidth = 0.05f;
        invisVis.material = new Material(Shader.Find("Sprites/Default"));
        // Generate the initial invisible vector
        GenerateInvisibleVector(true); // Generate the initial random vector
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate()
    {

        Debug.Log("Target Vector = " + targetVector);
        timeSinceLastShift += Time.fixedDeltaTime;

        // Periodically update the target vector
        if (timeSinceLastShift >= shiftInterval)
        {
            GenerateInvisibleVector(false);
            timeSinceLastShift = 0f;
            // // Smoothly rotate the invisible vector towards the target vector
            // invisibleVector = Vector3.Lerp(invisibleVector, targetVector, shiftSpeed * Time.fixedDeltaTime);
            
        }

        // Smoothly rotate the invisible vector towards the target vector
        invisibleVector = Vector3.Lerp(invisibleVector, targetVector, shiftSpeed * Time.fixedDeltaTime).normalized * forceMagnitude;

        if (Input.GetButtonDown("Fire1") && OneDart == true)
        {
            Shoot();
        }

        float horizontalInput = (Input.GetAxis("Horizontal")) * 0.75f;
        float verticalInput = (Input.GetAxis("Vertical")) * 0.75f;


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
            // Generate an initial random vector
            invisibleVector = Random.insideUnitCircle.normalized * forceMagnitude;
        }
        else
        {
            // Get the current angle of the invisible vector
            currentVectorAngle = Mathf.Atan2(invisibleVector.y, invisibleVector.x) * Mathf.Rad2Deg;

            // Calculate a random angle shift within the range
            float angleShift = Random.Range(-angularRange, angularRange);
            float newAngle = currentVectorAngle + angleShift;

            // Convert the new angle to a direction vector
            float radians = newAngle * Mathf.Deg2Rad;
            targetVector = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f).normalized * forceMagnitude;
        }

        Debug.Log($"Generated Invisible Vector: {invisibleVector}, Target Vector: {targetVector}");
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

        // Activate the GameObject (make it visible and functional)
        cylinder.SetActive(true);

        // Get the direction towards the target
        Vector3 direction = (CrossHair.position - Camera.main.transform.position).normalized;

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

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

        // Destroy the cylinder
        if (cylinder != null)
        {
            Destroy(cylinder);
        }

        // OneDart = true;
    }
}
