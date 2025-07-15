#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class EditorExitCleanup
{
    static EditorExitCleanup()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            if (Object.FindObjectOfType<EyeLinkManager>() is EyeLinkManager elm)
            {
                elm.FreeFSAMPLE();
                Debug.Log("EditorExitCleanup: FSAMPLE memory freed.");
            }
        }
    }
}
#endif
