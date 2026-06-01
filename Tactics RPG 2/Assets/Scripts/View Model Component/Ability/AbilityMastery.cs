using UnityEngine;

public class AbilityMastery : MonoBehaviour
{
    public Ability ability;
    public int masteryXP;
    public int level;

    public int XPNeeded()
    {
        return (int)Mathf.Pow(10, level + 1);
    }

    public MasteryRank Rank
    {
        get
        {
            int maxRank = System.Enum.GetValues(typeof(MasteryRank)).Length - 1;
            return (MasteryRank)Mathf.Min(level, maxRank);
        }
    }

    public void RegisterUse()
    {
        masteryXP++;

        // Must subtract XPNeeded() BEFORE incrementing level to avoid infinite loop
        while (masteryXP >= XPNeeded())
        {
            masteryXP -= XPNeeded();
            level++;
            this.PostNotification("AbilityMastery.LevelUp", level);
        }
    }
}
