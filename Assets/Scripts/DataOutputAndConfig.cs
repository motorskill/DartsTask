using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Ports;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class DataOutputAndConfig : MonoBehaviour
{
    [SerializeField] private AimShoot aimShoot;
    public EyeLinkManager eyeLinkManager;
    
    private int subjectNumber;
    private string subjectID = null;
    public int current_trial;
    public int current_block;

    // Paths for file management
    private string csvPath;
    private string directoryPath;

    // Onetime assignment bools
    private bool doOnce = true;
    private bool firstDataBuffer = true;

    // Switch for arrow coords
    public bool arrowIsNull = true;
    // buffer data for positionally useless phases
    private Dictionary<string, (string, string)> bufferPhases = new Dictionary<string, (string, string)>();
    private readonly List<string> bufferPhaseOrder = new List<string> { "ITI", "Return" };


    void Start()
    {
        subjectNumber = PlayerPrefs.GetInt("subjectTracker", 1);
        subjectID = PlayerPrefs.GetString("subjectID", null);

        InitializeConfiguration();
        SetupTrialData();
        LoadSubjectData();

        if (aimShoot.dataRecordingEnabled)
        {
            InitializeCSVHeader();
        }

        if (aimShoot.dataRecordingEnabled)
        {
            if (string.IsNullOrEmpty(subjectID))
            {
                eyeLinkManager.dataFileName = $"{subjectNumber}{current_block}{current_trial - 1}.edf";
            }
            else
            {
                string truncatedID = subjectID.Length >= 4 ? subjectID.Substring(0, 4) : subjectID;
                eyeLinkManager.dataFileName = $"{truncatedID}{current_block}{current_trial - 1}.edf";
            }
            eyeLinkManager.InitEyeLink();
        }

        UpdateTrialData();
        eyeLinkManager.AllocateFSAMPLEMemory();
    }

    private void InitializeConfiguration()
    {

        directoryPath = "D:/QuietArchery_Stuff/QuietArchery/Data";
        EnsureDirectory(directoryPath);

        if (!PlayerPrefs.HasKey("max_trials"))
        {
            PlayerPrefs.SetInt("max_trials", 4);
            PlayerPrefs.SetInt("max_blocks", 2);
            PlayerPrefs.SetString("subjectID", "");
            PlayerPrefs.SetInt("overwrite", 0);
            PlayerPrefs.SetInt("current_trial", 1);
            PlayerPrefs.SetInt("trialBlock", 1);
        }
    }

    private void LoadSubjectData()
    {
        if (string.IsNullOrEmpty(subjectID))
        {
            csvPath = $"{directoryPath}/Subject{subjectNumber}.csv";
        }
        else
        {
            csvPath = $"{directoryPath}/{subjectID}.block.{current_block}.csv";
        }
        Debug.Log("Subject data loaded: " + (subjectID ?? subjectNumber.ToString()));
    }

    private void SetupTrialData()
    {
        current_trial = PlayerPrefs.GetInt("current_trial", 1);
        current_block = PlayerPrefs.GetInt("current_block", 1);
    }

    private void UpdateTrialData()
    {
        int maxTrials = PlayerPrefs.GetInt("max_trials");
        int selectedBlock = PlayerPrefs.GetInt("max_blocks");

        if (current_trial > maxTrials)
        {
            PlayerPrefs.DeleteAll();
            if (aimShoot.serialPort != null && aimShoot.serialPort.IsOpen)
            {
                aimShoot.serialPort.Close();
                Debug.Log("Serial port closed.");
            }
            if (eyeLinkManager != null)
            {
                eyeLinkManager.StopRecording();
                EyelinkCoreInterop.close_eyelink_connection();
            }
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            Application.Quit();
            #endif
            Debug.Log("Max trials reached, resetting and exiting.");
            return;
        }
        else
        {
            current_trial++;
        }
        PlayerPrefs.SetInt("current_trial", current_trial);
        PlayerPrefs.SetInt("current_block", selectedBlock);
        Debug.Log($"Updated to Trial {current_trial}, Block {selectedBlock}");
    }

    public void LogDataEntry(string eventType)
    {
        string timestamp = aimShoot.GetAdjustedTime().ToString();

        // Target center is defined by featureTarget.transform.position ... (0,0,0)

        Vector3 arrowLocal = Vector3.zero;
        if (!arrowIsNull)
        {
            arrowLocal = aimShoot.arrowCoords.position - aimShoot.featureTarget.transform.position;
        }

        Vector3 crossHairLocal = aimShoot.CrossHair.position - aimShoot.featureTarget.transform.position;

        Vector2 target2D = new Vector2(0f, 0f); // Now the origin
        Vector2 arrow2D = new Vector2(arrowLocal.x, arrowLocal.y);
        float radialError = Vector2.Distance(target2D, arrow2D);


        int condition = aimShoot.conditionIndex;
        /*
        Conditions associated with different states
        1: Near, Quiet
        2: Near, Noisy
        3: Far, Quiet
        4: Far, Noisy
        */
        string cond_type = "";
        switch (condition)
        {
            case 1:
                cond_type = "Near+Quiet";
                break;

            case 2:
                cond_type = "Near+Noisy";
                break;

            case 3:
                cond_type = "Far+Quiet";
                break;

            case 4:
                cond_type = "Far+Noisy";
                break;
        }

        float forcePressure; float tabletY; float tabletX;
        try
        {
            float.TryParse(aimShoot.parts[4], out forcePressure);
            float.TryParse(aimShoot.parts[3], out tabletY);
            float.TryParse(aimShoot.parts[2], out tabletX);

        }
        catch
        {
            forcePressure = 0f;
            tabletY = 0f;
            tabletX = 0f;
        }

        if (eventType == "Shoot")
        {
            if (!arrowIsNull)
            {
                eventType = "Shoot (post-shot)";
            }
            else
            {
                eventType = "Shoot (pre-shot)";
            }
        }
        else if (bufferPhaseOrder.Contains(eventType))
        {
            string bufferData = $"{subjectID}," +
                                $"{current_trial - 1},{current_block},{timestamp}," +
                                $"{arrowLocal.x},{arrowLocal.y},{arrowLocal.z}," +
                                $"{crossHairLocal.x},{crossHairLocal.y}," +
                                $"0,0," +
                                $"{radialError},{eventType},{cond_type},{tabletX},{tabletY},{forcePressure}\n";

            if (!bufferPhases.ContainsKey(eventType))
            {
                // First time seeing this buffer phase
                bufferPhases[eventType] = (bufferData, bufferData); // first and last are the same for now
            }
            else
            {
                var current = bufferPhases[eventType];
                bufferPhases[eventType] = (current.Item1, bufferData); // update only the last
            }

            return;
        }

        string data = $"{subjectID}," +
                      $"{current_trial - 1},{current_block},{timestamp}," +
                      $"{arrowLocal.x},{arrowLocal.y},{arrowLocal.z}," +
                      $"{crossHairLocal.x},{crossHairLocal.y}," +
                      $"0,0," + // Target is origin
                      $"{radialError},{eventType},{cond_type},{tabletX},{tabletY},{forcePressure}\n";

        var orderedBufferData = bufferPhaseOrder
            .Where(bufferPhases.ContainsKey)
            .SelectMany(phase =>
            {
                var tuple = bufferPhases[phase];
                return new[] { tuple.Item1, tuple.Item2 };
            });


        if (orderedBufferData.Any())
        {
            File.AppendAllText(csvPath, string.Join("", orderedBufferData));
            bufferPhases.Clear();
        }

        File.AppendAllText(csvPath, data);
        Debug.Log("Data entry logged.");
    }

    private void InitializeCSVHeader()
    {
        // Define the header line for the CSV file
        string header = "SubjectID,Trial#,Block#,Timestamp,Arrow X,Arrow Y,Arrow Z,Crosshair X position,Crosshair Y position,Center-Target X position,Center-Target Y position,Radial Error,Event,Condition,tabletX,tabletY,Force\n";
        
        // Check if it’s the first trial and block, then add the header if necessary
        if (current_trial == 1   && !File.Exists(csvPath))
        {
            File.AppendAllText(csvPath, header);
            Debug.Log("CSV header initialized");
        }
    }
    private void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    void Update()
    {
        if (doOnce && PlayerPrefs.GetInt("EyelinkRecording") == 0)
        {
            // Don't make new files
            PlayerPrefs.SetInt("EyelinkRecording", 1);
            eyeLinkManager.StartRecording();
            doOnce = false;
        }

        // Definitely will have to write a bool here to ensure we don't do this if not connected to eyelink
        eyeLinkManager.PollEyelink();
        Debug.Log(
            $"Gaze: ({eyeLinkManager.currentEyeTrackingData.gx[0]}, {eyeLinkManager.currentEyeTrackingData.gy[0]})"
            );
    }
}