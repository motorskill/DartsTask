using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Runtime.InteropServices;

public class Viewpoint : MonoBehaviour
{
    private const string dllPath = "VPX_InterApp_64.dll";
    // Delegate to handle the callback

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate int DataCallback(int msg, int subMsg, int param1, int param2, IntPtr userPtr);

    public enum VPX_STATUSItem
    {
        ViewPointIsRunning = 0,   // Replace 0 with actual value from the SDK if different
        DataFileIsOpen = 1,       // Replace 1 with actual value
        DataFileIsPaused = 2,     // Replace 2 with actual value
        // Add other status items as needed
    }

    [DllImport(dllPath)]
    public static extern int VPX_SendCommand(string cmdLineArg);
    [DllImport(dllPath)]
    public static extern int VPX_InsertCallback(DataCallback callback, IntPtr userPtr);
    [DllImport(dllPath)]
    public static extern int VPX_RemoveCallback(DataCallback callback);
    [DllImport(dllPath)]
    public static extern int VPX_GetStatus(int statusRequest);

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Succesfully initiated viewpoint class");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
