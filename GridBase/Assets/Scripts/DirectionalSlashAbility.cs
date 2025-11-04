using UnityEngine;

[CreateAssetMenu(fileName = "DirectionalSlash", menuName = "Abilities/DirectionalSlash")]
public class DirectionalSlashAbility : Ability
{
    public int maxLength = 3; // how many tiles forward to affect

    void OnEnable()
    {
        abilityName = string.IsNullOrEmpty(abilityName) ? "Directional Slash" : abilityName;
        description = "Choose a direction (up/down/left/right) adjacent to the user. Hits up to 3 tiles: closest takes 3, next 2, next 1.";
        targetType = TargetType.Tile; // we use tile clicks to choose direction
        range = 1; // player must click adjacent tile to choose direction
        playActivationAnimation = true;
    }

    public override bool Activate(Chessman user, Chessman target)
    {
        // Not used for this ability; it uses ActivateOnTile
        return false;
    }

    public override bool ActivateOnTile(Chessman user, Vector2Int tile)
    {
        if (user == null) return false;
        GridManager gm = GridManager.Instance;
        if (gm == null) return false;

        Vector2Int origin = user.gridPosition;
        Vector2Int dir = tile - origin;

        // Accept any tile along an orthogonal line (same row or column), not just immediate neighbor.
        if ((dir.x == 0 && dir.y == 0) || (dir.x != 0 && dir.y != 0))
        {
            Debug.Log("DirectionalSlash: Invalid direction selected (must be in same row or column, not the origin).");
            return false;
        }

        // Normalize to unit direction
        dir.x = (dir.x == 0) ? 0 : (int)Mathf.Sign(dir.x);
        dir.y = (dir.y == 0) ? 0 : (int)Mathf.Sign(dir.y);

        // Apply damage falloff: distance 1 -> 3, 2 -> 2, 3 -> 1
        for (int i = 1; i <= maxLength; i++)
        {
            Vector2Int pos = origin + dir * i;
            if (!gm.IsValidTile(pos)) break;

            Chessman c = gm.GetCharacterAt(pos);
            if (c == null || !c.IsAlive()) continue;

            // Only damage enemies (not allies)
            if (c.isEnemy == user.isEnemy) continue;

            int damage = Mathf.Clamp(4 - i, 1, 3);
            c.TakeDamage(damage, user);
        }

        return true;
    }
}
