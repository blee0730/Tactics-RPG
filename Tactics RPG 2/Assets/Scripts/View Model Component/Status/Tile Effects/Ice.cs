using UnityEngine;

public class Ice : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Unit unit =
            other.GetComponent<Unit>();

        if (unit == null)
            return;

        Status status =
            unit.GetComponentInChildren<Status>();

        if (status == null)
            return;

        DurationStatusCondition condition =
            status.Add
            <
                StopStatusEffect,
                DurationStatusCondition
            >();

        condition.duration = 1;
    }
}