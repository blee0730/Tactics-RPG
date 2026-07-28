using UnityEngine;
using System.Collections.Generic;

public class PortalTileEffect : MonoBehaviour
{
    public PortalTileEffect linkedPortal;
    public int durationRounds = 3;
    public bool removeWhenExpired = true;
    public bool requireEmptyExit = true;

    static Dictionary<Unit, float> recentlyTeleported = new Dictionary<Unit, float>();
    Tile owner;

    void OnEnable()
    {
        owner = GetComponentInParent<Tile>();
        this.AddObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification);
        this.AddObserver(OnRoundEnded, TurnOrderController.RoundEndedNotification);
    }

    void OnDisable()
    {
        this.RemoveObserver(OnTurnBegan, TurnOrderController.TurnBeganNotification);
        this.RemoveObserver(OnRoundEnded, TurnOrderController.RoundEndedNotification);
    }

    void OnTriggerEnter(Collider other)
    {
        Unit unit = other.GetComponentInParent<Unit>();
        TryTeleport(unit);
    }

    void OnTurnBegan(object sender, object args)
    {
        Unit unit = sender as Unit;
        if (unit != null && owner != null && unit.tile == owner)
            TryTeleport(unit);
    }

    void OnRoundEnded(object sender, object args)
    {
        if (durationRounds < 0)
            return;
        durationRounds--;
        if (durationRounds <= 0 && removeWhenExpired)
            Destroy(gameObject);
    }

    void TryTeleport(Unit unit)
    {
        if (unit == null || linkedPortal == null)
            return;
        if (recentlyTeleported.ContainsKey(unit) && Time.time - recentlyTeleported[unit] < 0.5f)
            return;

        Tile exit = linkedPortal.GetComponentInParent<Tile>();
        if (exit == null)
            return;

        Tile landing = FindLandingTile(exit);
        if (landing == null)
            return;

        recentlyTeleported[unit] = Time.time;
        unit.Place(landing);
        unit.Match();
    }

    Tile FindLandingTile(Tile exit)
    {
        if (exit.content == null || !requireEmptyExit)
            return exit;

        Board board = GameObject.FindObjectOfType<Board>();
        if (board == null)
            return null;

        Point[] offsets = new Point[] { new Point(0, 1), new Point(1, 0), new Point(0, -1), new Point(-1, 0) };
        for (int i = 0; i < offsets.Length; ++i)
        {
            Tile tile = board.GetTile(exit.pos + offsets[i]);
            if (tile != null && tile.content == null)
                return tile;
        }
        return null;
    }
}
