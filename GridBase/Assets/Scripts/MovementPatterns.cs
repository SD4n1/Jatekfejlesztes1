using System.Collections.Generic;
using UnityEngine;

public static class MovementPatterns
{
    public static HashSet<Vector2Int> SurroundMove(Vector2Int pos, GridManager grid, bool isEnemy, int range = 1)
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
                    if (targetPiece.isEnemy != isEnemy)
                        tiles.Add(target);
                    break;
                }
            }
        }

        return tiles;
    }

    public static HashSet<Vector2Int> GetTilesInArea(Vector2Int center, GridManager grid, int size)
    {
        HashSet<Vector2Int> tiles = new();
        int radius = (size - 1) / 2; // size=3 -> radius=1

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                Vector2Int target = center + new Vector2Int(x, y);
                if (grid.IsValidTile(target))
                {
                    tiles.Add(target);
                }
            }
        }
        return tiles;
    }
}
