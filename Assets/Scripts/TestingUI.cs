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
    public TMP_InputField BlockOverwrite;
    public GameObject CrashScreen;
    public GameObject NormalScreen;
    public TMP_Text BlockMessage;
    public Button RestartFromLastBlock;
    public TMP_InputField BlockToRestartFrom;
    public Button RestartFromSpecifiedBlock;
    public Button RestartWholeSubject;
    private string configPath;
    private string trialblock_tracker;
    private string subject_tracker;


    // Start is called before the first frame update
    void Start()
    {
        // #if UNITY_EDITOR
        //     string directoryPath = Application.dataPath + "/Data";
        //     if (!Directory.Exists(directoryPath))
        //     {
        //         Directory.CreateDirectory(directoryPath);
        //     }
        //     // filePath = directoryPath + "/Subject" + subjectNumber + ".txt";
        //     // csvPath = directoryPath + "/Subject" + subjectNumber + ".csv";
        //     if (!Directory.Exists(Application.dataPath + "/Resources"))
        //     {
        //         Directory.CreateDirectory(Application.dataPath + "/Resources");
        //     }
        //     configPath = Application.dataPath + "/Resources/test_config.txt";
        //     if (!File.Exists(configPath))
        //     {
        //         // Create the file
        //         using (FileStream fs = File.Create(configPath));
        //         WriteArrayToFile(configPath, new string [] {"trials","2","blocks","2","subject","","overwrite"}, false);
        //     }
        //     // string testPath = Application.dataPath + "/Resources/trial_coords.txt";
        //     // if (!File.Exists(testPath))
        //     // {
        //     //     // Create the file
        //     //     using (FileStream fs = File.Create(testPath));

        //     // }
        //     trialblock_tracker = Application.dataPath + "/Resources/trial_block_track.txt";
        //     if (!File.Exists(trialblock_tracker))
        //     {
        //         // Create the file
        //         using (FileStream fs = File.Create(trialblock_tracker))
        //         WriteArrayToFile(trialblock_tracker, new string [] {"1", "1"}, false);
        //     }
        //     subject_tracker = Application.dataPath + "/Resources/subject_tracker.txt";
        //     if (!File.Exists(subject_tracker))
        //     {
        //         // Create the file
        //         using (FileStream fs = File.Create(subject_tracker))
        //         WriteArrayToFile(subject_tracker, new string [] {"0"}, false);
        //     }
        // #else
            string backend = "QuietArcheryBackend";
            string directoryPath = "D:/QuietArchery_Stuff" + "/" + backend + "/Data";
            // Ensure the directory exists
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            
            if (!Directory.Exists("D:/QuietArchery_Stuff" + "/" + backend + "/Resources"))
            {
                Directory.CreateDirectory("D:/QuietArchery_Stuff" + "/" + backend + "/Resources");
            }

            configPath = "D:/QuietArchery_Stuff" + "/" + backend + "/Resources/test_config.txt";
            if (!File.Exists(configPath))
            {
                // Create the file
                using (FileStream fs = File.Create(configPath));
                WriteArrayToFile(configPath, new string [] {"trials","2","blocks","2","subject","","overwrite"}, false);
            }
            // string testPath = "D:/QuietArchery_Stuff" + "/" + backend + "/Resources/trial_coords.txt";
            // if (!File.Exists(testPath))
            // {
            //     // Create the file
            //     using (FileStream fs = File.Create(testPath));

            // }
            trialblock_tracker = "D:/QuietArchery_Stuff" + "/" + backend + "/Resources/trial_block_track.txt";
            if (!File.Exists(trialblock_tracker))
            {
                // Create the file
                using (FileStream fs = File.Create(trialblock_tracker));
                WriteArrayToFile(trialblock_tracker, new string [] {"1", "1"}, false);
            }
            subject_tracker = "D:/QuietArchery_Stuff" + "/" + backend + "/Resources/subject_tracker.txt";
            if (!File.Exists(subject_tracker))
            {
                // Create the file
                using (FileStream fs = File.Create(subject_tracker));
                WriteArrayToFile(subject_tracker, new string [] {"0"}, false);
            }
            string trials_used = "D:/QuietArchery_Stuff" + "/" + backend + "/Resources/trials_used.txt";
            if (!File.Exists(trials_used))
            {
                // Create the file
                using (FileStream fs = File.Create(trials_used));
                WriteArrayToFile(trials_used, new string [] {""}, false);
            }
        // #endif
        string[] current_trial_block = ParseFile(trialblock_tracker);
        if (current_trial_block[0] != "1" || current_trial_block[1] != "1")
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

    // Update is called once per frame
    void Update()
    {        
    }

    public string GetBlockNumCrash()
    {
        return BlockToRestartFrom.text;
    }

    public void UpdateBlockCrashedOn()
    {
        string[] current_trial_block = ParseFile(trialblock_tracker);
        BlockMessage.text = "Block where crash occured: " + current_trial_block[1].ToString();
    }

    public void RestartLastBlock()
    {
        string[] config_content = ParseFile(configPath);
        string[] current_trial_block = ParseFile(trialblock_tracker);
        int BlockToOverwrite = int.Parse(current_trial_block[1]);
        WriteArrayToFile(configPath, config_content, false);
        using (StreamWriter writer = new StreamWriter(configPath, true))
        {
            writer.Write("," + BlockToOverwrite);
        }
        SceneManager.LoadScene("ArcheryScene");
    }
    public void RestartSpecificBlock()
    {
        string[] config_content = ParseFile(configPath);
        string[] current_trial_block = ParseFile(trialblock_tracker);
        string BlockToReplace = GetBlockNumCrash();
        if(IsInteger(BlockToReplace))
        {
            int BlockToOverwrite = int.Parse(BlockToReplace);
            WriteArrayToFile(configPath, config_content, false);
            using (StreamWriter writer = new StreamWriter(configPath, true))
            {
                writer.Write("," + BlockToOverwrite);
            }
            SceneManager.LoadScene("ArcheryScene");
        }
    }
    public void RestartSubject()
    {
        string[] config_content = ParseFile(configPath);
        string[] current_trial_block = ParseFile(trialblock_tracker);
        int BlockToOverwrite = 1;
        WriteArrayToFile(configPath, config_content, false);
        using (StreamWriter writer = new StreamWriter(configPath, true))
        {
            writer.Write("," + BlockToOverwrite);
        }
        SceneManager.LoadScene("ArcheryScene");
    }

    public string GetSubjectFieldText()
    {
        return SubjectID.text;
    }

    public string GetBlockOverwriteText()
    {
        return BlockOverwrite.text;
    }

    public string GetTrialFieldText()
    {
        return TrialField.text;
    }

    public string GetBlockFieldText()
    {
        return BlockField.text;
    }

    public void ReadyCheck()
    {
        string SubjectID_content = GetSubjectFieldText();
        string BlockOverwrite_content = GetBlockOverwriteText();
        string TrialField_content = GetTrialFieldText();
        string BlockField_content = GetBlockFieldText();
        string[] config_content = ParseFile(configPath);
        string[] trialBlockData = ParseFile(trialblock_tracker);
        string[] subject_tracker_content = ParseFile(subject_tracker);
        if (SubjectID_content == "next" && TrialField_content != "" && BlockField_content != "")
        { // No subject number: new subject can pick next number
            config_content[1] = TrialField_content;
            trialBlockData[1] = BlockField_content;
            config_content[5] = SubjectID_content;
            WriteArrayToFile(configPath, config_content, false);
            WriteArrayToFile(trialblock_tracker, trialBlockData, false);
            subject_tracker_content[0] = SubjectID_content;
            WriteArrayToFile(subject_tracker, subject_tracker_content, false);
            SceneManager.LoadScene("ArcheryScene");
        }
        string directoryPath;
        string filePath;
        #if UNITY_EDITOR
        if (IsInteger(SubjectID_content))
        {
            directoryPath = Application.dataPath + "/Data";
            filePath = directoryPath + "/Subject" + SubjectID_content + ".csv";
        }
        else
        {
            directoryPath = Application.dataPath + "/Data";
            filePath = directoryPath + "/" + SubjectID_content + ".csv";
        }
        #else
        if (IsInteger(SubjectID_content))
        {
            string backend = "QuietArcheryBackend";
            filePath = "D:/QuietArchery_Stuff" + "/" + backend + "/Subject" + SubjectID_content + ".csv";
        }
        else
        {
            string backend = "QuietArcheryBackend";
            filePath = "D:/QuietArchery_Stuff" + "/" + backend + "/" + SubjectID_content + ".block." + BlockField_content + ".csv";

        }
        #endif
        if (File.Exists(filePath))
        { // If file path exists
            // Check which block to overwrite.
            BlockOverwrite.gameObject.SetActive(true);
            if (IsInteger(BlockOverwrite_content) && int.Parse(BlockOverwrite_content) <= int.Parse(config_content[3]) && int.Parse(BlockOverwrite_content) > 0)
            {
                config_content[5] = SubjectID_content;
                WriteArrayToFile(configPath, config_content, false);
                using (StreamWriter writer = new StreamWriter(configPath, true))
                {
                    writer.Write("," + BlockOverwrite_content);
                }
                BlockOverwrite.gameObject.SetActive(false);
                SceneManager.LoadScene("ArcheryScene");
            }
        }
        else
        { // File path does not exist
            // Communicate to main scene which subject # it needs to use, if any.
            config_content[1] = TrialField_content;
            trialBlockData[1] = BlockField_content;
            config_content[5] = SubjectID_content;
            WriteArrayToFile(configPath, config_content, false);
            WriteArrayToFile(trialblock_tracker, trialBlockData, false);
            subject_tracker_content[0] = SubjectID_content;
            WriteArrayToFile(subject_tracker, subject_tracker_content, false);  
            SceneManager.LoadScene("ArcheryScene");
        }
    }

    public static bool IsInteger(string input)
    {
        int result;
        return int.TryParse(input, out result);
    }

    string[] ParseFile(string fileName)
    {
        // Ensure the file is in the Resources folder
        TextAsset file = Resources.Load<TextAsset>(Path.GetFileNameWithoutExtension(fileName));
        if (file != null)
        {
            // Read the file contents
            string fileContents = file.text;

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
}