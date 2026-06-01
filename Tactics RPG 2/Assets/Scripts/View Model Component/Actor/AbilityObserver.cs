using UnityEngine;

public class AbilityObserver : MonoBehaviour
{
    // Check this in the Inspector on Rein's unit — leave unchecked on everyone else
    [SerializeField] bool isAnalyzeUser;

    AbilityObservationSystem observation;

    void Awake()
    {
        observation = GetComponent<AbilityObservationSystem>();
        this.AddObserver(OnAbilityUsed, "AbilityUsed");
    }

    void OnDestroy()
    {
        this.RemoveObserver(OnAbilityUsed, "AbilityUsed");
    }

    void OnAbilityUsed(object sender, object args)
    {
        // Only Rein records observations
        if (!isAnalyzeUser)
            return;

        Ability ability = args as Ability;
        if (ability == null || ability.data == null)
            return;

        // Don't count Rein watching himself
        Unit caster = (sender as Component)?.GetComponentInParent<Unit>();
        Unit rein = GetComponentInParent<Unit>();
        if (caster == rein)
            return;

        observation.Observe(ability.data);
    }
}
