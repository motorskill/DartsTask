using System;
// using UnityEditor.MemoryProfiler;
using UnityEngine;
using UnityEngine.SceneManagement;


public class CalibrationEyelink : MonoBehaviour
{

    public bool connectionActive = false;
    public bool calibrationStarted = false;
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

            // string calibrationResult = "";
            // EyelinkCoreInterop.eyelink_cal_message(calibrationResult);
            // Debug.Log(calibrationResult);

            // EyelinkCoreInterop.send_command("screen_pixel_coords = 0 0 1919 1079");
            // EyelinkCoreInterop.send_command("link_sample_data = LEFT,RIGHT,GAZE");
            // Debug.Log("EyeLink initialized.");

            // Get Unity window handle for calibration UI
            // IntPtr hwnd = EyelinkCoreInterop.GetActiveWindow();
            // if (hwnd == IntPtr.Zero)
            // {
            //     Debug.LogError("Failed to get Unity window handle.");
            //     return;
            // }

            // EyelinkCoreInterop.init_expt_graphics(hwnd, Screen.currentResolution.width, Screen.currentResolution.height, 32);
            // connectionActive = true;
            // Debug.Log("EyeLink initialized and graphics setup.");
        }
    }

    public void CalibrateEyes()
    {
        EyelinkCoreInterop.do_tracker_setup();
        calibrationStarted = true;  // Used to trigger transition when eyelink_in_setup() becomes false
    }

    private void Start()
    {
        InitEyeLink();
        CalibrateEyes();
    }
    private void Update()
    {
        if (EyelinkCoreInterop.eyelink_in_setup() == 0 && calibrationStarted)
        {
            SceneManager.LoadScene("ArcheryScene");
        }        
    }
}