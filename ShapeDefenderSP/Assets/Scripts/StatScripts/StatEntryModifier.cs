public readonly struct StatEntryModifier
{
    public float ModifyingValue { get; }
    public float ModifyingPercent { get; }

    public StatEntryModifier(float value, float percent)
    {
        ModifyingValue = value;
        ModifyingPercent = percent;
    }
}
