using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor; // To handle the exit functionality in the editor
#endif
using UnityEngine.SceneManagement;
using UnityEngine;
using Unity.VisualScripting;

// Handles data output and configuration for a Unity project
public class DataOutputAndConfig : MonoBehaviour
{
    
    // Serialized fields for Unity Inspector configuration
    // [SerializeField] private LineForceController LineForceController;
    // [SerializeField] private CapsuleCollider Hole; // Reference to the hole collider
    // [SerializeField] private Rigidbody ballRigidbody; // Rigidbody of the ball
    // [SerializeField] private Transform PivotHitBox; // Pivot point for club hitbox
    // [SerializeField] private Transform PivotPerspective; // Pivot point for perspective
    [SerializeField] private AimShoot aimShoot;
    
    // Paths for file management
    private string filePath;
    private string csvPath;
    private string configPath;
    private string trialblock_tracker;
    private string trials_used;
    private string subject_tracker;
    private string testPath;

    // Subject and trial information
    private int subjectNumber;
    private string subjectID = null;
    private string directoryPath;
    private string resourcePath;

    // Trial and difficulty-related variables
    // public bool hard_shot; // Indicates if the shot is considered "hard"
    public int current_trial; // Current trial number
    public int current_block; // Current block number
    // public float hard_radius = 150f; // Radius for a hard trial
    // public float easy_radius = 0f; // Radius for an easy trial
    // public float hard_deviation = 0f; // Deviation for a hard trial
    // public float easy_deviation = 0f; // Deviation for an easy trial
    
    // Test position and placeholder variables
    // private Vector3 testPos;

    // Start is called before the first frame update
    void Start()
    {
        subjectNumber = 1;

        // Perform all setup steps in order

        // Step 1
        InitializeConfiguration();

        // Step 2
        SetupTrialData();
        LoadSubjectData();

        // if (LineForceController.dataRecordingEnabled)
        // {
        //     // Step 3
        //     InitializeCSVHeader();
        // }
        
        // // Step 4
        // SetupTrialPositions();

        // Step 5
        UpdateTrialData();

        // // Step 6
        // AssignTrialPositions();
    }

    // Class to manage trial operations such as tracking and resetting
    private class TrialManager
    {
        // Get the file path for trials used
        public static string GetTrialsUsedFilePath()
        {
            #if UNITY_EDITOR
                string TrialsUsedPath = Application.dataPath + "/Resources/trials_used.txt";
                if (!File.Exists(TrialsUsedPath))
                {
                    // Create the file
                    using (FileStream fs = File.Create(TrialsUsedPath)); // Create file if it doesn't exist
                }
                return TrialsUsedPath;
            #else
                string backend = "QuietArcheryBackend";
                string TrialsUsedPath = "D:/QuietArchery_Stuff" + "/" + backend + "/Resources/trials_used.txt";
                if (!File.Exists(TrialsUsedPath))
                {
                    // Create the file
                    using (FileStream fs = File.Create(TrialsUsedPath)); // Create file if it doesn't exist
                }
                return TrialsUsedPath;
            #endif
        }

        // Check if a specific trial has already been used
        public static bool IsTrialUsed(int trial)
        {
            string trialsUsedFilePath = GetTrialsUsedFilePath();
            string[] trialsUsed = ParseFile(trialsUsedFilePath);
            return trialsUsed.Contains(trial.ToString());
        }

        // Add a trial to the "trials used" list
        public static void AddTrial(int trial)
        {
            string trialsUsedFilePath = GetTrialsUsedFilePath();
            string[] newTrial = { trial.ToString() };
            WriteArrayToFile(trialsUsedFilePath, newTrial, true); // Append the new trial to the file
            Debug.Log("Trial added: " + trial);
        }

        // Clear all recorded trials
        public static void ClearTrials()
        {
            string trialsUsedFilePath = GetTrialsUsedFilePath();
            if (File.Exists(trialsUsedFilePath))
            {
                File.Delete(trialsUsedFilePath); // Delete the file
            }
        }
    }

    // Parse file content into a string array
    public static string[] ParseFile(string fileName)
    {
        // Ensure the file is in the Resources folder and load it without the extension
        string fileContents = File.ReadAllText(fileName);
        if (fileContents != null)
        {
            // Split the contents by commas
            string[] tokens = fileContents.Split(',');

            return tokens;
        }
        else
        {
            Debug.LogError("File not found!");
            return new string[0];
        }
    }

