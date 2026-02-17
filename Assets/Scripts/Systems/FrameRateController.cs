using UnityEngine;

public class FrameRateController : MonoBehaviour
{
    [Header("Frame Rate")]
    [SerializeField] private int targetFps = 60;

    [Header("VSync")]
    [Tooltip("If true, uses VSync instead of Application.targetFrameRate.")]
    [SerializeField] private bool useVSync = false;

    private void Awake()
    {
        // Keep this object around if you ever change scenes later
        DontDestroyOnLoad(gameObject);

        if (useVSync)
        {
            // 1 = Every V Blank (locks to monitor refresh), 0 = off
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = -1; // let VSync control it
        }
        else
        {
            // Disable VSync so the target FPS cap actually applies
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFps;
        }
    }
}
