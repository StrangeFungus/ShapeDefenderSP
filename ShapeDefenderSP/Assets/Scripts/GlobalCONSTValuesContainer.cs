using UnityEngine;

[System.Serializable]
public static class GlobalCONSTValuesContainer
{
    [SerializeField] private static float _MINIMUMCOOLDOWNTIMER = 0.1f;
    public static float MINIMUMCOOLDOWNTIMER { get => _MINIMUMCOOLDOWNTIMER; }

    [SerializeField] private static float _DEFAULTCOROUTINEDELAYTIMER = 0.00001f;
    public static float DEFAULTCOROUTINEDELAYTIMER { get => _DEFAULTCOROUTINEDELAYTIMER; }
}
