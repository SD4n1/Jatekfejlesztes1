using System.Collections.Generic;
using UnityEngine;

public class RangerCharacter : BaseCharacter
{
    public RangerCharacter(string name, bool isEnemy) : base(name, isEnemy)
    {
        InitializeStats();
    }

    public override void InitializeStats()
    {
        MaxHealth = 6;
        CurrentHealth = MaxHealth;
        AttackPower = 3;
        MoveRange = 4;
        AttackRange = 3;
    }

    public override HashSet<Vector2Int> GetValidMoveTiles(Vector2Int gridPos, GridManager grid)
    {
        return MovementPatterns.SurroundMove(gridPos, grid, IsEnemy, MoveRange);
    }

    public override HashSet<Vector2Int> GetValidAttackTiles(Vector2Int gridPos, GridManager grid)
    {
        return MovementPatterns.SurroundMove(gridPos, grid, IsEnemy, AttackRange, true);
    }
}
