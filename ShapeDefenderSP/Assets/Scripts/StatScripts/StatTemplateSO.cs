using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;

[CreateAssetMenu(menuName = "StatEntry/Stat Template")]
public class StatTemplateSO : ScriptableObject
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
            Debug.LogWarning($"Duplicate StatNames found in StatTemplateSO: {string.Join(", ", duplicates)}", this);
        }
    }

}
