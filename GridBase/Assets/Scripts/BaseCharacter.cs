using System.Collections.Generic;
using UnityEngine;
public abstract class BaseCharacter
{
    public string Name { get; protected set; }
    public int MaxHealth { get; protected set; }
    public int CurrentHealth { get; protected set; }
    public int AttackPower { get; protected set; }
    public int MoveRange { get; protected set; }
    public int AttackRange { get; protected set; }

    public Sprite Sprite { get; protected set; }

    public bool IsEnemy { get; protected set; }

    public BaseCharacter(string name, bool isEnemy)
    {
        Name = name;
        IsEnemy = isEnemy;
    }

    public virtual void InitializeStats() { }

    public virtual bool TakeDamage(int amount)
    {
        if (amount < 0)
            amount = 0;

        CurrentHealth -= amount;

        if (CurrentHealth < 0)
            CurrentHealth = 0;

        // Visszajelzés: meghalt-e
        return CurrentHealth <= 0;
    }

    public bool IsAlive() => CurrentHealth > 0;

    public abstract HashSet<Vector2Int> GetValidMoveTiles(Vector2Int gridPos, GridManager grid);
    public abstract HashSet<Vector2Int> GetValidAttackTiles(Vector2Int gridPos, GridManager grid);
}

