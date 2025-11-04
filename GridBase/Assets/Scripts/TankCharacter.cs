using System.Collections.Generic;
using UnityEngine;

public class TankCharacter : BaseCharacter
{
    public TankCharacter(string name, bool isEnemy) : base(name, isEnemy)
    {
        InitializeStats();
    }

    public override void InitializeStats()
    {
        MaxHealth = 14;
        CurrentHealth = MaxHealth;
        AttackPower = 5;
        MoveRange = 2;
        AttackRange = 2;
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
