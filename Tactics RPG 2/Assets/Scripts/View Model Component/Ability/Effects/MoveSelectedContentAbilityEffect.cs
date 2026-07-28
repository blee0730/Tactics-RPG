using UnityEngine;

public class MoveSelectedContentAbilityEffect : BaseAbilityEffect
{
    public bool requireEmptyDestination = true;
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
        Tile source = area.tiles[0];
        Tile destination = area.tiles[1];
        if (source == null || destination == null || source.content == null)
            return 0;
        if (requireEmptyDestination && destination.content != null)
            return 0;

        GameObject content = source.content;
        Unit unit = content.GetComponent<Unit>();
        if (unit != null)
        {
            unit.Place(destination);
            unit.Match();
        }
        else
        {
            source.content = null;
            destination.content = content;
            content.transform.localPosition = destination.center;
        }
        lastFrame = Time.frameCount;
        return 0;
    }
}
