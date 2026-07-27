using UnityEngine;
<<<<<<< Updated upstream
=======
using System.Collections;
>>>>>>> Stashed changes
using System.Collections.Generic;

public class PassByAbilityEffect : BaseAbilityEffect
{
    public int damagePowerPercent = 70;
    public bool damageAdjacentToPath = true;
    public bool stopBeforeOccupiedDestination = true;

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        Unit attacker = GetComponentInParent<Unit>();
<<<<<<< Updated upstream
        PathAbilityArea area = GetComponentInParent<Ability>().GetComponent<PathAbilityArea>();
        Board board = GameObject.FindObjectOfType<Board>();
        if (attacker == null || area == null || area.tiles == null || area.tiles.Count == 0 || board == null)
            return 0;

        List<Tile> path = new List<Tile>(area.tiles);
        if (stopBeforeOccupiedDestination && path.Count > 0 && path[path.Count - 1].content != null)
            path.RemoveAt(path.Count - 1);
=======
        Board board = GameObject.FindObjectOfType<Board>();
        if (attacker == null || attacker.tile == null || target == null || board == null)
            return 0;

        List<Tile> path = GetManualPath(board, attacker.tile, target);
>>>>>>> Stashed changes
        if (path.Count == 0)
            return 0;

        HashSet<Unit> damaged = new HashSet<Unit>();
        int total = 0;
        for (int i = 0; i < path.Count; ++i)
        {
            Tile step = path[i];
<<<<<<< Updated upstream
=======
            if (step == null)
                continue;

>>>>>>> Stashed changes
            DamageEnemyOnTile(attacker, step, damaged, ref total);
            if (damageAdjacentToPath)
                DamageAdjacentEnemies(board, attacker, step, damaged, ref total);
        }

        Tile destination = path[path.Count - 1];
        if (destination != null && destination.content == null)
        {
            attacker.Place(destination);
            attacker.Match();
        }
<<<<<<< Updated upstream
        return -total;
    }

=======

        return -total;
    }

    List<Tile> GetManualPath(Board board, Tile start, Tile end)
    {
        Ability ability = GetComponentInParent<Ability>();
        PathAbilityArea pathArea = ability != null ? ability.GetComponent<PathAbilityArea>() : null;
        if (pathArea != null && pathArea.tiles != null && pathArea.tiles.Count > 0)
            return new List<Tile>(pathArea.tiles);

        return BuildFallbackPath(board, start, end);
    }

    List<Tile> BuildFallbackPath(Board board, Tile start, Tile end)
    {
        List<Tile> path = new List<Tile>();
        if (board == null || start == null || end == null)
            return path;

        Point current = start.pos;
        int guard = 0;
        while (current != end.pos && guard++ < 64)
        {
            Point next = current;
            int dx = end.pos.x - current.x;
            int dy = end.pos.y - current.y;

            if (Mathf.Abs(dx) >= Mathf.Abs(dy) && dx != 0)
                next.x += dx > 0 ? 1 : -1;
            else if (dy != 0)
                next.y += dy > 0 ? 1 : -1;
            else if (dx != 0)
                next.x += dx > 0 ? 1 : -1;

            Tile tile = board.GetTile(next);
            if (tile == null)
                break;

            path.Add(tile);
            current = next;
        }

        if (stopBeforeOccupiedDestination && path.Count > 0 && path[path.Count - 1].content != null)
            path.RemoveAt(path.Count - 1);

        return path;
    }

>>>>>>> Stashed changes
    void DamageAdjacentEnemies(Board board, Unit attacker, Tile center, HashSet<Unit> damaged, ref int total)
    {
        Point[] offsets = new Point[]
        {
            new Point(0, 1),
            new Point(1, 0),
            new Point(0, -1),
            new Point(-1, 0)
        };

        for (int i = 0; i < offsets.Length; ++i)
<<<<<<< Updated upstream
            DamageEnemyOnTile(attacker, board.GetTile(center.pos + offsets[i]), damaged, ref total);
=======
        {
            Tile tile = board.GetTile(center.pos + offsets[i]);
            DamageEnemyOnTile(attacker, tile, damaged, ref total);
        }
>>>>>>> Stashed changes
    }

    void DamageEnemyOnTile(Unit attacker, Tile tile, HashSet<Unit> damaged, ref int total)
    {
        if (tile == null || tile.content == null)
            return;
<<<<<<< Updated upstream
        Unit defender = tile.content.GetComponent<Unit>();
        if (defender == null || defender == attacker || damaged.Contains(defender))
            return;
        Alliance attackerAlliance = attacker.GetComponentInChildren<Alliance>();
        Alliance defenderAlliance = defender.GetComponentInChildren<Alliance>();
        if (attackerAlliance == null || defenderAlliance == null || !attackerAlliance.IsMatch(defenderAlliance, Targets.Foe))
            return;
=======

        Unit defender = tile.content.GetComponent<Unit>();
        if (defender == null || damaged.Contains(defender) || defender == attacker)
            return;
        if (!IsEnemy(attacker, defender))
            return;

>>>>>>> Stashed changes
        Stats defenderStats = defender.GetComponent<Stats>();
        if (defenderStats == null || defenderStats[StatTypes.HP] <= 0)
            return;

        int damage = CalculateDamage(attacker, defender);
        defenderStats[StatTypes.HP] -= damage;
        damaged.Add(defender);
        total += damage;
    }

<<<<<<< Updated upstream
=======
    bool IsEnemy(Unit attacker, Unit defender)
    {
        Alliance attackerAlliance = attacker.GetComponent<Alliance>();
        Alliance defenderAlliance = defender.GetComponent<Alliance>();
        if (attackerAlliance == null || defenderAlliance == null)
            return false;
        return attackerAlliance.type != defenderAlliance.type && defenderAlliance.type != Alliances.Neutral;
    }

>>>>>>> Stashed changes
    int CalculateDamage(Unit attacker, Unit defender)
    {
        Stats attackerStats = attacker.GetComponent<Stats>();
        Stats defenderStats = defender.GetComponent<Stats>();
        if (attackerStats == null || defenderStats == null)
            return 1;
<<<<<<< Updated upstream
        int damage = attackerStats[StatTypes.STR] - (defenderStats[StatTypes.DEF] / 2);
=======

        int attack = attackerStats[StatTypes.STR];
        int defense = defenderStats[StatTypes.DEF];
        int damage = attack - (defense / 2);
>>>>>>> Stashed changes
        damage = Mathf.Max(damage, 1);
        damage = Mathf.Max(1, damagePowerPercent * damage / 100);
        return Mathf.Clamp(damage, 1, 999);
    }
}
