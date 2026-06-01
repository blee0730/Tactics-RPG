using UnityEngine;

public class AbilityEvolutionController : MonoBehaviour
{
    Ability ability;
    AbilityMastery mastery;

    void Awake()
    {
        ability = GetComponent<Ability>();
        mastery = GetComponent<AbilityMastery>();
        this.AddObserver(OnMasteryLevelUp, "AbilityMastery.LevelUp");
    }

    void OnDestroy()
    {
        this.RemoveObserver(OnMasteryLevelUp, "AbilityMastery.LevelUp");
    }

    void OnMasteryLevelUp(object sender, object args)
    {
        CheckUnlocks();
    }

    void CheckUnlocks()
    {
        if (ability.data == null || ability.data.evolutions == null)
            return;

        foreach (var evolution in ability.data.evolutions)
        {
            // == fires exactly once when the level is first reached
            // >= would re-fire every previous evolution on every future level-up
            if (mastery.level == evolution.requiredLevel)
            {
                this.PostNotification(
                    "AbilityUnlocked",
                    new AbilityUnlockNotification(evolution.unlockAbility)
                );
            }
        }
    }
}
