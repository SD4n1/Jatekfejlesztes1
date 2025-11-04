using UnityEngine;

[CreateAssetMenu(fileName = "TankSlamAbility", menuName = "Abilities/TankSlamAbility")]
public class TankSlamAbility : Ability
{
    public override bool Activate(Chessman user, Chessman target)
    {
        if (user == null) return false;

        GridManager grid = GridManager.Instance;
        if (grid == null) return false;

        Vector2Int center = user.gridPosition;
        int hits = 0;
        int damage = 3; 

        for (int dx = -range; dx <= range; dx++)
        {
            for (int dy = -range; dy <= range; dy++)
            {
                Vector2Int pos = new Vector2Int(center.x + dx, center.y + dy);
                if (!grid.IsValidTile(pos)) continue;

                Chessman c = grid.GetCharacterAt(pos);
                if (c == null) continue;
                if (!c.IsAlive()) continue;
                if (c.isEnemy == user.isEnemy) continue; // only hit enemies

                c.TakeDamage(damage, user);
                hits++;
            }
        }

        return hits > 0;
    }

    void OnEnable()
    {
        abilityName = string.IsNullOrEmpty(abilityName) ? "Slam" : abilityName;
        targetType = TargetType.Self; // self-centered ability: no external click needed
        range = 1; // default range 1
    }
}