    // Write an array of strings to a file
    public static void WriteArrayToFile(string filePath, string[] tokens, bool append)
    {

        using (StreamWriter writer = new StreamWriter(filePath, append))
        {
            if (tokens == null || tokens.Length == 0)
            {
                Debug.Log("Array is empty or null.");
                return;
            }

            // Join the tokens with a comma delimiter
            string fileContents = string.Join(",", tokens);

            // Write the contents to the file
            writer.Write(fileContents);

            Debug.Log("File written successfully.");
        }
    }

    // Generate a random position on a circle
    Vector3 GenerateRandomPositionOnCircle(Vector3 center, float radius, float deviation)
    {
        // Generate a random angle in radians
        float randomAngle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);

        // Calculate the position on the circle
        float x = Mathf.Cos(randomAngle) * radius;
        float z = Mathf.Sin(randomAngle) * radius;

        // Add deviation
        x += UnityEngine.Random.Range(-deviation, deviation);
        z += UnityEngine.Random.Range(-deviation, deviation);

        // Create the position vector
        Vector3 position = new Vector3(x, 0, z) + center;

        return position;
    }

    // Check if a string is an integer
    public static bool IsInteger(string input)
    {
        int result;
        return int.TryParse(input, out result); // Try parsing the input
    }

    // Delete rows from a CSV file based on a threshold
    void DeleteRowsFromCSV(string csvFilePath, int threshold)
    {
        // Read all lines from the CSV file
        var lines = File.ReadAllLines(csvFilePath);

        // Filter out the rows where the second column is greater than or equal to the threshold
        var filteredLines = lines.Where((line, index) =>
        {
            // Skip header row
            if (index == 0) return true;

            var columns = line.Split(',');
            if (int.TryParse(columns[1], out int value))
            {
                return value < threshold;
            }
            return true; // If unable to parse, keep the line
        });

        // Write the filtered lines back to the CSV file
        File.WriteAllLines(csvFilePath, filteredLines);
    }

    // Ensure a directory exists, creating it if necessary
    private void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    // Ensure a file exists, optionally initializing with default content
    private void EnsureFile(string path, string[] defaultContent = null)
    {
        if (!File.Exists(path))
        {
            using (FileStream fs = File.Create(path)) { }
            WriteArrayToFile(path, defaultContent, false);
        }
    }

    private void InitializeConfiguration()
    {
        // Set base paths based on editor mode or runtime mode
        directoryPath = Application.isEditor ? Application.dataPath + "/Data" : "D:/QuietArchery_Stuff/QuietArcheryBackend/Data";
        resourcePath = Application.isEditor ? Application.dataPath + "/Resources" : "D:/QuietArchery_Stuff/QuietArcheryBackend/Resources";

        // Ensure directories
        EnsureDirectory(directoryPath);
        EnsureDirectory(resourcePath);

        // Define paths and default content for files
        configPath = resourcePath + "/test_config.txt";
        // testPath = resourcePath + "/trial_coords.txt";
        subject_tracker = resourcePath + "/subject_tracker.txt";
        trialblock_tracker = resourcePath + "/trial_block_track.txt";
        trials_used = resourcePath + "/trials_used.txt";

        // Ensure necessary files are created with default content
        EnsureFile(configPath, new string[] { "trials", "2", "blocks", "2", "subject", "", "overwrite" });
        // EnsureFile(testPath);
        EnsureFile(trialblock_tracker, new string[] { "1", "1" });
        EnsureFile(subject_tracker, new string[] { "0" });
        EnsureFile(trials_used, new string[] { "" });
    }

    private void LoadSubjectData()
    {
        // Load subject data from the tracker and config files
        string[] subjectNumData = ParseFile(subject_tracker);
        string[] testConfigData = ParseFile(configPath);

        // Determine subject number or ID based on tracker and config file content
        if (subjectNumData[0] == "")
        {
            if (testConfigData[5] == "next")
            {
                // Start with subject number 1, increment if it exists
                subjectNumber = 1;
                csvPath = $"{directoryPath}/Subject{subjectNumber}.csv";
                while (File.Exists(csvPath))
                {
                    subjectNumber++;
                    csvPath = $"{directoryPath}/Subject{subjectNumber}.csv";
                }
                filePath = $"{directoryPath}/Subject{subjectNumber}.txt";
            }
            else if (!int.TryParse(testConfigData[5], out subjectNumber))
            {
                // Set subject ID from config if it's not an integer
                subjectID = testConfigData[5];
            }
        }
        else
        {
            // Parse subject number or ID from subject tracker
            if (!int.TryParse(subjectNumData[0], out subjectNumber))
            {
                subjectID = subjectNumData[0];
            }
        }

        // Determine file paths based on subject ID or number
        if (subjectID == null)
        {
            filePath = $"{directoryPath}/Subject{subjectNumber}.txt";
            csvPath = $"{directoryPath}/Subject{subjectNumber}.csv";
        }
        else
        {
            filePath = $"{directoryPath}/Subject{subjectID}.txt";
            csvPath = $"{directoryPath}/{subjectID}.csv";
        }

        Debug.Log("Subject data loaded and paths set");
    }



    private void SetupTrialData()
    {
        // Parse the trial and block numbers from the tracker file
        string[] trialBlockData = ParseFile(trialblock_tracker);
        string[] testConfigData = ParseFile(configPath);

        // Set current trial and block from tracker or initialize if missing
        if (trialBlockData.Length >= 2)
        {
            current_trial = int.Parse(trialBlockData[0]);
            current_block = int.Parse(trialBlockData[1]);
        }
        else
        {
            current_trial = 1;
            current_block = 1;
        }

        // Check if test configuration specifies a starting block and trial override
        if (testConfigData.Length > 7 && IsInteger(testConfigData[7]))
        {
            current_block = int.Parse(testConfigData[7]);
            current_trial = 1;  // Reset trial to start of new block
            DeleteRowsFromCSV(csvPath, current_block);  // Remove prior data for this block

            // Update configuration file to exclude override after applying
            string[] updatedConfig = testConfigData.Take(7).ToArray();
            WriteArrayToFile(configPath, updatedConfig, false);
        }

        Debug.Log($"Trial data initialized: Block {current_block}, Trial {current_trial}");
    }

    private void InitializeCSVHeader()
    {
        string[] trialBlockData = ParseFile(trialblock_tracker);

        current_trial = int.Parse(trialBlockData[0]);
        current_block = int.Parse(trialBlockData[1]);

        // Define the header line for the CSV file
        string header = "Trial#,Block#,Timestamp,Ball X,Ball Z,Clubhead X position,Clubhead Y position,Clubhead Z position,Club X rotation,Club Z rotation,Clubhead Y rotation,Hole x position,Hole z position,Radial Error,Event,Difficulty\n";
        
        // Check if it’s the first trial and block, then add the header if necessary
        if (current_trial == 1 && current_block == 1 && !File.Exists(csvPath))
        {
            File.AppendAllText(csvPath, header);
            Debug.Log("CSV header initialized");
        }
    }

    // private void SetupTrialPositions()
    // {
    //     // Parse configuration and determine the number of trials
    //     string[] testConfig = ParseFile(configPath);
    //     int numTrials = int.Parse(testConfig[1]);

    //     // Parse existing trial coordinates to check if they need to be generated
    //     string[] coordinates = ParseFile(testPath);
    //     if (coordinates[0] == "" && coordinates[^1] != "done")
    //     {
    //         // Generate trial positions if none exist
    //         Vector3 holePosition = Hole.transform.TransformPoint(Hole.center);
    //         using (StreamWriter writer = new StreamWriter(testPath, false))
    //         {
    //             for (int i = 0; i < numTrials; i++)
    //             {
    //                 Vector3 trialPosition;
    //                 if (i % 2 == 0)
    //                 {
    //                     trialPosition = GenerateRandomPositionOnCircle(holePosition, hard_radius, hard_deviation);
    //                 }
    //                 else
    //                 {
    //                     trialPosition = GenerateRandomPositionOnCircle(holePosition, easy_radius, easy_deviation);
    //                 }

    //                 // Write the generated position to the file
    //                 writer.Write($"{trialPosition.x},{trialPosition.z},");
    //             }
    //             writer.Write("done"); // Indicate completion
    //         }
    //     }

    //     Debug.Log("Trial positions generated and written to file.");
    // }


    private void UpdateTrialData()
    {
        // Parse maximum trials and blocks from the configuration
        string[] testConfig = ParseFile(configPath);
        int maxTrials = int.Parse(testConfig[1]);
        int maxBlocks = int.Parse(testConfig[3]);

        // Check if the current trial exceeds the maximum trials for the block
        if (current_trial > maxTrials)
        {
            current_trial = 2; // Reset trial to the first one
            if (current_block > 1)
            {
                TrialManager.ClearTrials();
            }
            current_block++;    // Move to the next block
        }
        else
        {
            current_trial++; // Proceed to the next trial
        }

        // Check if the current block exceeds the maximum blocks
        if (current_block > maxBlocks)
        {
            // Reset the tracker files to initial values
            WriteArrayToFile(trialblock_tracker, new string[] { "1", "1" }, false);
            WriteArrayToFile(subject_tracker, new string[] { "0" }, false);

            // Clear the configuration file if needed
            if (testConfig.Length > 7)
            {
                testConfig[5] = ""; // Clear subject ID in config
                string[] updatedConfig = testConfig.Take(7).ToArray();
                WriteArrayToFile(configPath, updatedConfig, false);
            }
            else
            {
                testConfig[5] = "";
                WriteArrayToFile(configPath, testConfig, false);
            }

            // Clear any existing trial positions in `testPath`
            // WriteArrayToFile(testPath, new string[] { "" }, false);
            TrialManager.ClearTrials();
            // Exit application or stop play mode in editor
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif

            Debug.Log("Reached max blocks, resetting trial and block data. Exiting...");
            return; // End function early if quitting
        }

        // Update tracker file with new trial and block numbers
        WriteArrayToFile(trialblock_tracker, new string[] { current_trial.ToString(), current_block.ToString() }, false);

        Debug.Log($"Updated to Trial {current_trial}, Block {current_block}");
    }

    // private void AssignTrialPositions()
    // {
    //     Vector3 holePosition = Hole.transform.TransformPoint(Hole.center);
    //     string[] coordinates = ParseFile(testPath);
    //     string[] testConfig = ParseFile(configPath);
    //     int maxTrials = int.Parse(testConfig[1]);
    //     int current_trial_copy = current_trial;
    //     System.Random rnd = new System.Random();

    //     do
    //     {
    //         current_trial_copy = rnd.Next(2, maxTrials + 2);
    //     } while (TrialManager.IsTrialUsed(current_trial_copy));

    //     TrialManager.AddTrial(current_trial_copy); // Mark this trial number as used
    //     int trialIndex = current_trial_copy - 2;
    //     int xCoordIndex = 2 * trialIndex;
    //     int zCoordIndex = xCoordIndex + 1;

    //     // Ensure the trialIndex is within bounds and the strings are valid floats
    //     if (trialIndex >= 0 && trialIndex + 1 < coordinates.Length &&
    //         float.TryParse(coordinates[xCoordIndex], out float xCoordinate) &&
    //         float.TryParse(coordinates[zCoordIndex], out float zCoordinate))
    //     {
    //         testPos = new Vector3(xCoordinate, 5.5f, zCoordinate);
    //         ballRigidbody.position = testPos;
    //         float radial_midpoint = easy_radius + (hard_radius - easy_radius)/2;
    //         Vector3 ballPosition = ballRigidbody.position;
    //         float distance_between_hole_and_ball = Vector3.Distance(ballPosition, holePosition);
    //         if (distance_between_hole_and_ball > radial_midpoint)//distance between hole and ball is greater than radial_midpoint
    //         {
    //             hard_shot = true;
    //         }
    //         else
    //         {
    //             hard_shot = false;
    //         }
    //     }
    // }

    // public void LogDataEntry(string eventType)
    // {
    //     string timestamp = FormatSceneTime(Time.time);

    //     Vector3 holePosition = Hole.transform.TransformPoint(Hole.center);
        
    //     // Could possibly remove lower line since it might be similar to the Vector3s with no Y coord
    //     Vector3 ballPositionOffset = new Vector3(ballRigidbody.position.x - holePosition.x, 0f, ballRigidbody.position.z - holePosition.z);

    //     Vector3 pivotHitBoxPositionOffset = new Vector3(PivotHitBox.position.x - holePosition.x, PivotHitBox.position.y - holePosition.y, PivotHitBox.position.z - holePosition.z);
    //     Vector3 holePositionNoY = new Vector3(holePosition.x, 0f, holePosition.z);
    //     Vector3 ballPositionNoY = new Vector3(ballRigidbody.position.x, 0f, ballRigidbody.position.z);
    //     float radialError = Vector3.Distance(ballPositionNoY, holePositionNoY);

    //     // int difficulty = hard_shot ? 1 : 0;

    //     string data = $"{current_trial - 1},{current_block},{timestamp},{ballPositionOffset.x},{ballPositionOffset.z}," +
    //                   $"{pivotHitBoxPositionOffset.x},{pivotHitBoxPositionOffset.y},{pivotHitBoxPositionOffset.z}," +
    //                   $"{PivotPerspective.rotation.x},{PivotPerspective.rotation.z},{PivotHitBox.rotation.y}," +
    //                   $"0,0,{radialError},{eventType},{difficulty}\n";
        
    //     File.AppendAllText(csvPath, data);
    //     Debug.Log("Data entry logged.");
    // }

    string FormatSceneTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60F);
        int seconds = Mathf.FloorToInt(time - minutes * 60);
        float fraction = time * 1000;
        fraction = (fraction % 1000);

        return string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, fraction);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
