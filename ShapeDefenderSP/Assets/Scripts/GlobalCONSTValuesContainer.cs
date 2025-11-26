using UnityEngine;

[System.Serializable]
public static class GlobalCONSTValuesContainer
{
    [SerializeField] private static float minimumCooldownTimer = 0.1f;
    public static float MINIMUMCOOLDOWNTIMER { get => minimumCooldownTimer; }

    [SerializeField] private static float defaultCoroutineDelayTimer = 0.00001f;
    public static float DEFAULTCOROUTINEDELAYTIMER { get => defaultCoroutineDelayTimer; }
    
    [SerializeField] private static float levelUpStatMultiplier = 0.1f;
    public static float LEVELUPSTATMULTIPLIER { get => levelUpStatMultiplier; }
}
