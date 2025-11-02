using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [Header("Grid Settings")]
    public int gridWidth = 8;
    public int gridHeight = 6; 
    public float cellSize = 1.1f; // Ezt a r�gi Chessman.cs-b�l vetted (1.1f)
    public Vector3 gridOrigin = new Vector3(-3.833333f, -2.75f, 0);

    [Header("Prefabs")]
    public GameObject moveHighlightPrefab;
    public GameObject attackHighlightPrefab;

    // FIGYELEM: Character helyett Chessman-t t�rolunk!
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

    // Vil�g koordin�t�b�l r�cs koordin�ta
    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        // A te koordin�ta-rendszered alapj�n (a SetCoords-b�l)
        // (X - X_origin) / cella_m�ret
        // Haszn�lj RoundToInt-et a pontoss�g�rt
        int x = Mathf.RoundToInt((worldPosition.x - gridOrigin.x) / cellSize);
        int y = Mathf.RoundToInt((worldPosition.y - gridOrigin.y) / cellSize);
        return new Vector2Int(x, y);
    }

    // R�cs koordin�t�b�l vil�g koordin�ta
    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        // A te koordin�ta-rendszered alapj�n (a SetCoords-b�l)
        float x = (gridPosition.x * cellSize) + gridOrigin.x;
        float y = (gridPosition.y * cellSize) + gridOrigin.y;

        // JAV�T�S: A Z �rt�k 0 helyett -0.2, hogy a t�bla (Z=0) el�tt legyen
        return new Vector3(x, y, -0.2f);
    }

    // Karakter regisztr�l�sa a r�csra (j�t�k elej�n)
    public void RegisterCharacter(Vector2Int pos, Chessman character)
    {
        if (characterGrid.ContainsKey(pos))
        {
            Debug.LogWarning($"A(z) {pos} mez� m�r foglalt, de {character.name} megpr�b�lja elfoglalni.");
        }
        characterGrid[pos] = character;
    }

    // Karakter mozgat�sa a r�cson
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

    // Visszaadja, hogy egy mez� szabad-e
    public bool IsTileOccupied(Vector2Int pos)
    {
        return characterGrid.ContainsKey(pos) && characterGrid[pos] != null && characterGrid[pos].IsAlive();
    }

    // Visszaadja, hogy ki van a mez�n
    public Chessman GetCharacterAt(Vector2Int pos)
    {
        if (characterGrid.ContainsKey(pos))
        {
            return characterGrid[pos];
        }
        return null;
    }


    // Megkeresi az �sszes el�rhet� mez�t (Breadth-First Search)
    public HashSet<Vector2Int> FindReachableTiles(Vector2Int start, int range, bool canOccupy = false)
    {
        HashSet<Vector2Int> reachableTiles = new HashSet<Vector2Int>();
        Queue<(Vector2Int pos, int dist)> queue = new Queue<(Vector2Int, int)>();

        if (!IsValidTile(start))
        {
            Debug.LogWarning($"FindReachableTiles: A start poz�ci� ({start}) �rv�nytelen!");
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

                // Ha nem l�phet�nk foglalt mez�re, �s foglalt
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

    // --- Jelz�k (Highlights) kezel�se ---

    // show move tiles; currentPos, if provided, will tint that tile differently (e.g. yellow)
    public void ShowMoveTiles(HashSet<Vector2Int> tiles, Vector2Int? currentPos = null)
    {
        ClearMoveTiles();
        if (moveHighlightPrefab == null) return;

        foreach (var tile in tiles)
        {
            Vector3 worldPos = GridToWorld(tile);

            // JAVÍTÁS: A Z érték 1 helyett -0.1
            // így a tábla (0) előtt, de a bábu (-0.2) mögött lesz
            worldPos.z = -0.1f;

            var go = Instantiate(moveHighlightPrefab, worldPos, Quaternion.identity);
            // Set 50% transparency for move plates. If this tile is the current position,
            // tint it yellow and keep 50% alpha.
            var img = go.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                Color c = img.color;
                if (currentPos.HasValue && tile == currentPos.Value) c = Color.yellow;
                c.a = 0.5f;
                img.color = c;
            }
            else
            {
                var sr = go.GetComponentInChildren<SpriteRenderer>();
                if (sr != null)
                {
                    Color c = sr.color;
                    if (currentPos.HasValue && tile == currentPos.Value) c = Color.yellow;
                    c.a = 0.5f;
                    sr.color = c;
                }
            }
            activeHighlights.Add(go);
        }
    }

    public void ShowAttackTiles(HashSet<Vector2Int> tiles)
    {
        ClearAttackTiles();
        if (attackHighlightPrefab == null) return;

        foreach (var tile in tiles)
        {
            Vector3 worldPos = GridToWorld(tile);

            // JAV�T�S: A Z �rt�k 1 helyett -0.1
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