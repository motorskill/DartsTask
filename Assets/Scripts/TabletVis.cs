using System.IO.Ports;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TabletVis : MonoBehaviour
{
    // Serial port vars
    public SerialPort serialPort;
    private string mostRecentData = ""; // To store the most recent data point
    private object lockObject = new object(); // Lock for thread-safe access

    [SerializeField]
    private string portName = "COM4"; // Replace with your port name
    [SerializeField]
    private int baudRate = 115200;
    private string[] parts;

    // screen vars
    private const float UNITY_X_MAX = 5.132F; //Maximum X relative to camera before exiting view on the focal plane
    private const float UNITY_X_MIN = -5.132F; //Minimum X relative to camera before exiting view on the focal plane
    private const float UNITY_Y_MAX = 2.88675F; //Maximum Y relative to camera before exiting view on the focal plane
    private const float UNITY_Y_MIN = -2.88675F; //Minimum Y relative to camera before exiting view on the focal plane
    private const float RAW_X_MIN = 0f;
    private const float RAW_X_MAX = 120.0f;
    private const float RAW_Y_MIN = 0f;
    private const float RAW_Y_MAX = 100f;

    // other object to serve as  "home"
    [SerializeField] GameObject Home;
    // Timer tracking
    private float overlapTimer = 0f;
    private bool isOverlapping = false;
    private float requiredOverlapTime = 1f;  // seconds
    public bool returnComplete = false;

    // Start is called before the first frame update
    void Start()
    {
        serialPort = new SerialPort(portName, baudRate);
        serialPort.Parity = Parity.None;
        serialPort.DataBits = 8;
        serialPort.StopBits = StopBits.One;
        serialPort.Handshake = Handshake.None;

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
    }

    // Update is called once per frame
    void Update()
    {
        if (!returnComplete)
        {
            PreProcessInputData();

            if (isOverlapping)
            {
                overlapTimer += Time.deltaTime;
                if (overlapTimer >= requiredOverlapTime)
                {
                    if (serialPort != null && serialPort.IsOpen)
                    {
                        serialPort.Close();
                        Debug.Log("Serial port closed.");
                    }
                    returnComplete = true;
                    Debug.Log("Returned to home for sufficient time. Phase complete!");
                    // You can trigger your next event/transition here
                }
            }
            else
            {
                overlapTimer = 0f; // Reset timer if not continuously overlapping
            }
        }
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
                // Normalize tablet raw coords (0 to 1 range)
                float xNorm = Mathf.InverseLerp(RAW_X_MIN, RAW_X_MAX, xRaw);
                float yNorm = Mathf.InverseLerp(RAW_Y_MIN, RAW_Y_MAX, yRaw);

                // Map to Unity scene bounds
                float fullX = Mathf.Lerp(UNITY_X_MIN, UNITY_X_MAX, xNorm);
                float fullY = Mathf.Lerp(UNITY_Y_MIN, UNITY_Y_MAX, yNorm);


                Debug.Log("Gaze X coord: " + fullX + " | Gaze Y coord: " + fullY);

                transform.localPosition = new Vector3(fullX, fullY, 0f);
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
    
    // Overlap detection
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject == Home)
        {
            Debug.Log("OVERLAPPING");
            isOverlapping = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == Home)
        {
            Debug.Log("stopped overlapping");
            isOverlapping = false;
        }
    }
}
