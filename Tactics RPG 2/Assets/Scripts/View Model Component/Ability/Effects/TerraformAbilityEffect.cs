using UnityEngine;

public class TerraformAbilityEffect : BaseAbilityEffect
{
    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        if (target == null)
            return 0;
        TerraformAbilityArea area = GetComponentInParent<Ability>().GetComponent<TerraformAbilityArea>();
        Board board = GameObject.FindObjectOfType<Board>();
        if (area == null || board == null)
            return 0;
        int operation = area.GetOperation(target);
        if (operation == 0)
            return 0;
        board.SetTileHeight(target, target.height + (area.heightStep * operation));
        return 0;
    }
}
