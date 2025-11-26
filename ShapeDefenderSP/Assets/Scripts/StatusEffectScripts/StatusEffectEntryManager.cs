using System;
using System.Collections.Generic;
using SDSPEnums;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class StatusEffectEntryManager : MonoBehaviour, IStatusEffectEntryManager
{
    public static StatusEffectEntryManager Instance { get; private set; }
    [SerializeField] private float defaultStunDuration = 1.0f;
    [SerializeField] private float defaultPushPullEffectSpeed = 25.0f;

    [SerializeField] private List<StatusEffectEntry> defaultStatusEffects;

    // BUFF RELATED DATA STRUCTURES
    private Dictionary<StatusEffectName, StatusEffectEntry> defaultBuffStatusEffectsDictionary = new();
    private List<StatusEffectName> buffNames = new()
    {
        StatusEffectName.PowerSurgeStack,
        StatusEffectName.PiercingStrikeStack,
        StatusEffectName.CriticalBoostStack,
        StatusEffectName.FortifiedStack,
        StatusEffectName.RegenerationStack,
        StatusEffectName.ResistanceStack,
        StatusEffectName.HasteStack,
        StatusEffectName.FocusStack,
        StatusEffectName.EmpowerStack,
        StatusEffectName.InspirationStack,
        StatusEffectName.AdaptiveStack,
        StatusEffectName.CooldownReductionStack,
    };

    // DEBUFF RELATED DATA STRUCTURES
    private Dictionary<StatusEffectName, StatusEffectEntry> defaultDebuffStatusEffectsDictionary = new();
    private List<StatusEffectName> debuffNames = new()
    {
        StatusEffectName.BleedingStack,
        StatusEffectName.SlowedStack,
        StatusEffectName.PoisoningStack,
        StatusEffectName.EnvenomationStack,
        StatusEffectName.WeakenedStack,
        StatusEffectName.VulnerabilityStack,
        StatusEffectName.BurningStack,
        StatusEffectName.ChilledStack,
        StatusEffectName.ParalyzationStack,
        StatusEffectName.BlindedStack,
        StatusEffectName.CursedStack,
        StatusEffectName.HexedStack,
        StatusEffectName.StunEffect,
        StatusEffectName.KnockbackEffect,
        StatusEffectName.FreezeEffect,
        StatusEffectName.FearEffect,
        StatusEffectName.ImmobilizeEffect,
    };
    private List<StatusEffectName> debuffsThatApplyDamageOverTime = new()
    {
        StatusEffectName.BleedingStack,
        StatusEffectName.PoisoningStack,
        StatusEffectName.EnvenomationStack,
        StatusEffectName.BurningStack,
        StatusEffectName.ChilledStack,
        StatusEffectName.FreezeEffect,
        StatusEffectName.CursedStack,
        StatusEffectName.HexedStack,
    };
    private List<StatusEffectName> debuffsThatReduceTargetsStats= new()
    {
        StatusEffectName.SlowedStack,
        StatusEffectName.WeakenedStack,
        StatusEffectName.VulnerabilityStack,
        StatusEffectName.BlindedStack,
    };
    private List<StatusEffectName> debuffsThatRestrictTargetsActions = new()
    {
        StatusEffectName.StunEffect,
        StatusEffectName.ParalyzationStack,
        StatusEffectName.FearEffect,
        StatusEffectName.ImmobilizeEffect,
        StatusEffectName.FreezeEffect,
    };

    // MANAGERS
    private IStatEntryManager iStatEntryManager;
    private IDefenseSequenceManager iDefenseSequenceManager;
    private IHealthManager iHealthManager;


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

        InterfaceContainer.Register<IStatusEffectEntryManager>(this);

        SetupDefaultStatusEffects();
    }

    private void Start()
    {
        iStatEntryManager ??= InterfaceContainer.Request<IStatEntryManager>();
        iDefenseSequenceManager ??= InterfaceContainer.Request<IDefenseSequenceManager>();
        iHealthManager ??= InterfaceContainer.Request<IHealthManager>();
    }

    private void SetupDefaultStatusEffects()
    {
        if (defaultStatusEffects != null)
        {
            foreach (var statusEffect in defaultStatusEffects)
            {
                if (buffNames.Contains(statusEffect.StatusEffectsName))
                {
                    defaultBuffStatusEffectsDictionary.Add(statusEffect.StatusEffectsName, statusEffect);
                }
                else if (debuffNames.Contains(statusEffect.StatusEffectsName))
                {
                    defaultDebuffStatusEffectsDictionary.Add(statusEffect.StatusEffectsName, statusEffect);
                }
            }
        }
    }

    public Dictionary<StatusEffectName, StatusEffectEntry> CopyStatusEffectEntryDictionary(Dictionary<StatusEffectName, StatusEffectEntry> statusEffectEntryDictionary)
    {
        Dictionary<StatusEffectName, StatusEffectEntry> newStatusEffectEntryDictionary = new Dictionary<StatusEffectName, StatusEffectEntry>();

        foreach (var entry in statusEffectEntryDictionary)
        {
            newStatusEffectEntryDictionary.Add(entry.Key, new());
            newStatusEffectEntryDictionary[entry.Key].CopyStatusEffectEntry(entry.Value);
        }

        return newStatusEffectEntryDictionary;
    }

    public StatusEffectEntry CopyDefaultStatusEffectEntry(StatusEffectName statusEffectsName)
    {
        if (buffNames.Contains(statusEffectsName))
        {
            if (defaultBuffStatusEffectsDictionary.ContainsKey(statusEffectsName))
            {
                StatusEffectEntry newStatusEffectEntry = new();
                newStatusEffectEntry.CopyStatusEffectEntry(defaultBuffStatusEffectsDictionary[statusEffectsName]);
                
                return newStatusEffectEntry;
            }
        }
        else if (debuffNames.Contains(statusEffectsName))
        {
            if (defaultDebuffStatusEffectsDictionary.ContainsKey(statusEffectsName))
            {
                StatusEffectEntry newStatusEffectEntry = new();
                newStatusEffectEntry.CopyStatusEffectEntry(defaultDebuffStatusEffectsDictionary[statusEffectsName]);

                return newStatusEffectEntry;
            }
        }

        return null;
    }

    public void ApplyStatusEffect(BaseEntityController targetEntitiesController, StatusEffectEntryContainer statusEffectEntryContainerToApply, StatusEffectName statusEffectsName, BaseEntityController attackingEntitiesController = null)
    {
        if (targetEntitiesController == null) { Debug.Log($"target Entities Controller wasnt found, returning..."); return; }
        if (statusEffectEntryContainerToApply == null) { Debug.Log($"status Effect Entry To Apply wasnt found, returning..."); return; }
        if (statusEffectsName == StatusEffectName.Default) { Debug.Log($"status Effects name was Default, returning..."); return; }
        if (attackingEntitiesController == null) { Debug.Log($"attacking Entities Controller was null... (should it have been?)"); }

        if (!targetEntitiesController.EntitiesActiveStatusEffects.ContainsKey(statusEffectsName))
        {
            targetEntitiesController.EntitiesActiveStatusEffects.Add(statusEffectsName, new());
        }
        
        if (targetEntitiesController.EntitiesActiveStatusEffects[statusEffectsName].Count < statusEffectEntryContainerToApply.StatusEffectsDictionary[statusEffectsName].MaxStackAmount)
        {
            targetEntitiesController.EntitiesActiveStatusEffects[statusEffectsName].Add(statusEffectEntryContainerToApply.StatusEffectsDictionary[statusEffectsName]);
        }

        if (defaultBuffStatusEffectsDictionary.ContainsKey(statusEffectsName))
        {
            // --- ONCE THE DEBUFFS ARE FINISHED AND TESTED, I WILL WORRY ABOUT ADDING IN THE BUFFS....
        }
        else if (defaultDebuffStatusEffectsDictionary.ContainsKey(statusEffectsName))
        {
            StartANewDebuffEffectCoroutine(targetEntitiesController, statusEffectEntryContainerToApply, statusEffectsName, attackingEntitiesController);
        }

        //iFloatingTextManager.GenerateFloatingText(baseEntityController.transform.position, statusEffectsName.ToString(), targetEntitiesController.EntitiesActiveStatusEffects[statusEffectsName].Count.ToString());
    }

    private void StartANewDebuffEffectCoroutine(BaseEntityController targetEntitiesController, StatusEffectEntryContainer statusEffectEntryContainerToApply, StatusEffectName statusEffectsName, BaseEntityController attackingEntitiesController = null)
    {
        if (targetEntitiesController == null) { Debug.Log($"target Entities Controller wasnt found, returning..."); return; }
        if (statusEffectEntryContainerToApply == null) { Debug.Log($"status Effect Entry To Apply wasnt found, returning..."); return; }
        if (statusEffectsName == StatusEffectName.Default) { Debug.Log($"status Effects name was Default, returning..."); return; }
        if (attackingEntitiesController == null) { Debug.Log($"attacking Entities Controller was null... (should it have been?)"); }

        if (statusEffectsName == StatusEffectName.PoisoningStack)
        {
            if (targetEntitiesController.EntitiesActiveStatusEffects[statusEffectsName].Count >= 10)
            {
                RemoveStatusEffectStacks(targetEntitiesController, StatusEffectName.PoisoningStack, 10);
                ApplyStatusEffect(targetEntitiesController, statusEffectEntryContainerToApply, StatusEffectName.EnvenomationStack, attackingEntitiesController);
            }
        }

        if (statusEffectsName == StatusEffectName.ChilledStack)
        {
            if (targetEntitiesController.EntitiesActiveStatusEffects[statusEffectsName].Count >= 10)
            {
                RemoveStatusEffectStacks(targetEntitiesController, StatusEffectName.ChilledStack, 10);
                ApplyStatusEffect(targetEntitiesController, statusEffectEntryContainerToApply, StatusEffectName.FreezeEffect, attackingEntitiesController);
            }
        }

        if (statusEffectEntryContainerToApply.StatusEffectsDictionary.ContainsKey(statusEffectsName))
        {
            if (!targetEntitiesController.EntitiesActiveStatusEffects.ContainsKey(statusEffectsName))
            {
                targetEntitiesController.EntitiesActiveStatusEffects.Add(statusEffectsName, new());
            }

            targetEntitiesController.EntitiesActiveStatusEffects[statusEffectsName].Add(statusEffectEntryContainerToApply.StatusEffectsDictionary[statusEffectsName]);
        }

        if (debuffsThatApplyDamageOverTime.Contains(statusEffectsName))
        {
            iDefenseSequenceManager.ApplyDamageOverTime(targetEntitiesController, statusEffectEntryContainerToApply, statusEffectsName);
        }
        else if (debuffsThatReduceTargetsStats.Contains(statusEffectsName))
        {
            foreach (var effect in statusEffectEntryContainerToApply.StatusEffectsDictionary)
            {
                foreach (var stat in effect.Value.EffectsStats.StatEntryDictionary)
                {
                    StatEntryModifier currentStatEntryModifier = new StatEntryModifier(stat.Value.);
                }
            }
            iStatEntryManager.ApplyEnemyStatReductions(targetEntitiesController, statusEffectsName, 1, );
            StartCoroutine(StatReductionsCoroutine(targetEntitiesController, statusEffectEntryContainerToApply, statusEffectsName, attackingEntitiesController));
        }
        else if (debuffsThatRestrictTargetsActions.Contains(statusEffectsName))
        {
            StartCoroutine(ActionLockedEffectsCoroutine(targetEntitiesController, statusEffectEntryContainerToApply, statusEffectsName, attackingEntitiesController));

            if (statusEffectsName == StatusEffectName.ParalyzationStack)
            {
                StartCoroutine(MovementParalyzationCheckCoroutine(targetEntitiesController));
            }
        }
        else if (statusEffectsName == StatusEffectName.KnockbackEffect)
        {
            targetEntitiesController.EntitiesRunningStatusEffectsCoroutines[statusEffectsName] = true;
            StartCoroutine(PushPullEffectCoroutine(targetEntitiesController, statusEffectEntryContainerToApply, statusEffectsName, attackingEntitiesController));
        }
    }

    private void RemoveStatusEffectStacks(BaseEntityController targetEntitiesController, StatusEffectName statusEffectsName, int numberOfStacksToRemove)
    {
        if (targetEntitiesController != null)
        {
            if (targetEntitiesController.EntitiesActiveStatusEffects.TryGetValue(statusEffectsName, out var statusEffectEntries))
            {
                int stackAmountToRemove = Mathf.Min(numberOfStacksToRemove, statusEffectEntries.Count);
                statusEffectEntries.RemoveRange(0, stackAmountToRemove);

                // Do I need to call the removal of said stacks if they are part of the stat reductions
                // or will that be taken care of as the ienumerators internal lifecycle?
                if (statusEffectEntries.Count == 0)
                {
                    targetEntitiesController.EntitiesActiveStatusEffects.Remove(statusEffectsName);
                    targetEntitiesController.EntitiesRunningStatusEffectsCoroutines.Remove(statusEffectsName);
                }
            }
        }
    }
    
   

    private IEnumerator StatReductionsCoroutine(MonoBehaviour attackOrAoeController, BaseEntityController targetEntitiesController, StatusEffectName statusEffectsName)
    {
        if (targetEntitiesController == null) { yield break; }

        if (entitiesActiveStatusEffectCoroutines[targetEntitiesController].ContainsKey(statusEffectsName))
        {
            for (int i = 0; i < entitiesActiveStatusEffects[targetEntitiesController][statusEffectsName].Count; i++)
            {
                iStatManager.ApplyEnemyStatReductions(targetEntitiesController, statusEffectsName, attackOrAoeController);
            }
        }

        float duration = iStatManager.StatsTotalValue(targetEntitiesController, StatName.StatusEffectDuration, StatDictionaryToTarget.StatusEffectDict);

        while (targetEntitiesController != null && entitiesActiveStatusEffects[targetEntitiesController][statusEffectsName].Count > 0)
        {
            yield return new WaitForSeconds(GameManager.DefaultCoroutineWaitForSecondsTime + duration);

            iStatManager.RemoveEnemyStatReductions(targetEntitiesController, statusEffectsName, entitiesActiveStatusEffects[targetEntitiesController][statusEffectsName].Count);
            entitiesActiveStatusEffects[targetEntitiesController][statusEffectsName].Remove(entitiesActiveStatusEffects[targetEntitiesController][statusEffectsName].Count);
        }
    }


    private IEnumerator ActionLockedEffectsCoroutine(BaseEntityController targetEntitiesController, StatusEffectName statusEffectsName)
    {
        if (targetEntitiesController == null) { yield break; }

        float tickSpeed = iStatManager.StatsTotalValue(targetEntitiesController, StatName.StatusEffectDuration, StatDictionaryToTarget.StatusEffectDict, statusEffectsName);

        if (statusEffectsName == StatusEffectName.ParalyzationStack)
        {
            tickSpeed = iStatManager.StatsTotalValue(targetEntitiesController, StatName.StatusEffectDamageHitRate, StatDictionaryToTarget.StatusEffectDict, statusEffectsName);
        }

        CheckEntitiesCurrentState(targetEntitiesController);

        while (entitiesActiveStatusEffects[targetEntitiesController][statusEffectsName].Count > 0)
        {
            yield return new WaitForSeconds(GameManager.DefaultCoroutineWaitForSecondsTime + tickSpeed);
            entitiesActiveStatusEffects[targetEntitiesController][statusEffectsName].Remove(entitiesActiveStatusEffects[targetEntitiesController][statusEffectsName].Count);
        }

        CheckEntitiesCurrentState(targetEntitiesController);
    }

    private void CheckEntitiesCurrentState(BaseEntityController targetEntitiesController)
    {
        if (targetEntitiesController != null)
        {
            foreach (var debuff in entitiesActiveStatusEffects[targetEntitiesController])
            {
                if (debuffNames.Contains(debuff.Key))
                {
                    if (debuff.Value != null)
                    {
                        if (debuff.Value[debuff.Value.Count].BlocksEntitiesAbilityToMove)
                        {
                            targetEntitiesController.CanObjectMove = false;
                        }
                        else
                        {
                            targetEntitiesController.CanObjectMove = true;
                        }

                        if (debuff.Value[debuff.Value.Count].BlocksEntitiesAbilityToAttack)
                        {
                            targetEntitiesController.CanEntityAttack = false;
                        }
                        else
                        {
                            targetEntitiesController.CanEntityAttack = true;
                        }
                    }
                }
            }
        }
    }





    //iStatusEffectEntryManager.EntityParalyzationCheck(callingEntitiesController)
    public void EntitiesActionParalyzationCheck(BaseEntityController targetEntitiesController)
    {
        if (targetEntitiesController != null)
        {
            if (entitiesActiveStatusEffectCoroutines[targetEntitiesController].ContainsKey(StatusEffectName.ParalyzationStack))
            {
                if (entitiesActiveStatusEffects[targetEntitiesController][StatusEffectName.ParalyzationStack].Count > 0)
                {
                    float paralyzationStunChanceReduction = iStatManager.StatsTotalValue(targetEntitiesController, StatName.StatusEffectResistance, StatDictionaryToTarget.BaseStatDict) / 100.0f;
                    float paralyzationStunChance = iStatManager.StatsTotalValue(targetEntitiesController, StatName.StatusEffectInflictionChance, StatDictionaryToTarget.StatusEffectDict, StatusEffectName.ParalyzationStack) / 100.0f;
                    paralyzationStunChance -= (paralyzationStunChance * paralyzationStunChanceReduction);
                    paralyzationStunChance = Mathf.Max(0, paralyzationStunChance);

                    if (UnityEngine.Random.value <= paralyzationStunChance)
                    {
                        ApplyStunEffectCoroutine(null, targetEntitiesController, StatusEffectName.ParalyzationStack);
                    }
                }
            }
        }
    }

    private IEnumerator MovementParalyzationCheckCoroutine(BaseEntityController targetEntitiesController)
    {
        if (targetEntitiesController != null)
        {
            if (entitiesActiveStatusEffects[targetEntitiesController].ContainsKey(StatusEffectName.ParalyzationStack))
            {
                float waitTimer = iStatManager.StatsTotalValue(targetEntitiesController, StatName.StatusEffectDamageHitRate, StatDictionaryToTarget.StatusEffectDict, StatusEffectName.ParalyzationStack);
                float currentTimer = waitTimer;

                float paralyzationStunChanceReduction = iStatManager.StatsTotalValue(targetEntitiesController, StatName.StatusEffectResistance, StatDictionaryToTarget.BaseStatDict) / 100.0f;

                while (targetEntitiesController != null && entitiesActiveStatusEffects[targetEntitiesController][StatusEffectName.ParalyzationStack].Count > 0)
                {
                    if (targetEntitiesController.IsObjectMoving)
                    {
                        currentTimer -= Time.deltaTime;
                        if (currentTimer < 0.0f)
                        {
                            EntitiesActionParalyzationCheck(targetEntitiesController);

                            currentTimer = waitTimer;
                        }
                    }

                    yield return new WaitForSeconds(GameManager.DefaultCoroutineWaitForSecondsTime);
                }
            }
        }
    }

    private void ApplyStunEffectCoroutine(MonoBehaviour attackOrAoeController, BaseEntityController targetEntitiesController, StatusEffectName statusEffectsNameCausingStun)
    {
        if (targetEntitiesController != null)
        {
            if (!entitiesActiveStatusEffects[targetEntitiesController].ContainsKey(StatusEffectName.StunEffect))
            {
                entitiesActiveStatusEffects[targetEntitiesController].Add(StatusEffectName.StunEffect, new());
                if (attackOrAoeController == null)
                {
                    if (debuffStatusEffectDictionary.ContainsKey(StatusEffectName.StunEffect))
                    {
                        entitiesActiveStatusEffects[targetEntitiesController][StatusEffectName.StunEffect].Add(1, StatusEffectEntry.CopyStatusEffectEntry(debuffStatusEffectDictionary[StatusEffectName.StunEffect]));
                    }
                }
                else if (attackOrAoeController is AttackPrefabController attackPrefabController)
                {
                    if (attacksStatusEffects[attackPrefabController].ContainsKey(StatusEffectName.StunEffect))
                    {
                        entitiesActiveStatusEffects[targetEntitiesController][StatusEffectName.StunEffect].Add(1, StatusEffectEntry.CopyStatusEffectEntry(attacksStatusEffects[attackPrefabController][StatusEffectName.StunEffect]));
                    }
                }
                else if (attackOrAoeController is AreaOfEffectController areaOfEffectController)
                {
                    if (aoeStatusEffects[areaOfEffectController].ContainsKey(StatusEffectName.StunEffect))
                    {
                        entitiesActiveStatusEffects[targetEntitiesController][StatusEffectName.StunEffect].Add(1, StatusEffectEntry.CopyStatusEffectEntry(aoeStatusEffects[areaOfEffectController][StatusEffectName.StunEffect]));
                    }
                }
            }

            entitiesActiveStatusEffectCoroutines[targetEntitiesController][StatusEffectName.StunEffect].StunTimerForRunningStatusEffectsCoroutines = iStatManager.StatsTotalValue(targetEntitiesController, StatName.StatusEffectDamageHitRate, StatDictionaryToTarget.StatusEffectDict, statusEffectsNameCausingStun);
        }

        CheckEntitiesCurrentState(targetEntitiesController);

        if (entitiesActiveStatusEffectCoroutines[targetEntitiesController][StatusEffectName.StunEffect].RunningStatusEffectsCoroutine == null)
        {
            entitiesActiveStatusEffectCoroutines[targetEntitiesController][StatusEffectName.StunEffect].RunningStatusEffectsCoroutine = StartCoroutine(StunEffectCoroutine(targetEntitiesController));
        }
    }

    private IEnumerator StunEffectCoroutine(BaseEntityController targetEntitiesController)
    {
        while (targetEntitiesController != null && entitiesActiveStatusEffectCoroutines[targetEntitiesController][StatusEffectName.StunEffect].StunTimerForRunningStatusEffectsCoroutines > 0)
        {
            entitiesActiveStatusEffectCoroutines[targetEntitiesController][StatusEffectName.StunEffect].StunTimerForRunningStatusEffectsCoroutines -= Time.deltaTime;

            yield return new WaitForSeconds(GameManager.DefaultCoroutineWaitForSecondsTime);
        }

        CheckEntitiesCurrentState(targetEntitiesController);

        entitiesActiveStatusEffectCoroutines[targetEntitiesController].Remove(StatusEffectName.StunEffect);
    }

    private void ApplyPushPullEffectCoroutine(Vector3 targetDirectionOfKnockback, BaseEntityController targetEntitiesController, StatusEffectEntry statusEffectCausingKnockback)
    {
        if (targetEntitiesController != null)
        {
            if (entitiesActiveStatusEffectCoroutines[targetEntitiesController].ContainsKey(statusEffectCausingKnockback.StatusEffectsName))
            {
                CheckEntitiesCurrentState(targetEntitiesController);

                Vector3 locationToPushPullFrom = targetEntitiesController.transform.localPosition;
                Vector3 direction = targetDirectionOfKnockback - locationToPushPullFrom;
                direction.Normalize();

                if (!entitiesActiveStatusEffects[targetEntitiesController].ContainsKey(StatusEffectName.KnockbackEffect))
                {
                    entitiesActiveStatusEffects[targetEntitiesController].Add(StatusEffectName.KnockbackEffect, new());
                    entitiesActiveStatusEffects[targetEntitiesController][StatusEffectName.KnockbackEffect].Add(1, StatusEffectEntry.CopyStatusEffectEntry(statusEffectCausingKnockback));
                }

                if (entitiesActiveStatusEffectCoroutines[targetEntitiesController][StatusEffectName.KnockbackEffect].RunningStatusEffectsCoroutine == null)
                {
                    entitiesActiveStatusEffectCoroutines[targetEntitiesController][StatusEffectName.KnockbackEffect].RunningStatusEffectsCoroutine = StartCoroutine(PushPullEffectCoroutine(direction, targetEntitiesController, statusEffectCausingKnockback));
                }
            }
        }
    }

    private IEnumerator PushPullEffectCoroutine(Vector3 targetDirectionOfEffect, BaseEntityController targetEntitiesController, StatusEffectEntry statusEffectCausingEffect)
    {
        bool reachedDestination = false;
        while (!reachedDestination)
        {
            targetEntitiesController.transform.localPosition = Vector3.MoveTowards(targetEntitiesController.transform.localPosition, targetDirectionOfEffect, defaultPushPullEffectSpeed * Time.deltaTime);

            if (targetEntitiesController.transform.localPosition == targetDirectionOfEffect)
            {
                reachedDestination = true;
            }

            yield return new WaitForSeconds(GameManager.DefaultCoroutineWaitForSecondsTime);
        }

        float damageDealtMin = iStatManager.StatsTotalValue(targetEntitiesController, StatName.MinimumAttackDamage, StatDictionaryToTarget.StatusEffectDict, statusEffectCausingEffect.StatusEffectsName);
        float damageDealtMax = iStatManager.StatsTotalValue(targetEntitiesController, StatName.MinimumAttackDamage, StatDictionaryToTarget.StatusEffectDict, statusEffectCausingEffect.StatusEffectsName);

        float damageDealt = UnityEngine.Random.Range(damageDealtMin, damageDealtMax);

        iStatManager.ApplyDamageToEntity(null, null, targetEntitiesController, damageDealt, false, DamageType.Crushing, 0.0f, false);

        CheckEntitiesCurrentState(targetEntitiesController);
    }
}
