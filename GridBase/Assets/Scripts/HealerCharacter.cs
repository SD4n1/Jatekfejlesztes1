using System.Collections.Generic;
using UnityEngine;

public class HealerCharacter : BaseCharacter
{
    public HealerCharacter(string name, bool isEnemy) : base(name, isEnemy)
    {
        InitializeStats();
    }

    public override void InitializeStats()
    {
        MaxHealth = 7;
        CurrentHealth = MaxHealth;
        AttackPower = 1;
        MoveRange = 4;
        AttackRange = 1;
    }

    public override HashSet<Vector2Int> GetValidMoveTiles(Vector2Int gridPos, GridManager grid)
    {
        return MovementPatterns.SurroundMove(gridPos, grid, IsEnemy, MoveRange);
    }

    public override HashSet<Vector2Int> GetValidAttackTiles(Vector2Int gridPos, GridManager grid)
    {
        return MovementPatterns.SurroundMove(gridPos, grid, IsEnemy, AttackRange);
    }
}
