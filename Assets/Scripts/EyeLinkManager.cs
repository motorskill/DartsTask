using System;
using System.Runtime.InteropServices;

// using UnityEditor.MemoryProfiler;
using UnityEngine;

public class EyeLinkManager : MonoBehaviour
{
    public string dataFileName = "default.edf";
    public bool isRecording = false;
    public bool connectionActive = false;
    // Live eye data
    public FSAMPLE currentEyeTrackingData;
    public IntPtr currentEyeTrackingPointer;

    public void InitEyeLink()
    {
        if (!connectionActive)
        {
            if (EyelinkCoreInterop.open_eyelink_connection(-1) != 0)
            {
                Debug.LogError("Could not open connection to EyeLink DLL.");
                return;
            }
            else
            {
                Debug.Log("Connection opened to EyeLink DLL.");
            }
            // EyelinkCoreInterop.eyelink_dummy_open();
            EyelinkCoreInterop.eyelink_open();
            // if (EyelinkCoreInterop.eyelink_send_command("set_calibration = HV5") != 0)
            // {
            //     Debug.LogError("Could not send command to EyeLink.");
            // }

            // EyelinkCoreInterop.do_tracker_setup();

            // string calibrationResult = "";
            // EyelinkCoreInterop.eyelink_cal_message(calibrationResult);
            // Debug.Log(calibrationResult);

            connectionActive = true;

            // EyelinkCoreInterop.send_command("screen_pixel_coords = 0 0 1919 1079");
            // EyelinkCoreInterop.send_command("link_sample_data = LEFT,RIGHT,GAZE");
            // Debug.Log("EyeLink initialized.");
        }
    }

    public void StartRecording()
    {
        if (!isRecording){
            if (EyelinkCoreInterop.open_data_file(dataFileName) != 0)
        {
            Debug.LogError("Failed to create file");
        }

        if (EyelinkCoreInterop.start_recording(1,1,0,0) != 0)
        {
            Debug.LogError("Failed to start recording");
        }
        // if (EyelinkCoreInterop.start_recording() != 0)
        // {
        //     Debug.LogError("Failed to start recording.");
        //     return;
        // }

        isRecording = true;
        Debug.Log("Recording started.");
        }
    }

    public void StopRecording()
    {

        if (isRecording)
        {
            EyelinkCoreInterop.stop_recording();
            if (EyelinkCoreInterop.close_data_file() != 0)
            {
                Debug.LogError("Failed to close file");
            }
            // transfer file back to computer maybe
            isRecording = false;
            Debug.Log("Recording stopped and connection closed.");
        }
    }

    public void AllocateFSAMPLEMemory()
    {
        currentEyeTrackingData= new FSAMPLE
        {
            px = new float[2],
            py = new float[2],
            hx = new float[2],
            hy = new float[2],
            pa = new float[2],
            gx = new float[2],
            gy = new float[2],
            hdata = new short[8]
        };

        int size = Marshal.SizeOf(typeof(FSAMPLE));
        currentEyeTrackingPointer = Marshal.AllocHGlobal(size);
        Marshal.StructureToPtr(currentEyeTrackingData, currentEyeTrackingPointer, false);
    }

    public void PollEyelink()
    {
        short result = EyelinkCoreInterop.eyelink_newest_float_sample(currentEyeTrackingPointer);
        if (result == 1)
        {
            currentEyeTrackingData = Marshal.PtrToStructure<FSAMPLE>(currentEyeTrackingPointer);
        }
    }

    public void FreeFSAMPLE()
    {
        Marshal.FreeHGlobal(currentEyeTrackingPointer);
    }

    void OnApplicationQuit()
    {
        EyelinkCoreInterop.close_eyelink_connection();
        StopRecording();
    }
}
