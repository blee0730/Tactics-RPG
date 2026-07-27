using UnityEngine;
using System.Collections;

public class SummonUnitAbilityEffect : BaseAbilityEffect
{
	[Header("Summon")]
	public string unitRecipeName;
	public int levelOverride = 0;
	public bool useCasterLevel = true;
	public bool matchCasterAlliance = true;
	public Alliances allianceOverride = Alliances.None;
	public bool requireEmptyTile = true;
	public bool faceSameDirectionAsCaster = true;
	public bool addToBattleUnitList = true;

	public override int Predict (Tile target)
	{
		return 0;
	}

	protected override int OnApply (Tile target)
	{
		if (target == null)
			return 0;
		if (requireEmptyTile && target.content != null)
			return 0;
		if (string.IsNullOrEmpty(unitRecipeName))
		{
			Debug.LogWarning("SummonUnitAbilityEffect needs a Unit Recipe Name.");
			return 0;
		}

		Unit caster = GetComponentInParent<Unit>();
		int level = ResolveLevel(caster);
		GameObject summoned = UnitFactory.Create(unitRecipeName, level);
		if (summoned == null)
			return 0;

		Unit summonedUnit = summoned.GetComponent<Unit>();
		if (summonedUnit == null)
			return 0;

		ApplyAlliance(caster, summoned);
		if (faceSameDirectionAsCaster && caster != null)
			summonedUnit.dir = caster.dir;

		summonedUnit.Place(target);
		summonedUnit.Match();

		if (addToBattleUnitList)
		{
			BattleController bc = GameObject.FindObjectOfType<BattleController>();
			if (bc != null && !bc.units.Contains(summonedUnit))
				bc.units.Add(summonedUnit);
		}

		return 0;
	}

	int ResolveLevel (Unit caster)
	{
		if (levelOverride > 0)
			return levelOverride;

		if (useCasterLevel && caster != null)
		{
			Stats stats = caster.GetComponent<Stats>();
			if (stats != null)
				return Mathf.Max(1, stats[StatTypes.LVL]);
		}

		return 1;
	}

	void ApplyAlliance (Unit caster, GameObject summoned)
	{
		Alliance summonAlliance = summoned.GetComponentInChildren<Alliance>();
		if (summonAlliance == null)
			summonAlliance = summoned.AddComponent<Alliance>();

		if (allianceOverride != Alliances.None)
		{
			summonAlliance.type = allianceOverride;
			return;
		}

		if (matchCasterAlliance && caster != null)
		{
			Alliance casterAlliance = caster.GetComponentInChildren<Alliance>();
			if (casterAlliance != null)
				summonAlliance.type = casterAlliance.type;
		}
	}
}
