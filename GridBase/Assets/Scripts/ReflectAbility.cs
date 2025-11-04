using UnityEngine;

[CreateAssetMenu(fileName = "ReflectAbility", menuName = "Abilities/ReflectAbility")]
public class ReflectAbility : Ability
{
    public override bool Activate(Chessman user, Chessman target)
    {
        if (user == null) return false;
        user.ActivateReflect();
        return true;
    }

    void OnEnable()
    {
        abilityName = string.IsNullOrEmpty(abilityName) ? "Reflect" : abilityName;
        targetType = TargetType.Self;
        range = 0;
    }
}
