using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }
    // Use this to set up static managers and anything that needs set up in a specific order.
    // Do I want to set up default stats I could copy?
    // Stat manager needs its InitializeDefaults function called









    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        AttackManager.SetUpDefaultAttackPrefabs();
    }
}
