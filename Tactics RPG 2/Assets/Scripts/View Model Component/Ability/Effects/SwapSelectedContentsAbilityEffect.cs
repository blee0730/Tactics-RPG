using UnityEngine;
<<<<<<< Updated upstream

public class SwapSelectedContentsAbilityEffect : BaseAbilityEffect
{
    int lastFrame = -1;

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        if (Time.frameCount == lastFrame)
            return 0;
        AbilityArea area = GetComponentInParent<Ability>().GetComponent<AbilityArea>();
        if (area == null || area.tiles == null || area.tiles.Count < 2)
            return 0;
        Tile a = area.tiles[0];
        Tile b = area.tiles[1];
        if (a == null || b == null || a.content == null || b.content == null)
            return 0;

        GameObject aContent = a.content;
        GameObject bContent = b.content;
        Unit aUnit = aContent.GetComponent<Unit>();
        Unit bUnit = bContent.GetComponent<Unit>();
        if (aUnit != null) aUnit.Place(b); else { b.content = aContent; aContent.transform.localPosition = b.center; }
        if (bUnit != null) bUnit.Place(a); else { a.content = bContent; bContent.transform.localPosition = a.center; }
        if (aUnit != null) aUnit.Match();
        if (bUnit != null) bUnit.Match();
        lastFrame = Time.frameCount;
        return 0;
    }
=======
using System.Collections;

public class SwapSelectedContentsAbilityEffect : BaseAbilityEffect
{
	public override int Predict (Tile target)
	{
		return 0;
	}

	protected override int OnApply (Tile target)
	{
		AbilityArea area = GetComponentInParent<AbilityArea>();
		if (area == null || area.tiles == null || area.tiles.Count < 2)
			return 0;

		Tile a = area.tiles[0];
		Tile b = area.tiles[1];
		if (a == null || b == null || a.content == null || b.content == null)
			return 0;

		GameObject aContent = a.content;
		GameObject bContent = b.content;
		Unit aUnit = aContent.GetComponent<Unit>();
		Unit bUnit = bContent.GetComponent<Unit>();

		if (aUnit != null)
			aUnit.Place(b);
		else
		{
			a.content = null;
			b.content = aContent;
			aContent.transform.localPosition = b.center;
		}

		if (bUnit != null)
			bUnit.Place(a);
		else
		{
			b.content = null;
			a.content = bContent;
			bContent.transform.localPosition = a.center;
		}

		if (aUnit != null)
			aUnit.Match();
		if (bUnit != null)
			bUnit.Match();

		return 0;
	}
>>>>>>> Stashed changes
}
