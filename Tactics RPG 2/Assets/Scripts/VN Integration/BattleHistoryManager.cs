using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UnitSnapshot
{
    public string unitName;
    public int    hp;
    public int    mp;
    public int    x;
    public int    y;
    public bool   isAlive;
}

[System.Serializable]
public class BattleSnapshot
{
    public int    turnNumber;
    public string activeUnitName;
    public List<UnitSnapshot> units = new List<UnitSnapshot>();
    public string timestamp;

    public static BattleSnapshot Capture(BattleController bc, int turnNumber)
    {
        var snap = new BattleSnapshot();
        snap.turnNumber     = turnNumber;
        snap.activeUnitName = bc.turn?.actor != null ? bc.turn.actor.name : "";
        snap.timestamp      = System.DateTime.Now.ToString("HH:mm:ss");

        // Board stores tiles in topTiles dictionary — find all occupied ones
        foreach (var kvp in bc.board.topTiles)
        {
            Tile tile = kvp.Value;
            if (tile.content == null) continue;

            Unit unit = tile.content.GetComponent<Unit>();
            if (unit == null) continue;

            Stats stats = unit.GetComponent<Stats>();
            if (stats == null) continue;

            var us      = new UnitSnapshot();
            us.unitName = unit.name;
            us.hp       = stats[StatTypes.HP];
            us.mp       = stats[StatTypes.MP];
            us.x        = tile.pos.x;
            us.y        = tile.pos.y;
            us.isAlive  = stats[StatTypes.HP] > 0;
            snap.units.Add(us);
        }

        return snap;
    }
}

// Add this component to the BattleController GameObject.
// Records a BattleSnapshot at the end of each turn.
// Time-magic abilities call Rewind(steps) to roll the board back.
public class BattleHistoryManager : MonoBehaviour
{
    public const string SnapshotNotification = "BattleHistory.SnapshotTaken";
    public const string RewindNotification   = "BattleHistory.Rewound";

    [Tooltip("How many turns back time magic can reach")]
    public int maxHistory = 10;

    public static BattleHistoryManager instance { get; private set; }

    readonly List<BattleSnapshot> history = new List<BattleSnapshot>();
    BattleController bc;
    int turnCount;

    void Awake()
    {
        instance = this;
        bc       = GetComponent<BattleController>();
    }

    void OnEnable()
    {
        this.AddObserver(OnTurnCompleted, TurnOrderController.TurnCompletedNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnTurnCompleted, TurnOrderController.TurnCompletedNotification);
    }

    void OnTurnCompleted(object sender, object args)
    {
        turnCount++;
        TakeSnapshot();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    public void TakeSnapshot()
    {
        BattleSnapshot snap = BattleSnapshot.Capture(bc, turnCount);
        history.Add(snap);

        if (history.Count > maxHistory)
            history.RemoveAt(0);

        this.PostNotification(SnapshotNotification, snap);
        Debug.Log($"[BattleHistory] Turn {turnCount} snapshot — {history.Count} in history.");
    }

    public int  AvailableRewinds          => history.Count;
    public bool CanRewind(int steps = 1)  => history.Count >= steps;

    public BattleSnapshot Rewind(int steps = 1)
    {
        if (!CanRewind(steps))
        {
            Debug.LogWarning("[BattleHistory] Not enough history to rewind that far.");
            return null;
        }

        int index = history.Count - steps;
        BattleSnapshot target = history[index];

        history.RemoveRange(index, history.Count - index);

        ApplySnapshot(target);
        this.PostNotification(RewindNotification, target);

        Debug.Log($"[BattleHistory] Rewound {steps} turn(s) to turn {target.turnNumber}.");
        return target;
    }

    // ── Snapshot apply ────────────────────────────────────────────────────────

    void ApplySnapshot(BattleSnapshot snap)
    {
        foreach (var us in snap.units)
        {
            // Find the unit by name in the scene
            GameObject go = GameObject.Find(us.unitName);
            if (go == null) continue;

            Unit  unit  = go.GetComponent<Unit>();
            Stats stats = go.GetComponent<Stats>();
            if (unit == null || stats == null) continue;

            // Restore HP and MP directly via Stats
            stats.SetValue(StatTypes.HP, us.hp, false);
            stats.SetValue(StatTypes.MP, us.mp, false);

            // Restore position if unit has moved
            Tile targetTile = bc.board.GetTile(new Point(us.x, us.y));
            if (targetTile != null && unit.tile != targetTile)
                unit.Place(targetTile);
        }
    }
}
