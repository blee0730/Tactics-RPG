using UnityEngine;

public class RewindTimeMemory : MonoBehaviour
{
    public int previousHP;
    public int previousMP;
    public Tile previousTile;
    public Directions previousFacing;
    public bool hasPreviousSnapshot;

    Unit owner;
    Stats stats;

    void OnEnable()
    {
        owner = GetComponent<Unit>();
        stats = GetComponent<Stats>();
        if (owner != null)
            this.AddObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, owner);
    }

    void OnDisable()
    {
        if (owner != null)
            this.RemoveObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification, owner);
    }

    void OnTurnBegan(object sender, object args)
    {
        CaptureSnapshot();
    }

    public void CaptureSnapshot()
    {
        if (owner == null)
            owner = GetComponent<Unit>();
        if (stats == null)
            stats = GetComponent<Stats>();
        if (owner == null || stats == null)
            return;

        previousHP = stats[StatTypes.HP];
        previousMP = stats[StatTypes.MP];
        previousTile = owner.tile;
        previousFacing = owner.dir;
        hasPreviousSnapshot = true;
    }

    public bool RestoreSnapshot()
    {
        if (!hasPreviousSnapshot)
            return false;
        if (owner == null)
            owner = GetComponent<Unit>();
        if (stats == null)
            stats = GetComponent<Stats>();
        if (owner == null || stats == null)
            return false;

        stats.SetValue(StatTypes.HP, Mathf.Clamp(previousHP, 0, stats[StatTypes.MHP]), false);
        stats.SetValue(StatTypes.MP, Mathf.Clamp(previousMP, 0, stats[StatTypes.MMP]), false);

        if (previousTile != null && (previousTile.content == null || previousTile.content == owner.gameObject))
        {
            owner.Place(previousTile);
            owner.dir = previousFacing;
            owner.Match();
        }
        return true;
    }
}
