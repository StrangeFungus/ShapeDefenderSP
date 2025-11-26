using System.Collections.Generic;
using System.Linq;
using SDSPEnums;
using Unity.VisualScripting;
using UnityEngine;

public class AttackEntryManager : MonoBehaviour, IAttackEntryManager
{
    private static AttackEntryManager Instance { get; set; }

    // DEFAULT ATTACKS THAT CAN BE COPIED FROM
    [SerializeField] private GameObject fallbackDefaultAttackPrefab;
    private static (AttackName, BaseAttackController) fallbackDefaultAttackController;

    [SerializeField] private List<GameObject> defaultAttackPrefabs;

    private static Dictionary<AttackName, BaseAttackController> defaultAttackControllers = new();

    // OBJECT POOL FOR ATTACKS
    [SerializeField] private GameObject attackObjectPoolParent;
    private static Dictionary<AttackName, List<BaseAttackController>> attackPrefabObjectPool = new();

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

        InterfaceContainer.Register<IAttackEntryManager>(this);

        SetUpDefaultAttackPrefabs();
    }

    private void SetUpDefaultAttackPrefabs()
    {
        if (fallbackDefaultAttackPrefab != null && fallbackDefaultAttackController == (null, null))
        {
            fallbackDefaultAttackPrefab.TryGetComponent<BaseAttackController>(out var fallbackAttackController);
            {
                fallbackDefaultAttackController = (AttackName.None, fallbackAttackController);
            }
        }

        if (Instance.defaultAttackPrefabs != null)
        {
            if (Instance.defaultAttackPrefabs.Count > 0)
            {
                if (defaultAttackControllers.Count > 0)
                {
                    defaultAttackControllers.Clear();
                }

                foreach (var attack in Instance.defaultAttackPrefabs)
                {
                    if (attack.TryGetComponent<BaseAttackController>(out var baseAttackController))
                    {
                        if (!defaultAttackControllers.ContainsKey(baseAttackController.AttacksEntry.AttacksName))
                        {
                            defaultAttackControllers.Add(baseAttackController.AttacksEntry.AttacksName, baseAttackController);
                        }
                        else
                        {
                            Debug.Log($"Default Attack Prefabs already contains an entry for: {baseAttackController.AttacksEntry.AttacksName}");
                        }
                    }
                    else
                    {
                        Debug.Log($"The Default Attack Prefab Object ({attack.name}) didnt have or had the wrong component (No AttackPrefabController).");
                    }
                }
            }
        }
        else
        {
            Debug.Log($"Default Attack Prefabs are not set up in the inspector.");
        }
    }

    public BaseAttackController GetDefaultAttackController(AttackName attacksName)
    {
        if (defaultAttackControllers.TryGetValue(attacksName, out BaseAttackController baseAttackController))
        {
            return baseAttackController;
        }

        return fallbackDefaultAttackController.Item2;
    }

    public void AddNewAttack(AttackName attacksName, AttackEntryContainer entitiesAttackContainer, Transform parentsTransform)
    {
        if (attacksName != AttackName.None)
        {
            if (!entitiesAttackContainer.AttackControllerDictionary.ContainsKey(attacksName))
            {
                if (attackPrefabObjectPool.ContainsKey(attacksName))
                {
                    int poolCount = attackPrefabObjectPool[attacksName].Count;
                    if (poolCount > 0)
                    {
                        BaseAttackController baseAttackController = attackPrefabObjectPool[attacksName][poolCount];
                        baseAttackController.ResetAttacksController();

                        entitiesAttackContainer.AddAttack(baseAttackController);
                        attackPrefabObjectPool[attacksName].Remove(baseAttackController);
                    }
                }
                else
                {
                    if (defaultAttackControllers.TryGetValue(attacksName, out var defaultBaseAttackController))
                    {
                        GameObject newAttackObject = Instantiate(defaultBaseAttackController.gameObject);
                        if (newAttackObject.TryGetComponent<BaseAttackController>(out var attackController))
                        {
                            attackController.CopyAttacksControllerData(defaultBaseAttackController);
                            entitiesAttackContainer.AddAttack(attackController);

                            if (!attackPrefabObjectPool.ContainsKey(attacksName))
                            {
                                attackPrefabObjectPool.Add(attacksName, new());
                            }

                            if (!attackPrefabObjectPool[attacksName].Contains(attackController))
                            {
                                attackPrefabObjectPool[attacksName].Add(attackController);
                            }
                        }
                        else
                        {
                            Debug.Log($"newAttackObject, {defaultBaseAttackController.AttacksEntry.AttacksName}, didnt have a controller. Destroying Object.");
                            Destroy(newAttackObject);
                        }
                    }
                }
            }
            else
            {
                Debug.Log($"entitiesAttackContainer.AttackControllerDictionary already contains {attacksName}, returning...");
                return;
            }

            if (entitiesAttackContainer.AttackControllerDictionary[attacksName] != null && parentsTransform != null)
            {
                entitiesAttackContainer.AttackControllerDictionary[attacksName].gameObject.transform.SetParent(parentsTransform, false);
                entitiesAttackContainer.AttackControllerDictionary[attacksName].gameObject.SetActive(true);
            }
        }

        if (entitiesAttackContainer.AttackControllerDictionary[attacksName] != null && parentsTransform != null)
        {
            entitiesAttackContainer.AttackControllerDictionary[attacksName].gameObject.transform.SetParent(parentsTransform, false);
            entitiesAttackContainer.AttackControllerDictionary[attacksName].gameObject.SetActive(false);
        }
    }

    public void RemoveAttack(AttackName attacksName, AttackEntryContainer entitiesAttackContainer)
    {
        if (entitiesAttackContainer != null)
        {
            if (entitiesAttackContainer.AttackControllerDictionary.ContainsKey(attacksName))
            {
                BaseAttackController baseAttackController = entitiesAttackContainer.AttackControllerDictionary[attacksName];

                attackPrefabObjectPool[attacksName].Add(baseAttackController);
                entitiesAttackContainer.RemoveAttack(attacksName);

                baseAttackController.gameObject.transform.SetParent(Instance.gameObject.transform, false);
                baseAttackController.gameObject.SetActive(false);
            }
        }
    }

    public void ChangeAttackUsability(AttackName attacksName, AttackEntryContainer entitiesAttackContainer, bool canAttackBeUsed)
    {
        if (entitiesAttackContainer != null)
        {
            if (entitiesAttackContainer.AttackControllerDictionary.ContainsKey(attacksName))
            {
                if (entitiesAttackContainer.AttackControllerDictionary[attacksName] != null)
                {
                    if (canAttackBeUsed)
                    {
                        entitiesAttackContainer.AttackControllerDictionary[attacksName].enabled = true;
                        if (entitiesAttackContainer.AttackControllerDictionary[attacksName].TryGetComponent<SpriteRenderer>(out var renderer))
                        {
                            renderer.enabled = true;
                        }
                    }
                    else
                    {
                        entitiesAttackContainer.AttackControllerDictionary[attacksName].enabled = false;
                        if (entitiesAttackContainer.AttackControllerDictionary[attacksName].TryGetComponent<SpriteRenderer>(out var renderer))
                        {
                            renderer.enabled = false;
                        }
                    }
                }
            }
        }
    }
}
