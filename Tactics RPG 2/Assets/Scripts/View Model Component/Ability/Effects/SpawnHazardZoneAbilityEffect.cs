using UnityEngine;

public class SpawnHazardZoneAbilityEffect : BaseAbilityEffect
{
    public HazardZoneType hazardType = HazardZoneType.Generic;
    public int durationRounds = 3;
    public float percentOfMaxHP = 0.1f;
    public int flatDamage = 0;
    public int minimumDamage = 1;
    public bool canKnockOut = true;
    public bool damageOnEnter = true;
    public bool damageOnTurnStart = true;
    public bool replaceExistingSameType = true;

    public string statusName = "";
    public int statusDuration = 1;
    public int statusChance = 100;
    public bool applyStatusOnEnter = false;
    public bool applyStatusOnTurnStart = false;

    public int darknessAccuracyPenalty = 20;
    public GameObject visualPrefab;
    public Vector3 colliderSize = new Vector3(1f, 2f, 1f);
    public Vector3 colliderCenter = new Vector3(0f, 1f, 0f);

    public override int Predict(Tile target)
    {
        return 0;
    }

    protected override int OnApply(Tile target)
    {
        if (target == null)
            return 0;

        if (replaceExistingSameType)
        {
            HazardZoneTileEffect[] existing = target.GetComponentsInChildren<HazardZoneTileEffect>();
            for (int i = existing.Length - 1; i >= 0; --i)
            {
                if (existing[i] != null && existing[i].hazardType == hazardType)
                    Destroy(existing[i].gameObject);
            }
        }

        GameObject obj = new GameObject(hazardType.ToString() + " Hazard Zone");
        obj.transform.SetParent(target.transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        BoxCollider collider = obj.AddComponent<BoxCollider>();
        collider.isTrigger = true;
        collider.size = colliderSize;
        collider.center = colliderCenter;

        HazardZoneTileEffect effect = obj.AddComponent<HazardZoneTileEffect>();
        effect.hazardType = hazardType;
        effect.durationRounds = durationRounds;
        effect.percentOfMaxHP = percentOfMaxHP;
        effect.flatDamage = flatDamage;
        effect.minimumDamage = minimumDamage;
        effect.canKnockOut = canKnockOut;
        effect.damageOnEnter = damageOnEnter;
        effect.damageOnTurnStart = damageOnTurnStart;
        effect.statusName = statusName;
        effect.statusDuration = statusDuration;
        effect.statusChance = statusChance;
        effect.applyStatusOnEnter = applyStatusOnEnter;
        effect.applyStatusOnTurnStart = applyStatusOnTurnStart;
        effect.darknessAccuracyPenalty = darknessAccuracyPenalty;

        if (visualPrefab != null)
        {
            GameObject visual = Instantiate(visualPrefab, target.center, target.transform.rotation);
            visual.transform.SetParent(obj.transform);
        }
        return 0;
    }
}
