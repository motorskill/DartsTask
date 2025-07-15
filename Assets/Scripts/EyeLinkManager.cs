using System;
using System.Runtime.InteropServices;

// using UnityEditor.MemoryProfiler;
using UnityEngine;

public class EyeLinkManager : MonoBehaviour
{
    public string dataFileName = "default.edf";
    public bool connectionActive = false;
    // Live eye data
    public FSAMPLE currentEyeTrackingData;
    public IntPtr currentEyeTrackingPointer;
    public int eye_used;

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
        if (PlayerPrefs.GetInt("EyelinkRecording") == 0)
        {
            EyelinkCoreInterop.eyelink_send_command("screen_pixel_coords = 0 0 1919 1079");
            EyelinkCoreInterop.eyelink_send_command("link_sample_data = LEFT,RIGHT,GAZE,AREA,STATUS,INPUT");
            EyelinkCoreInterop.eyelink_send_command("file_sample_data = LEFT,RIGHT,GAZE,AREA,STATUS,INPUT");
            if (EyelinkCoreInterop.open_data_file(dataFileName) != 0)
            {
                Debug.LogError("Failed to create file");
            }

            if (EyelinkCoreInterop.start_recording(1, 1, 1, 0) != 0)
            {
                Debug.LogError("Failed to start recording");
            }
            // if (EyelinkCoreInterop.start_recording() != 0)
            // {
            //     Debug.LogError("Failed to start recording.");
            //     return;
            // }

            PlayerPrefs.SetInt("EyelinkRecording", 1);
            Debug.Log("Recording started.");
        }
    }

    public void StopRecording()
    {
        if (PlayerPrefs.GetInt("EyelinkRecording") == 1)
        {
            EyelinkCoreInterop.stop_recording();
            if (EyelinkCoreInterop.close_data_file() != 0)
            {
                Debug.LogError("Failed to close file");
            }
            // transfer file back to computer maybe
            PlayerPrefs.SetInt("EyelinkRecording", 0);
            Debug.Log("Recording stopped and connection closed.");
        }
    }

    public void AllocateFSAMPLEMemory()
    {
        currentEyeTrackingData = new FSAMPLE
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

        Debug.Log("FSAMPLE memory allocated.");
    }

    public void PollEyelink()
    {
        eye_used = EyelinkCoreInterop.eyelink_eye_available();
        if (eye_used == 2) eye_used = 0;
        short result = EyelinkCoreInterop.eyelink_newest_float_sample(currentEyeTrackingPointer);
        if (result > 0)
        {
            currentEyeTrackingData = Marshal.PtrToStructure<FSAMPLE>(currentEyeTrackingPointer);
            // DebugFSAMPLE();
        }
        else
        {
            Debug.LogWarning("eyelink_newest_float_sample returned no data despite check.");
        }
    }

    public void FreeFSAMPLE()
    {
        if (currentEyeTrackingPointer != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(currentEyeTrackingPointer);
            currentEyeTrackingPointer = IntPtr.Zero;
            Debug.Log("FSAMPLE memory freed.");
        }
    }

    void OnApplicationQuit()
    {
        EyelinkCoreInterop.close_eyelink_connection();
        StopRecording();
    }

    void OnDestroy()
    {
        PlayerPrefs.SetInt("EyelinkRecording", 0);
        StopRecording();
        FreeFSAMPLE();
    }

    public void DebugFSAMPLE()
    {
        var f = currentEyeTrackingData;

        Debug.Log($@"
        FSAMPLE:
        - time: {f.time}
        - type: {f.type}
        - flags: {f.flags}
        - px: [{f.px[0]}, {f.px[1]}]
        - py: [{f.py[0]}, {f.py[1]}]
        - hx: [{f.hx[0]}, {f.hx[1]}]
        - hy: [{f.hy[0]}, {f.hy[1]}]
        - pa (pupil area): [{f.pa[0]}, {f.pa[1]}]
        - gx (gaze x): [{f.gx[0]}, {f.gx[1]}]
        - gy (gaze y): [{f.gy[0]}, {f.gy[1]}]
        - rx: {f.rx}
        - ry: {f.ry}
        - status: {f.status}
        - input: {f.input}
        - buttons: {f.buttons}
        - htype: {f.htype}
        - hdata: [{string.Join(", ", f.hdata)}]
        ");
    }


}
