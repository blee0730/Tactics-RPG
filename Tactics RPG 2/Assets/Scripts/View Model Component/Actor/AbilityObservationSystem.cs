using System.Linq;
using UnityEngine;

public class AbilityObservationSystem : MonoBehaviour
{
    AbilityJournal journal;
    AbilityUnlockManager unlockManager;

    void Awake()
    {
        journal = GetComponent<AbilityJournal>();
        unlockManager = GetComponent<AbilityUnlockManager>();
    }

    public void Observe(AbilityData abilityData)
    {
        ObservedAbility observed = journal.observed
            .FirstOrDefault(x => x.abilityName == abilityData.abilityName);

        if (observed == null)
        {
            observed = new ObservedAbility();
            observed.abilityName = abilityData.abilityName;
            journal.observed.Add(observed);
        }

        observed.timesSeen++;

        // Check unlock every time we observe
        if (unlockManager != null)
            unlockManager.TryUnlock(abilityData);
    }

    public int TimesSeen(string abilityName)
    {
        foreach (var item in journal.observed)
            if (item.abilityName == abilityName)
                return item.timesSeen;
        return 0;
    }
}
