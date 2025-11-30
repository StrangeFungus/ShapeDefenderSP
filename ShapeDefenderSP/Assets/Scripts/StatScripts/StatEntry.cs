using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;

[System.Serializable]
public class StatEntry
{
    [SerializeField] private StatName statsName = StatName.Default;
    public StatName StatsName => statsName;

    [SerializeField] private float baseValue = 0f;
    public float BaseValue { get => baseValue; }

    private float startingBaseValue = 0f;
    public float StartingBaseValue => startingBaseValue;

    private float modifyingValueTotal = 0f;
    public float ModifyingValueTotal { get => modifyingValueTotal; set => modifyingValueTotal = value; }

    private float modifyingPercentTotal = 0f;
    public float ModifyingPercentTotal { get => modifyingPercentTotal; set => modifyingPercentTotal = value; }

    public float CurrentValueTotal { get { return (baseValue + modifyingValueTotal) * (1f + (modifyingPercentTotal / 100f)); } }

    public StatEntry(StatName StatsName, float BaseValue)
    {
        statsName = StatsName;
        baseValue = BaseValue;
        startingBaseValue = BaseValue;

        if (StatsName == StatName.Default)
        {
            Debug.Log($"Added a stat with the stats name of Default.");
        }
    }

    public void ModifyBaseValue(StatModificationAction statModificationAction, float incomingBaseValue)
    {
        switch (statModificationAction)
        {
            case StatModificationAction.AddToValue:
                baseValue += incomingBaseValue;
                break;
            case StatModificationAction.SubtractFromValue:
                baseValue -= incomingBaseValue;
                break;
            case StatModificationAction.SetValueTo:
                baseValue = incomingBaseValue;
                break;
            default:
                break;
        }
    }

    public void ResetStatToStartingValues()
    {
        baseValue = startingBaseValue;
        modifyingValueTotal = 0f;
        modifyingPercentTotal = 0f;
    }
}
