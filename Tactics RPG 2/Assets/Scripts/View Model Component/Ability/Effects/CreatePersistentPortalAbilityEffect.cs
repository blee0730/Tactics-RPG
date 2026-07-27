using UnityEngine;

public class CreatePersistentPortalAbilityEffect : BaseAbilityEffect
{
    public int durationRounds = 3;
    public bool requireEmptyTiles = true;
    public bool replaceExistingPortalsOnTiles = true;
    public GameObject visualPrefab;
    public Vector3 colliderSize = new Vector3(1f, 2f, 1f);
    public Vector3 colliderCenter = new Vector3(0f, 1f, 0f);

    int lastCreatedFrame = -1;

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        if (Time.frameCount == lastCreatedFrame)
            return 0;

        Ability ability = GetComponentInParent<Ability>();
        AbilityArea area = ability != null ? ability.GetComponent<AbilityArea>() : null;
        if (area == null || area.tiles == null || area.tiles.Count < 2)
            return 0;

        Tile a = area.tiles[0];
        Tile b = area.tiles[1];
        if (a == null || b == null || a == b)
            return 0;
        if (requireEmptyTiles && ((a.content != null) || (b.content != null)))
            return 0;

        PortalTileEffect portalA = CreatePortal(a);
        PortalTileEffect portalB = CreatePortal(b);
        if (portalA == null || portalB == null)
            return 0;
        portalA.linkedPortal = portalB;
        portalB.linkedPortal = portalA;
        lastCreatedFrame = Time.frameCount;
        return 0;
    }

    PortalTileEffect CreatePortal(Tile tile)
    {
        if (tile == null)
            return null;

        if (replaceExistingPortalsOnTiles)
        {
            PortalTileEffect[] existing = tile.GetComponentsInChildren<PortalTileEffect>();
            for (int i = existing.Length - 1; i >= 0; --i)
                if (existing[i] != null)
                    Destroy(existing[i].gameObject);
        }

        GameObject obj = new GameObject("Persistent Portal");
        obj.transform.SetParent(tile.transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        BoxCollider collider = obj.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = colliderSize;
        collider.center = colliderCenter;
        PortalTileEffect portal = obj.AddComponent<PortalTileEffect>();
        portal.durationRounds = durationRounds;
        if (visualPrefab != null)
        {
            GameObject visual = Instantiate(visualPrefab, tile.center, tile.transform.rotation);
            visual.transform.SetParent(obj.transform);
        }
        return portal;
    }
}
