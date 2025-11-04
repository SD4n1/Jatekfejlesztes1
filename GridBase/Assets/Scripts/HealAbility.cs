using UnityEngine;

[CreateAssetMenu(fileName = "HealAbility", menuName = "Abilities/HealAbility")]
public class HealAbility : Ability
{
    private const int HealAmount = 3;

    public override bool Activate(Chessman user, Chessman target)
    {
        if (user == null || target == null) return false;

        // Only allies
        if (user.isEnemy != target.isEnemy) return false;

        // Only alive targets
        if (!target.IsAlive()) return false;

        // Distance check using grid positions
        int dx = Mathf.Abs(user.gridPosition.x - target.gridPosition.x);
        int dy = Mathf.Abs(user.gridPosition.y - target.gridPosition.y);
        int distance = Mathf.Max(dx, dy);

        if (distance > range) return false;

        // Heal
        target.Heal(HealAmount);
        return true;
    }

    void OnEnable()
    {
        // sensible defaults for this ability
        abilityName = string.IsNullOrEmpty(abilityName) ? "Heal" : abilityName;
        targetType = TargetType.Ally;
        range = Mathf.Max(1, range);
        // default desired range is 3; only set if not customized in inspector
        if (range == 1) range = 3;
    }
}
