using System.Collections.Generic;
using UnityEngine;

public static class MovementPatterns
{
    // includeOccupied: if true, will include tiles occupied by enemy units (used for attack targeting)
    public static HashSet<Vector2Int> SurroundMove(Vector2Int pos, GridManager grid, bool isEnemy, int range = 1, bool includeOccupied = false)
    {
        HashSet<Vector2Int> tiles = new();
        Vector2Int[] directions = {
        new(0,1), new(0,-1), new(1,0), new(-1,0),
        new(1,1), new(1,-1), new(-1,1), new(-1,-1)
    };

        foreach (var dir in directions)
        {
            for (int i = 1; i <= range; i++)
            {
                Vector2Int target = pos + dir * i;
                if (!grid.IsValidTile(target))
                    break;

                Chessman targetPiece = grid.GetCharacterAt(target);
                if (targetPiece == null)
                {
                    tiles.Add(target);
                }
                else
                {
                    // Ha includeOccupied true, és az ott lévő bábu ellenség, akkor
                    // vegyük fel támadási célként. Mozgásra (includeOccupied=false)
                    // soha ne addjuk vissza a foglalt mezőt.
                    if (includeOccupied && targetPiece.isEnemy != isEnemy)
                    {
                        tiles.Add(target);
                    }
                    // Megtörjük a továbbhaladást ezen az irányon, mert egy foglalt
                    // mező akadályozza a további elérést.
                    break;
                }
            }
        }

        return tiles;
    }
}
