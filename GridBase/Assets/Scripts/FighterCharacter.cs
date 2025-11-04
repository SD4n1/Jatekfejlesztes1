using System.Collections.Generic;
using UnityEngine;

public class FighterCharacter : BaseCharacter
{
    public FighterCharacter(string name, bool isEnemy) : base(name, isEnemy)
    {
        InitializeStats();
    }

    public override void InitializeStats()
    {
        MaxHealth = 9;
        CurrentHealth = MaxHealth;
        AttackPower = 4;
        MoveRange = 3;
        AttackRange = 1;
    }

    public override HashSet<Vector2Int> GetValidMoveTiles(Vector2Int gridPos, GridManager grid)
    {
        return MovementPatterns.SurroundMove(gridPos, grid, IsEnemy, MoveRange);
    }

    public override HashSet<Vector2Int> GetValidAttackTiles(Vector2Int gridPos, GridManager grid)
    {
        // includeOccupied = true so attacks can target occupied enemy tiles
        return MovementPatterns.SurroundMove(gridPos, grid, IsEnemy, AttackRange, true);
    }
}
