using UnityEngine;

public class AbilityUnlockManager : MonoBehaviour
{
    AbilityJournal journal;

    void Awake()
    {
        journal = GetComponent<AbilityJournal>();
    }

    public bool HasObservedEnough(AbilityData ability)
    {
        foreach (var observed in journal.observed)
        {
            if (observed.abilityName == ability.abilityName)
                return observed.timesSeen >= ability.observationRequirement;
        }
        return false;
    }

    public void TryUnlock(AbilityData ability)
    {
        if (!HasObservedEnough(ability))
            return;

        // Guard against firing the notification repeatedly once already unlocked
        UnlockedAbilityCollection collection = GetComponent<UnlockedAbilityCollection>();
        if (collection != null && collection.Contains(ability.abilityName))
            return;

        this.PostNotification("AbilityUnlocked", new AbilityUnlockNotification(ability));
    }
}
