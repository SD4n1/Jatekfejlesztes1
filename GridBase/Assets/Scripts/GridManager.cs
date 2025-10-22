using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public int gridWidth = 8; // Sakk méret
    public int gridHeight = 8; // Sakk méret
    public float cellSize = 1.1f; // Ezt a régi Chessman.cs-bõl vetted (1.1f)
    public Vector3 gridOrigin = new Vector3(-3.833333f, -3.833333f, 0); // Ezt is!

    [Header("Prefabs")]
    public GameObject moveHighlightPrefab;
    public GameObject attackHighlightPrefab;

    // FIGYELEM: Character helyett Chessman-t tárolunk!
    private Dictionary<Vector2Int, Chessman> characterGrid = new Dictionary<Vector2Int, Chessman>();

    private List<GameObject> activeHighlights = new List<GameObject>();
    private List<GameObject> activeAttackHighlights = new List<GameObject>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    // Világ koordinátából rács koordináta
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        // A te koordináta-rendszered alapján (a SetCoords-ból)
        // (X - X_origin) / cella_méret
        // Használj RoundToInt-et a pontosságért
        int x = Mathf.RoundToInt((worldPosition.x - gridOrigin.x) / cellSize);
        int y = Mathf.RoundToInt((worldPosition.y - gridOrigin.y) / cellSize);
        return new Vector2Int(x, y);
    }

    // Rács koordinátából világ koordináta
    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        // A te koordináta-rendszered alapján (a SetCoords-ból)
        float x = (gridPosition.x * cellSize) + gridOrigin.x;
        float y = (gridPosition.y * cellSize) + gridOrigin.y;

        // JAVÍTÁS: A Z érték 0 helyett -0.2, hogy a tábla (Z=0) elõtt legyen
        return new Vector3(x, y, -0.2f);
    }

    // Karakter regisztrálása a rácsra (játék elején)
    public void RegisterCharacter(Vector2Int pos, Chessman character)
    {
        if (characterGrid.ContainsKey(pos))
        {
            Debug.LogWarning($"A(z) {pos} mezõ már foglalt, de {character.name} megpróbálja elfoglalni.");
        }
        characterGrid[pos] = character;
    }

    // Karakter mozgatása a rácson
    public void MoveCharacter(Vector2Int from, Vector2Int to, Chessman character)
    {
        if (characterGrid.ContainsKey(from))
        {
            characterGrid.Remove(from);
        }
        characterGrid[to] = character;
    }

    public void UnregisterCharacter(Vector2Int pos)
    {
        if (characterGrid.ContainsKey(pos))
        {
            characterGrid.Remove(pos);
        }
    }

    // Visszaadja, hogy egy mezõ szabad-e
    public bool IsTileOccupied(Vector2Int pos)
    {
        return characterGrid.ContainsKey(pos) && characterGrid[pos] != null && characterGrid[pos].IsAlive();
    }

    // Visszaadja, hogy ki van a mezõn
    public Chessman GetCharacterAt(Vector2Int pos)
    {
        if (characterGrid.ContainsKey(pos))
        {
            return characterGrid[pos];
        }
        return null;
    }


    // Megkeresi az összes elérhetõ mezõt (Breadth-First Search)
    public HashSet<Vector2Int> FindReachableTiles(Vector2Int start, int range, bool canOccupy = false)
    {
        HashSet<Vector2Int> reachableTiles = new HashSet<Vector2Int>();
        Queue<(Vector2Int pos, int dist)> queue = new Queue<(Vector2Int, int)>();

        if (!IsValidTile(start))
        {
            Debug.LogWarning($"FindReachableTiles: A start pozíció ({start}) érvénytelen!");
            return reachableTiles;
        }

        queue.Enqueue((start, 0));
        reachableTiles.Add(start);

        Vector2Int[] directions = {
            new Vector2Int(0, 1),  // Fel
            new Vector2Int(0, -1), // Le
            new Vector2Int(1, 0),  // Jobbra
            new Vector2Int(-1, 0)  // Balra
        };

        while (queue.Count > 0)
        {
            var (currentPos, currentDist) = queue.Dequeue();

            if (currentDist >= range)
                continue;

            foreach (var dir in directions)
            {
                Vector2Int nextPos = currentPos + dir;

                if (!IsValidTile(nextPos)) continue;
                if (reachableTiles.Contains(nextPos)) continue;

                // Ha nem léphetünk foglalt mezõre, és foglalt
                if (!canOccupy && IsTileOccupied(nextPos)) continue;

                reachableTiles.Add(nextPos);
                queue.Enqueue((nextPos, currentDist + 1));
            }
        }
        return reachableTiles;
    }

    public bool IsValidTile(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < gridWidth && pos.y >= 0 && pos.y < gridHeight;
    }

    // --- Jelzõk (Highlights) kezelése ---

    public void ShowMoveTiles(HashSet<Vector2Int> tiles)
    {
        ClearMoveTiles();
        if (moveHighlightPrefab == null) return;

        foreach (var tile in tiles)
        {
            Vector3 worldPos = GridToWorld(tile);

            // JAVÍTÁS: A Z érték 1 helyett -0.1
            // Így a tábla (0) elõtt, de a bábu (-0.2) mögött lesz
            worldPos.z = -0.1f;

            activeHighlights.Add(Instantiate(moveHighlightPrefab, worldPos, Quaternion.identity));
        }
    }

    public void ShowAttackTiles(HashSet<Vector2Int> tiles)
    {
        ClearAttackTiles();
        if (attackHighlightPrefab == null) return;

        foreach (var tile in tiles)
        {
            Vector3 worldPos = GridToWorld(tile);

            // JAVÍTÁS: A Z érték 1 helyett -0.1
            worldPos.z = -0.1f;

            activeAttackHighlights.Add(Instantiate(attackHighlightPrefab, worldPos, Quaternion.identity));
        }
    }

    public void ClearMoveTiles()
    {
        foreach (var highlight in activeHighlights)
        {
            Destroy(highlight);
        }
        activeHighlights.Clear();
    }

    public void ClearAttackTiles()
    {
        foreach (var highlight in activeAttackHighlights)
        {
            Destroy(highlight);
        }
        activeAttackHighlights.Clear();
    }
}