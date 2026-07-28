using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class BattleState : State 
{
	protected BattleController owner;
	protected Driver driver;
	public CameraRig cameraRig { get { return owner.cameraRig; }}
	public Board board { get { return owner.board; }}
	public LevelData levelData { get { return owner.levelData; }}
	public Transform tileSelectionIndicator { get { return owner.tileSelectionIndicator; }}
	public Point pos { get { return owner.pos; } set { owner.pos = value; }}
	public Tile currentTile { get { return owner.currentTile; }}
	public AbilityMenuPanelController abilityMenuPanelController { get { return owner.abilityMenuPanelController; }}
	public StatPanelController statPanelController { get { return owner.statPanelController; }}
	public HitSuccessIndicator hitSuccessIndicator { get { return owner.hitSuccessIndicator; }}
	public Turn turn { get { return owner.turn; }}
	public List<Unit> units { get { return owner.units; }}

	protected virtual void Awake ()
	{
		owner = GetComponent<BattleController>();
	}

	protected override void AddListeners ()
	{
		if (driver == null || driver.Current == Drivers.Human)
		{
			InputController.moveEvent += OnMove;
			InputController.fireEvent += OnFire;
			InputController.layerCycleEvent += OnCycleLayer;
		}
	}
	
	protected override void RemoveListeners ()
	{
		InputController.moveEvent -= OnMove;
		InputController.fireEvent -= OnFire;
		InputController.layerCycleEvent -= OnCycleLayer;
	}

	public override void Enter ()
	{
		driver = (turn.actor != null) ? turn.actor.GetComponent<Driver>() : null;
		base.Enter ();
	}

	protected virtual void OnMove (object sender, InfoEventArgs<Point> e)
	{
		
	}
	
	protected virtual void OnFire (object sender, InfoEventArgs<int> e)
	{
		
	}

	protected virtual void OnCycleLayer (object sender, InfoEventArgs<int> e)
	{
		CycleTileLayer(e.info);
	}

	protected virtual void SelectTile (Point p)
	{
		if (pos == p && owner.selectedTile != null)
			return;

		Tile tile = owner.currentTile != null
			? board.GetClosestSelectableTile(p, owner.currentTile.height)
			: board.GetTile(p);

		if (tile == null)
			return;

		SelectTile(tile);
	}

	protected virtual void SelectTile (Point p, List<Tile> allowedTiles)
	{
		if (allowedTiles == null)
		{
			SelectTile(p);
			return;
		}

		List<Tile> stack = board.GetSelectableTiles(p);
		if (stack.Count == 0)
			return;

		float preferredHeight = owner.currentTile != null ? owner.currentTile.height : float.MaxValue;
		Tile best = null;
		float bestDifference = float.MaxValue;
		for (int i = 0; i < stack.Count; ++i)
		{
			Tile candidate = stack[i];
			if (!allowedTiles.Contains(candidate))
				continue;

			float difference = Mathf.Abs(candidate.height - preferredHeight);
			if (best == null || difference < bestDifference)
			{
				best = candidate;
				bestDifference = difference;
			}
		}

		if (best != null)
			SelectTile(best);
	}

	protected virtual void SelectTile (Tile tile)
	{
		if (tile == null || owner.selectedTile == tile)
			return;

		pos = tile.pos;
		owner.selectedTile = tile;

		Vector3 indicatorPosition = tile.center;
		float indicatorOffset = owner.tileSelectionIndicatorYOffset;
		if (Mathf.Approximately(indicatorOffset, 0f))
			indicatorOffset = 0.08f;
		indicatorPosition.y += indicatorOffset;
		tileSelectionIndicator.localPosition = indicatorPosition;
	}

	protected virtual void CycleTileLayer (int direction)
	{
		CycleTileLayer(direction, null);
	}

	protected virtual void CycleTileLayer (int direction, List<Tile> allowedTiles)
	{
		List<Tile> stack = board.GetSelectableTiles(pos);
		if (stack.Count <= 1)
			return;

		List<Tile> candidates = new List<Tile>();
		for (int i = 0; i < stack.Count; ++i)
		{
			if (allowedTiles == null || allowedTiles.Contains(stack[i]))
				candidates.Add(stack[i]);
		}

		if (candidates.Count <= 1)
			return;

		int index = candidates.IndexOf(owner.currentTile);
		if (index < 0)
			index = candidates.IndexOf(board.GetTile(pos));
		if (index < 0)
			index = candidates.Count - 1;

		// With two selectable layers, Shift and Alt both behave as a clean toggle.
		// With three or more layers, Shift moves forward through the sorted stack
		// and Alt moves backward.
		int step = candidates.Count == 2 ? 1 : (direction >= 0 ? 1 : -1);
		int nextIndex = index + step;
		while (nextIndex < 0)
			nextIndex += candidates.Count;
		nextIndex %= candidates.Count;

		SelectTile(candidates[nextIndex]);
	}

	protected virtual Unit GetUnit (Point p)
	{
		Tile t = (p == pos && owner.currentTile != null) ? owner.currentTile : board.GetTile(p);
		GameObject content = t != null ? t.content : null;
		return content != null ? content.GetComponent<Unit>() : null;
	}

	protected virtual void RefreshPrimaryStatPanel (Point p)
	{
		Unit target = GetUnit(p);
		if (target != null)
			statPanelController.ShowPrimary(target.gameObject);
		else
			statPanelController.HidePrimary();
	}

	protected virtual void RefreshSecondaryStatPanel (Point p)
	{
		Unit target = GetUnit(p);
		if (target != null && target != turn.actor)
			statPanelController.ShowSecondary(target.gameObject);
		else
			statPanelController.HideSecondary();
	}

	protected virtual bool DidPlayerWin ()
	{
		return owner.GetComponent<BaseVictoryCondition>().Victor == Alliances.Hero;
	}
	
	protected virtual bool IsBattleOver ()
	{
		return owner.GetComponent<BaseVictoryCondition>().Victor != Alliances.None;
	}
}
