using UnityEngine;

[CreateAssetMenu(menuName = "Tactics RPG/Sprites/Directional Sprite Set")]
public class DirectionalSpriteSet : ScriptableObject
{
    [Header("Required")]
    public Sprite front;
    public Sprite right;
    public Sprite back;

    [Header("Optional 8-way/5-angle views")]
    public Sprite frontRight;
    public Sprite backRight;

    public Sprite GetFrontRightFallback()
    {
        return frontRight != null ? frontRight : right;
    }

    public Sprite GetBackRightFallback()
    {
        return backRight != null ? backRight : right;
    }
}
