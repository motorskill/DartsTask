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
    public TMP_InputField BlockOverwrite;
    public GameObject CrashScreen;
    public GameObject NormalScreen;
    public TMP_Text BlockMessage;
    public Button RestartFromLastBlock;
    public TMP_InputField BlockToRestartFrom;
    public Button RestartFromSpecifiedBlock;
    public Button RestartWholeSubject;

    void Start()
    {
        int current_trial = PlayerPrefs.GetInt("trialBlock", 1);
        int current_block = PlayerPrefs.GetInt("blockTracker", 1);
        if (current_trial != 1 || current_block != 1)
        {
            NormalScreen.SetActive(false);
            CrashScreen.SetActive(true);
            UpdateBlockCrashedOn();
            RestartFromLastBlock.onClick.AddListener(RestartLastBlock);
            RestartFromSpecifiedBlock.onClick.AddListener(RestartSpecificBlock);
            RestartWholeSubject.onClick.AddListener(RestartSubject);
        }
        else
        {
            ReadyButton.onClick.AddListener(ReadyCheck);
        }
    }

    void UpdateBlockCrashedOn()
    {
        BlockMessage.text = "Block where crash occurred: " + PlayerPrefs.GetInt("blockTracker", 1);
    }

    public void RestartLastBlock()
    {
        PlayerPrefs.SetInt("blockTracker", PlayerPrefs.GetInt("blockTracker", 1));
        SceneManager.LoadScene("ArcheryScene");
    }

    public void RestartSpecificBlock()
    {
        if (int.TryParse(BlockToRestartFrom.text, out int blockNum))
        {
            PlayerPrefs.SetInt("blockTracker", blockNum);
            SceneManager.LoadScene("ArcheryScene");
        }
    }

    public void RestartSubject()
    {
        PlayerPrefs.SetInt("blockTracker", 1);
        SceneManager.LoadScene("ArcheryScene");
    }

    public void ReadyCheck()
    {
        string subjectID = SubjectID.text;
        if (!string.IsNullOrEmpty(subjectID))
        {
            PlayerPrefs.SetString("subject", subjectID);
            PlayerPrefs.SetInt("trials", int.Parse(TrialField.text));
            PlayerPrefs.SetInt("blocks", int.Parse(BlockField.text));
            SceneManager.LoadScene("ArcheryScene");
        }
    }
}