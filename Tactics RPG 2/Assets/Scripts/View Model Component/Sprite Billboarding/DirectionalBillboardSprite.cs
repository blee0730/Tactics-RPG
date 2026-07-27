using UnityEngine;

/// <summary>
/// Camera-facing directional sprite for tactics units.
/// Attach this to the sprite visual child under a Unit's "Jumper" object.
/// The Unit root can still rotate for gameplay facing, while this child stays camera-facing.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class DirectionalBillboardSprite : MonoBehaviour
{
    [Header("References")]
    public Camera targetCamera;
    public Unit unit;
    public Transform billboardRoot;
    public SpriteRenderer spriteRenderer;
    public DirectionalSpriteSet sprites;

    [Header("Billboard")]
    [Tooltip("Keeps the sprite vertical instead of pitching it with the camera.")]
    public bool yawOnly = true;

    [Tooltip("Turn this on only if your sprite appears backwards/invisible from the camera.")]
    public bool invertBillboardForward = false;

    [Header("Directional Sprite Selection")]
    [Tooltip("Use front-diagonal and back-diagonal sprites when they exist.")]
    public bool useDiagonalSprites = true;

    [Tooltip("Assumes the side/diagonal sprites are drawn as the unit's right side, then flips them for left-side views.")]
    public bool mirrorLeftViews = true;

    [Header("Sprite Sheet Facing")]
    public bool sourceSideSpritesFaceLeft = true;

    private Sprite lastSprite;
    private bool lastFlipX;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (billboardRoot == null)
            billboardRoot = transform;

        if (unit == null)
            unit = GetComponentInParent<Unit>();

        if (targetCamera == null)
            targetCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (billboardRoot == null)
            billboardRoot = transform;

        // Important for your project:
        // Unit is added at runtime by UnitFactory, so it may not exist yet during Awake.
        if (unit == null)
            unit = GetComponentInParent<Unit>();

        if (targetCamera == null || spriteRenderer == null)
            return;

        UpdateBillboardRotation();
        UpdateDirectionalSprite();
    }

    private void UpdateBillboardRotation()
    {
        Vector3 forward = targetCamera.transform.forward;

        if (yawOnly)
            forward.y = 0f;

        if (forward.sqrMagnitude < 0.0001f)
            return;

        if (invertBillboardForward)
            forward = -forward;

        billboardRoot.rotation = Quaternion.LookRotation(forward.normalized, Vector3.up);
    }

    private void UpdateDirectionalSprite()
    {
        if (unit == null)
            unit = GetComponentInParent<Unit>();

        if (unit == null || sprites == null)
            return;

        Vector3 toCamera;

        if (targetCamera.orthographic)
            toCamera = -targetCamera.transform.forward;
        else
            toCamera = targetCamera.transform.position - unit.transform.position;

        toCamera.y = 0f;

        if (toCamera.sqrMagnitude < 0.0001f)
            return;

        float unitFacingAngle = DirectionToAngle(unit.dir);
        float cameraViewAngle = Mathf.Atan2(toCamera.x, toCamera.z) * Mathf.Rad2Deg;

        float relativeAngle = Mathf.DeltaAngle(unitFacingAngle, cameraViewAngle);
        float absAngle = Mathf.Abs(relativeAngle);

        Sprite selected = sprites.front;
        bool flipX = false;

        if (useDiagonalSprites && (sprites.frontRight != null || sprites.backRight != null))
        {
            if (absAngle <= 22.5f)
            {
                selected = sprites.front;
            }
            else if (absAngle <= 67.5f)
            {
                selected = sprites.GetFrontRightFallback();
                flipX = ShouldFlip(relativeAngle);
            }
            else if (absAngle <= 112.5f)
            {
                selected = sprites.right;
                flipX = ShouldFlip(relativeAngle);
            }
            else if (absAngle <= 157.5f)
            {
                selected = sprites.GetBackRightFallback();
                flipX = ShouldFlip(relativeAngle);
            }
            else
            {
                selected = sprites.back;
            }
        }
        else
        {
            if (absAngle <= 45f)
            {
                selected = sprites.front;
            }
            else if (absAngle <= 135f)
            {
                selected = sprites.right;
                flipX = ShouldFlip(relativeAngle);
            }
            else
            {
                selected = sprites.back;
            }
        }

        ApplySprite(selected, flipX);
    }

    private bool ShouldFlip(float relativeAngle)
    {
        if (!mirrorLeftViews)
            return false;

        // Positive relative angle = camera is viewing the unit's right side.
        // Negative relative angle = camera is viewing the unit's left side.
        if (sourceSideSpritesFaceLeft)
            return relativeAngle > 0f;

        return relativeAngle < 0f;
    }

    private void ApplySprite(Sprite sprite, bool flipX)
    {
        if (sprite == null)
            return;

        if (sprite != lastSprite)
        {
            spriteRenderer.sprite = sprite;
            lastSprite = sprite;
        }

        if (flipX != lastFlipX)
        {
            spriteRenderer.flipX = flipX;
            lastFlipX = flipX;
        }
    }

    private float DirectionToAngle(Directions direction)
    {
        switch (direction)
        {
            case Directions.North:
                return 0f;
            case Directions.East:
                return 90f;
            case Directions.South:
                return 180f;
            case Directions.West:
                return 270f;
            default:
                return 0f;
        }
    }
}
