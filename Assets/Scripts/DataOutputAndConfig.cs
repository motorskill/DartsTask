using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class DataOutputAndConfig : MonoBehaviour
{
    [SerializeField] private AimShoot aimShoot;
    
    private int subjectNumber;
    private string subjectID = null;
    public int current_trial;
    public int current_block;

    void Start()
    {
        subjectNumber = 1;
        InitializeConfiguration();
        SetupTrialData();
        LoadSubjectData();
        UpdateTrialData();
    }

    private void InitializeConfiguration()
    {
        if (!PlayerPrefs.HasKey("trials"))
        {
            PlayerPrefs.SetInt("trials", 2);
            PlayerPrefs.SetInt("blocks", 2);
            PlayerPrefs.SetString("subject", "");
            PlayerPrefs.SetInt("overwrite", 0);
            PlayerPrefs.SetInt("trialBlock", 1);
            PlayerPrefs.SetInt("subjectTracker", 0);
        }
    }

    private void LoadSubjectData()
    {
        subjectNumber = PlayerPrefs.GetInt("subjectTracker", 1);
        subjectID = PlayerPrefs.GetString("subject", null);

        Debug.Log("Subject data loaded: " + (subjectID ?? subjectNumber.ToString()));
    }

    private void SetupTrialData()
    {
        current_trial = PlayerPrefs.GetInt("trialBlock", 1);
        current_block = PlayerPrefs.GetInt("blocks");
    }

    private void UpdateTrialData()
    {
        int maxTrials = PlayerPrefs.GetInt("trials");
        int selectedBlock = PlayerPrefs.GetInt("blocks");

        if (current_trial > maxTrials)
        {
            PlayerPrefs.DeleteAll();
            if (aimShoot.serialPort != null && aimShoot.serialPort.IsOpen)
            {
                aimShoot.serialPort.Close();
                Debug.Log("Serial port closed.");
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
        PlayerPrefs.SetInt("trialBlock", current_trial);
        PlayerPrefs.SetInt("blockTracker", selectedBlock);
        Debug.Log($"Updated to Trial {current_trial}, Block {selectedBlock}");
    }
}