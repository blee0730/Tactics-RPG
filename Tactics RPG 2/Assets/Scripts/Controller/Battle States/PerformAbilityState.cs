using UnityEngine;
using System.Collections;

public class PerformAbilityState : BattleState
{
	AbilityArea aa;
	Tile target;
	Unit defender;
	Stats s;
	int currentHP;
	int maxHP;
	Point startPos;
	Tile midTile;
	Tile endTile;
	public override void Enter()
	{
		base.Enter();
		target = turn.targets[0];
		defender = target.content.GetComponent<Unit>();
		s = target.content.GetComponentInParent<Stats>();
		currentHP = s[StatTypes.HP];
		maxHP = s[StatTypes.MHP];
		startPos = target.pos;
		midTile = null;
		endTile = null;
		aa = turn.ability.GetComponent<AbilityArea>();
		turn.hasUnitActed = true;
		if (turn.hasUnitMoved)
			turn.lockMove = true;
		if (aa.count > 1)
		{
			aa.tiles.Clear();
			aa.counter = aa.count;
		}
			StartCoroutine(Animate());
	}
	
	IEnumerator Animate ()
	{
		// TODO play animations, etc
		yield return null;
		turn.ability.Perform(turn.targets);

		if (aa.displace == true)
			Displace();

		if (IsBattleOver())
			owner.ChangeState<CutSceneState>();
		else if (!UnitHasControl())
			owner.ChangeState<SelectUnitState>();
		else if (turn.hasUnitMoved)
			owner.ChangeState<EndFacingState>();
		else
			owner.ChangeState<CommandSelectionState>();
	}

	void Displace()
	{
		switch (aa.dir)
		{
			case Directions.North:
				if (board.GetTile(startPos + new Point(0, 1)) != null)
					midTile = board.GetTile(startPos + new Point(0, 1));
				if (midTile != null && board.GetTile(startPos + new Point(0, 2)) != null)
					midTile = board.GetTile(startPos + new Point(0, 2));
				break;
			case Directions.East:
				if (board.GetTile(startPos + new Point(1, 0)) != null)
					midTile = board.GetTile(startPos + new Point(1, 0));
				if (midTile != null && board.GetTile(startPos + new Point(2, 0)) != null)
					midTile = board.GetTile(startPos + new Point(2, 0));
				break;
			case Directions.South:
				if (board.GetTile(startPos + new Point(0, -1)) != null)
					midTile = board.GetTile(startPos + new Point(0, -1));
				if (midTile != null && board.GetTile(startPos + new Point(0, -2)) != null)
					midTile = board.GetTile(startPos + new Point(0, -2));
				break;
			default: //West
				if (board.GetTile(startPos + new Point(-1, 0)) != null)
					midTile = board.GetTile(startPos + new Point(-1, 0));
				if (midTile != null && board.GetTile(startPos + new Point(-2, 0)) != null)
					midTile = board.GetTile(startPos + new Point(-2, 0));
				break;
		}
		if (midTile == null && endTile == null)
			return;
		if(midTile != null && endTile == null)
        {
			if (target.height > midTile.height)
			{
				defender.Place(midTile);
				defender.Match();
				float fallDistance = target.height - midTile.height;
				if (s[StatTypes.JMP] < fallDistance)
				{
					int reduce = Mathf.Min(currentHP, Mathf.FloorToInt(maxHP * 0.1f * fallDistance));
					s.SetValue(StatTypes.HP, currentHP - reduce, false);
				}
				if (midTile.content != null)
				{
					//find content and reduce health
				}
			}
			else if ((target.height < endTile.height) && midTile.content == null)
			{
				defender.Place(midTile);
				defender.Match();
				int reduce = Mathf.Min(currentHP, Mathf.FloorToInt(maxHP * 0.1f));
				s.SetValue(StatTypes.HP, (currentHP - reduce), false);
			}
			else if ((target.height > endTile.height) && endTile.content == null)
			{
				defender.Place(endTile);
				defender.Match();
				float fallDistance = target.height - endTile.height;
				if (s[StatTypes.JMP] < fallDistance)
				{
					int reduce = Mathf.Min(currentHP, Mathf.FloorToInt(maxHP * 0.1f * fallDistance));
					s.SetValue(StatTypes.HP, currentHP - reduce, false);
				}
			}
			else if(endTile.content == null)
            {
                defender.Place(endTile);
				defender.Match();
            }
        }
	}
	
	bool UnitHasControl ()
	{
		return turn.actor.GetComponentInChildren<KnockOutStatusEffect>() == null;
	}
}