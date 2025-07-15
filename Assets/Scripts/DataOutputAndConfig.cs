using System.Runtime.InteropServices;
using System;
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
    private string SubjectConfigTextFile;

    // Onetime assignment bools
    private bool doOnce = true;
    private bool firstDataBuffer = true;

    // Switch for arrow coords
    public bool arrowIsNull = true;
    // buffer data for positionally useless phases
    private Dictionary<string, (string, string)> bufferPhases = new Dictionary<string, (string, string)>();
    private readonly List<string> bufferPhaseOrder = new List<string> {  "Return", "ITI" };
    public Phases phaseManager;
    [SerializeField] Transform baseOfTarget;


    void Start()
    {
        subjectNumber = PlayerPrefs.GetInt("subjectTracker", 1);
        subjectID = PlayerPrefs.GetString("subjectID", null);
        SubjectConfigTextFile = PlayerPrefs.GetString("SubjectConfigTextFile");

        InitializeConfiguration();
        SetupTrialData();
        // Read in trial and filename to find row for condition and timing
        FindCurrentTrialInfo(SubjectConfigTextFile);
        LoadSubjectData();

        if (aimShoot.dataRecordingEnabled)
        {
            InitializeCSVHeader();
        }

        if (aimShoot.dataRecordingEnabled)
        {
            if (string.IsNullOrEmpty(subjectID))
            {
                eyeLinkManager.dataFileName = $"{subjectNumber}{current_block}{current_trial}.edf";
            }
            else
            {
                string truncatedID = subjectID.Length >= 6 ? subjectID.Substring(0, 6) : subjectID;
                eyeLinkManager.dataFileName = $"{truncatedID}{current_block}{current_trial}.edf";
            }
            if (PlayerPrefs.GetInt("EyelinkRecording") == 0)
            {
                eyeLinkManager.InitEyeLink();
                eyeLinkManager.AllocateFSAMPLEMemory();
            }
        }

        UpdateTrialData();
    }

    private void InitializeConfiguration()
    {

        directoryPath = "D:/QuietArchery_Stuff/QuietArchery/Data";
        EnsureDirectory(directoryPath);
        if (current_trial == 1)
            PlayerPrefs.SetInt("PulseCount", 20);

        if (!PlayerPrefs.HasKey("max_blocks"))
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
        int maxTrials = 28;
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
                eyeLinkManager.FreeFSAMPLE();
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

    private void FindCurrentTrialInfo(string filename)
    {
        string path = "D:/QuietArchery_Stuff/QuietArchery/ConditionTXTArchery/" + filename;
        if (!File.Exists(path))
        {
            Debug.LogError("Config file needed to proceed with block");
            return;
        }

        try
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string headerLine = reader.ReadLine();
                if (headerLine == null)
                {
                    Debug.LogError("Config file is empty");
                    return;
                }

                string[] headers = headerLine.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                int trialIndex = Array.IndexOf(headers, "trial");
                int itiIndex = Array.IndexOf(headers, "iti");
                int readyIndex = Array.IndexOf(headers, "ready");
                int aimIndex = Array.IndexOf(headers, "aim");
                int conditionNumIndex = Array.IndexOf(headers, "condition_num");
                int conditionTextIndex = Array.IndexOf(headers, "condition_text");

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] tokens = line.Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length < headers.Length)
                        continue;

                    if (int.TryParse(tokens[trialIndex], out int trialNum) && trialNum == current_trial)
                    {
                        phaseManager.ITI_time = float.Parse(tokens[itiIndex]);
                        phaseManager.Ready_time = float.Parse(tokens[readyIndex]);
                        phaseManager.Aim_time = float.Parse(tokens[aimIndex]);
                        phaseManager.conditionNum = int.Parse(tokens[conditionNumIndex]);
                        phaseManager.conditionName = tokens[conditionTextIndex];
                        ApplyCondition(int.Parse(tokens[conditionNumIndex]));
                        aimShoot.conditionIndex = int.Parse(tokens[conditionNumIndex]);

                        Debug.Log($"Trial {trialNum}: ITI={phaseManager.ITI_time}, Ready={phaseManager.Ready_time}, Aim={phaseManager.Aim_time}, CondNum={phaseManager.conditionNum}, CondText={phaseManager.conditionName}");
                        break;
                    }
                }
            }

            phaseManager.CreatePhases();
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error reading trial info: {ex.Message}");
        }
    }

    public void LogDataEntry(string eventType)
    {
        string timestamp = aimShoot.GetAdjustedTime().ToString();

        int pulsesReceived = PlayerPrefs.GetInt("PulseCount");
        pulsesReceived += Input.GetKeyDown(KeyCode.Quote) ? 1 : 0;
        PlayerPrefs.SetInt("PulseCount", pulsesReceived);
        Debug.Log($"Key presses {pulsesReceived}");

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
            case 5:
                cond_type = "Near+Quiet+Distract";
                break;
            case 6:
                cond_type = "Near+Noisy+Distract";
                break;
            case 7:
                cond_type = "Far+Quiet+Distract";
                break;
            case 8:
                cond_type = "Far+Noisy+Distract";
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
                                $"{radialError},{eventType},{cond_type},{tabletX},{tabletY},{forcePressure}";
                                if (eyeLinkManager.currentEyeTrackingData.gx != null &&
                                    eyeLinkManager.currentEyeTrackingData.gy != null &&
                                    eyeLinkManager.eye_used >= 0 &&
                                    eyeLinkManager.currentEyeTrackingData.gx.Length > eyeLinkManager.eye_used &&
                                    eyeLinkManager.currentEyeTrackingData.gy.Length > eyeLinkManager.eye_used)
                                {
                                    bufferData += $",{eyeLinkManager.currentEyeTrackingData.gx[eyeLinkManager.eye_used]},{eyeLinkManager.currentEyeTrackingData.gy[eyeLinkManager.eye_used]}";
                                }
                                else
                                {
                                    bufferData += ",{},{}";
                                }
                                bufferData += $",{pulsesReceived}\n";
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
                      $"{radialError},{eventType},{cond_type},{tabletX},{tabletY},{forcePressure}";
                        if (eyeLinkManager.currentEyeTrackingData.gx != null &&
                            eyeLinkManager.currentEyeTrackingData.gy != null &&
                            eyeLinkManager.eye_used >= 0 &&
                            eyeLinkManager.currentEyeTrackingData.gx.Length > eyeLinkManager.eye_used &&
                            eyeLinkManager.currentEyeTrackingData.gy.Length > eyeLinkManager.eye_used)
                        {
                            data += $",{eyeLinkManager.currentEyeTrackingData.gx[eyeLinkManager.eye_used]},{eyeLinkManager.currentEyeTrackingData.gy[eyeLinkManager.eye_used]}";
                        }
                        else
                        {
                            data += ",{},{}";
                        }
                        data += $",{pulsesReceived}\n";

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
        string header = "SubjectID,Trial#,Block#,Timestamp,Arrow X,Arrow Y,Arrow Z,Crosshair X position,Crosshair Y position,Center-Target X position,Center-Target Y position,Radial Error,Event,Condition,tabletX,tabletY,Force,EyeX,EyeY,PulseReceived\n";

        // Check if it’s the first trial and block, then add the header if necessary
        if (current_trial == 1 && !File.Exists(csvPath))
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
            eyeLinkManager.StartRecording();
            doOnce = false;
        }

        if (PlayerPrefs.GetInt("EyelinkRecording") == 1)
        {
            // Definitely will have to write a bool here to ensure we don't do this if not connected to eyelink
            eyeLinkManager.PollEyelink();
            // Debug.Log(
            //     $"Gaze: (gx={eyeLinkManager.currentEyeTrackingData.gx[eyeLinkManager.eye_used]}, gy={eyeLinkManager.currentEyeTrackingData.gy[eyeLinkManager.eye_used]}, pa={eyeLinkManager.currentEyeTrackingData.pa[eyeLinkManager.eye_used]})"
            //     );
        }
        // eyeLinkManager.currentEyeTrackingData.gx[0] -> add to medialateraleyes list
        if (aimShoot.isAiming)
        {
            try //Get rid of error messages with this one simple trick!
            {
                FSAMPLE eyeData = eyeLinkManager.currentEyeTrackingData; //Or something that gets the correct data

                aimShoot.MedialateralEyes.Add(eyeData.gx[0]);
            }
            catch { }
        }
    }

    void FixedUpdate()
    {
        
    }
    void ApplyCondition(int condition_num)
    {
        Debug.Log($"Applying Condition {condition_num}");

        // Reset all variables first (e.g. Quiet Eye back to 0, etc.)
        StopAllCoroutines(); // Stop QE shifting if active

        switch (condition_num)
        {
            case 1: // Near, no distraction, Quiet
                // EnableScoringTarget(true);
                break;

            case 2: // Near, no distraction, Noisy
                // EnableScoringTarget(true);
                break;

            case 3: // Far, no distraction, Quiet
                ShrinkTarget();
                // EnableScoringTarget(true);
                break;

            case 4: // Far, no distraction, Noisy
                ShrinkTarget();
                // EnableScoringTarget(true);
                break;

            case 5: // Near, Distraction, Quiet
                break;

            case 6: // Near, Distraction, Noisy
                break;

            case 7: // Far, Distraction, Quiet
                ShrinkTarget();
                break;

            case 8: // Far, Distraction, Noisy
                ShrinkTarget();
                break;
        }
    }
    void ShrinkTarget()
    {
        foreach (Transform child in baseOfTarget)
        {
            child.localScale *= 0.5f;
        }
    }
}