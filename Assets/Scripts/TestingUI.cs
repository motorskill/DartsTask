using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using TMPro;
using System.ComponentModel;
using System.Linq;



public class TestingUI : MonoBehaviour
{
    public Button ReadyButton;
    public TMP_InputField SubjectID;
    public TMP_InputField TrialField;
    public TMP_InputField BlockField;
    // public TMP_InputField TagField;
    private string csvPath;
    public GameObject CrashScreen;
    public GameObject NormalScreen;
    public TMP_Text BlockMessage;
    public Button AutomaticallyResetCSV;
    public Button ManuallyResetCSV;


    // Start is called before the first frame update
    void Start()
    {
        int current_trial = PlayerPrefs.GetInt("current_trial", 1);
        int current_block = PlayerPrefs.GetInt("current_block", 1);
        int maxTrials = PlayerPrefs.GetInt("max_trials", 10);
        int maxBlocks = PlayerPrefs.GetInt("max_blocks", 5);

        if (current_trial != 1 || current_block != 1)
        {
            NormalScreen.SetActive(false);
            CrashScreen.SetActive(true);
            BlockMessage.text = "Crash detected. Restarting block " + current_block;
            AutomaticallyResetCSV.onClick.AddListener(ResetCSVandTrials); // need to add delete csv
            ManuallyResetCSV.onClick.AddListener(DeleteAllPrefsAndQuit); // need to add delete csv
        }
        else
        {
            ReadyButton.onClick.AddListener(ReadyCheck);
        }
    }

    public void ResetCSVandTrials()
    {
        int current_block = PlayerPrefs.GetInt("current_block", 1);
        PlayerPrefs.SetInt("current_trial", 1);

        // Clear used trials
        PlayerPrefs.SetString("trials_used", "");
        PlayerPrefs.Save();

        csvPath = $"D:/QuietArchery_Stuff/QuietArcheryBackend/Data/Subject{PlayerPrefs.GetString("subjectID", "")}.block.{current_block}.csv";

        // Overwrite the CSV file with a new header
        if (File.Exists(csvPath))
        {
            // File.WriteAllText(csvPath, "Trial#,Block#,Timestamp,Ball X,Ball Z,Clubhead X position,Clubhead Y position,Clubhead Z position,Club X rotation,Club Z rotation,Clubhead Y rotation,Hole x position,Hole z position,Radial Error,Event,Difficulty,Force,TransXinput,TransYinput\n");
        }

        // SceneManager.LoadScene("EyelinkCalibrationScene");
        SceneManager.LoadScene("ArcheryScene");
    }
    public void DeleteAllPrefsAndQuit()
    {
        // Clear all stored PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save(); // Ensure changes are written

        Debug.Log("All PlayerPrefs deleted. Exiting application...");

        #if UNITY_EDITOR
            // Stop play mode in the Unity Editor
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Quit the application
            Application.Quit();
        #endif
    }

    public void ReadyCheck()
    {
        string subjectID = SubjectID.text;
        int trialCount = int.Parse(TrialField.text);
        int blockCount = int.Parse(BlockField.text);

        PlayerPrefs.SetString("subjectID", subjectID);
        PlayerPrefs.SetInt("current_trial", 1);
        PlayerPrefs.SetInt("current_block", blockCount);
        PlayerPrefs.SetInt("max_trials", trialCount);
        PlayerPrefs.SetInt("max_blocks", blockCount);
        PlayerPrefs.SetInt("EyelinkRecording", 0);
        PlayerPrefs.Save();

        // SceneManager.LoadScene("EyelinkCalibrationScene");
        SceneManager.LoadScene("ArcheryScene");
    }
}