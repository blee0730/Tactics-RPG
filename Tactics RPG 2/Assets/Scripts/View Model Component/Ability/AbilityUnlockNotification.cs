public struct AbilityUnlockNotification
{
    public AbilityData unlockedAbility;

    public AbilityUnlockNotification(
        AbilityData ability)
    {
        unlockedAbility = ability;
    }
}