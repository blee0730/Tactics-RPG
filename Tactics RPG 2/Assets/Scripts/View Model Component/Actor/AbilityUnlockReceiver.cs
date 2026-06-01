using UnityEngine;

// Add this to Rein's unit alongside the other Analyze components.
// It receives the "AbilityUnlocked" notification and writes it into
// UnlockedAbilityCollection, completing the observation -> unlock chain.
public class AbilityUnlockReceiver : MonoBehaviour
{
    UnlockedAbilityCollection collection;

    void Awake()
    {
        collection = GetComponent<UnlockedAbilityCollection>();
        this.AddObserver(OnAbilityUnlocked, "AbilityUnlocked");
    }

    void OnDestroy()
    {
        this.RemoveObserver(OnAbilityUnlocked, "AbilityUnlocked");
    }

    void OnAbilityUnlocked(object sender, object args)
    {
        AbilityUnlockNotification notification = (AbilityUnlockNotification)args;

        if (notification.unlockedAbility == null)
            return;

        string name = notification.unlockedAbility.abilityName;

        if (!collection.Contains(name))
        {
            collection.Add(name);
            Debug.Log($"[Analyze] Rein unlocked: {name}");
            // TODO: hook in UI notification / fanfare here
        }
    }
}
