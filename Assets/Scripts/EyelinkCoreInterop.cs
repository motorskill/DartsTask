using System.Runtime.InteropServices;
using UnityEngine;

public class EyelinkCoreInterop : MonoBehaviour
{
    private const string DLL_NAME = "eyelink_core64.dll";
    private const string GRAPHICS_DLL_NAME = "eyelink_core_graphics64.dll";

    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern short open_eyelink_connection(short mode);
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern short eyelink_dummy_open();
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern short eyelink_open();
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern short do_tracker_setup();
    [DllImport(GRAPHICS_DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern short open_data_file(string name);
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern short close_data_file();
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern short start_recording(short file_samples, short file_events, short link_samples, short link_events);
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void stop_recording();
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern short eyelink_send_command(string text);
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern void close_eyelink_connection();
    [DllImport(DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern short eyelink_is_connected();
    [DllImport(GRAPHICS_DLL_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
    public static extern short eyelink_cal_message(string msg);
}
