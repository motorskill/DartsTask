    #if UNITY_EDITOR
    using UnityEditor;
    #endif
    using System.Collections;
    using System.Collections.Generic;
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public class SyncViewPoint : MonoBehaviour
    {
        [SerializeField] AimShoot aimShoot;
        [SerializeField] DataOutputAndConfig dataOutput;
        private bool isFileOpen = false;  // Track whether a data file is currently open
        private bool isFilePaused = false;
        private bool FileStarted = false;
        private bool Exclude = true;
        private Viewpoint.DataCallback dataCallback;
        
        // VPX_DAT_FRESH constant from VP_Message_NotificationCodes enum
        private const int VPX_DAT_FRESH = 2;

        private const int EYE_A = 0; // Or whatever value corresponds to the left eye in the SDK
        private const int EYE_B = 1; // Or whatever value corresponds to the right eye in the SDK


        // Start is called before the first frame update
        void Start()
        {

            // // Close the previous data file if one is open
            // if (isFileOpen)
            // {
            //     Viewpoint.VPX_SendCommand("dataFile_Close");
            //     isFileOpen = false;
            // }

            // dataCallback = new Viewpoint.DataCallback(OnNewDataCallback);

            // result = Viewpoint.VPX_InsertCallback(dataCallback, IntPtr.Zero);
            // Debug.Log("VPX_InsertCallback result: " + result);

            // int status = Viewpoint.VPX_GetStatus((int)Viewpoint.VPX_STATUSItem.DataFileIsOpen);
            // Debug.Log("Data File Open Status: " + status);
            Exclude = true;
            isFilePaused = true;

        }

        // Update is called once per frame
        void Update()
        {
        // if (Input.anyKeyDown)
        // {
            // if (isFilePaused)
            // {
            //     Viewpoint.VPX_SendCommand("dataFile_Pause OFF");
            //     isFilePaused = false;
            // }
            if(aimShoot.dataRecordingEnabled)
            {
                if (!FileStarted && Exclude)
                {
                    // Open new data file
                    int result = Viewpoint.VPX_SendCommand("dataFile_NewUnique");
                    Debug.Log("VPX_SendCommand result (NewUnique): " + result);

                    result = Viewpoint.VPX_SendCommand("dataFile_AsynchStringData OFF");
                    Debug.Log("VPX_SendCommand result (AsynchStringData OFF): " + result);

                    // Mark that a new data file is now open
                    isFileOpen = true;
                    FileStarted = true;
                }


                if (FileStarted)
                {
                    Viewpoint.VPX_SendCommand($"dataFile_InsertString 'Game time (seconds): {aimShoot.GetAdjustedTime()} Trial#: {dataOutput.current_trial - 1} Block#: {dataOutput.current_block}'");
                }
            }
            // Viewpoint.VPX_SendCommand($"dataFile_InsertMarker ");
        // }
        }

        void OnApplicationQuit()
        {
            if (aimShoot.dataRecordingEnabled)
            {
                Exclude = false;
                FileStarted = false;
                Viewpoint.VPX_SendCommand("dataFile_Close");
                isFileOpen = false;
            }
        }

        #if UNITY_EDITOR
            // Detect when the play mode changes
            private void OnEnable()
            {
                EditorApplication.playModeStateChanged += OnPlayModeChanged;
            }

            private void OnDisable()
            {
                EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            }

            private void OnPlayModeChanged(PlayModeStateChange state)
            {
                if (state == PlayModeStateChange.ExitingPlayMode && isFileOpen && aimShoot.dataRecordingEnabled)
                {
                    Exclude = false;
                    FileStarted = false;
                    // This is equivalent to OnApplicationQuit for the editor
                    Viewpoint.VPX_SendCommand("dataFile_Close");
                    isFileOpen = false;
                    // Remove the callback function before quitting
                    Viewpoint.VPX_RemoveCallback(dataCallback);
                }
            }
        #endif 
    }
