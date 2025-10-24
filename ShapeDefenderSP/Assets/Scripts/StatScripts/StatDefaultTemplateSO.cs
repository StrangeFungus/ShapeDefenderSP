using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;

[CreateAssetMenu(menuName = "StatEntry/Stat Default Template")]
public class StatDefaultTemplateSO : ScriptableObject
{
    [SerializeField] private List<StatEntry> statEntries;
    public IReadOnlyList<StatEntry> StatEntries => statEntries;

    private void OnValidate()
    {
        HashSet<StatName> duplicates = new();
        HashSet<StatName> seen = new();

        foreach (var stat in statEntries)
        {
            if (!seen.Add(stat.StatsName))
            {
                duplicates.Add(stat.StatsName);
            }
        }

        if (duplicates.Count > 0)
        {
            Debug.LogWarning($"Duplicate StatNames found in StatDefaultTemplateSO: {string.Join(", ", duplicates)}", this);
        }
    }
}
