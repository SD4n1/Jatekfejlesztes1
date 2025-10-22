using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// using TMPro;
// using UnityEngine.UI;

// -------------------------------------------------------------------
// AZ ÚJ "CUCC" (ENUM)
// Ezt a Chessman osztály FÖLÉ, de a fájlba helyezzük.
// -------------------------------------------------------------------
public enum PieceType
{
    None,
    Pawn,   // Gyalog
    Rook,   // Bástya
    Knight, // Huszár
    Bishop, // Futó
    Queen,  // Királynő
    King    // Király
}


// -------------------------------------------------------------------
// A MÓDOSÍTOTT CHESSMAN OSZTÁLY
// -------------------------------------------------------------------
public class Chessman : MonoBehaviour
{
    // EZ AZ ÚJ LEGERDÜLŐ MENÜ!
    [Header("Bábu Típusa")]
    [Tooltip("Válaszd ki a bábu típusát! Ez határozza meg a lépését és a statjait.")]
    public PieceType pieceType = PieceType.None;

    [Header("Stats")]
    public string characterName;
    public int maxHealth = 10;
    public int currentHealth;
    public int attackPower = 2;
    [Tooltip("Pipáld be, ha ez a bábu az ellenség (fekete)")]
    public bool isEnemy = false;

    [Header("Grid & Movement")]
    public int moveRange = 4;
    public int attackRange = 1;
    [HideInInspector]
    public Vector2Int gridPosition;

    [Header("Abilities")]
    public List<Ability> abilities;

    [Header("Visuals - Sprites")]
    public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;
    public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;

    [Header("Visuals - UI (Opcionális)")]
    public GameObject selectionFrame;
    public Slider healthSlider;

    [Header("System")]
    private GridManager gridManager;
    private Vector3 baseWorldPosition;
    public string player;

    void Start()
    {
        currentHealth = maxHealth;

        SetStatsAndSprite();

        gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            Debug.LogError("Nincs GridManager a jelenetben!");
            return;
        }
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth; // Kezdetben max életerő
        }

        baseWorldPosition = transform.position;
        gridPosition = gridManager.WorldToGrid(baseWorldPosition);
        baseWorldPosition = gridManager.GridToWorld(gridPosition);
        transform.position = baseWorldPosition;

        gridManager.RegisterCharacter(gridPosition, this);

        // Az 'isEnemy' alapján beállítjuk a játékos-színt
        if (isEnemy)
        {
            player = "black";
        }
        else
        {
            player = "white";
        }

        // Kinézet és statok beállítása a TÍPUS alapján
        SetStatsAndSprite();

        SetSelected(false);
    }

    // JAVÍTVA: Most már a 'pieceType' (enum) alapján működik, nem a név alapján
    // JAVÍTVA: Tiszteletben tartja az Inspectorban beállított értékeket
    public void SetStatsAndSprite()
    {
        // Alapértelmezett statok (csak akkor használjuk, ha az Inspectorban 0 van)
        int defaultHealth = 5;
        int defaultDamage = 1;
        int defaultMove = 4;
        int defaultAttack = 1;

        // 1. Sprite beállítása a TÍPUS és SZÍN alapján
        switch (this.pieceType)
        {
            case PieceType.Queen:
                this.GetComponent<SpriteRenderer>().sprite = isEnemy ? black_queen : white_queen;
                break;
            case PieceType.Knight:
                this.GetComponent<SpriteRenderer>().sprite = isEnemy ? black_knight : white_knight;
                break;
            case PieceType.Bishop:
                this.GetComponent<SpriteRenderer>().sprite = isEnemy ? black_bishop : white_bishop;
                break;
            case PieceType.King:
                this.GetComponent<SpriteRenderer>().sprite = isEnemy ? black_king : white_king;
                break;
            case PieceType.Rook:
                this.GetComponent<SpriteRenderer>().sprite = isEnemy ? black_rook : white_rook;
                break;
            case PieceType.Pawn:
                this.GetComponent<SpriteRenderer>().sprite = isEnemy ? black_pawn : white_pawn;
                break;
            case PieceType.None:
                // Üresen hagyjuk, vagy adjunk neki egy alap sprite-ot?
                break;
        }

        // 2. Statok beállítása: Csak akkor állít be defaultot, ha az Inspectorban 0 van
        switch (this.pieceType)
        {
            case PieceType.Queen:
                if (maxHealth <= 0) maxHealth = 8;
                if (attackPower <= 0) attackPower = 4;
                if (moveRange <= 0) moveRange = 6;
                if (attackRange <= 0) attackRange = 1;
                break;
            case PieceType.Knight:
                if (maxHealth <= 0) maxHealth = 5;
                if (attackPower <= 0) attackPower = 3;
                if (moveRange <= 0) moveRange = 3; // Lépés-szabály miatt ez csak a keresés határa
                if (attackRange <= 0) attackRange = 1;
                break;
            case PieceType.Bishop:
                if (maxHealth <= 0) maxHealth = 5;
                if (attackPower <= 0) attackPower = 2;
                if (moveRange <= 0) moveRange = 5;
                if (attackRange <= 0) attackRange = 1;
                break;
            case PieceType.King:
                if (maxHealth <= 0) maxHealth = 12;
                if (attackPower <= 0) attackPower = 3;
                if (moveRange <= 0) moveRange = 1; // Sakkban a király csak 1-et lép
                if (attackRange <= 0) attackRange = 1;
                break;
            case PieceType.Rook:
                if (maxHealth <= 0) maxHealth = 7;
                if (attackPower <= 0) attackPower = 3;
                if (moveRange <= 0) moveRange = 5;
                if (attackRange <= 0) attackRange = 1;
                break;
            case PieceType.Pawn:
                if (maxHealth <= 0) maxHealth = 3;
                if (attackPower <= 0) attackPower = 1;
                if (moveRange <= 0) moveRange = 1; // Alapból 1-et lép (a 2-es kezdőlépést külön kell kezelni)
                if (attackRange <= 0) attackRange = 1;
                break;

            case PieceType.None:
            default:
                if (maxHealth <= 0) maxHealth = defaultHealth;
                if (attackPower <= 0) attackPower = defaultDamage;
                if (moveRange <= 0) moveRange = defaultMove;
                if (attackRange <= 0) attackRange = defaultAttack;
                break;
        }

        // 3. Véglegesítés
        currentHealth = maxHealth;
        if (string.IsNullOrEmpty(characterName))
        {
            // Ha nincs név beírva, használja a GameObject nevét
            characterName = this.gameObject.name;
        }
        // Biztosítjuk, hogy az attackRange legalább 1 legyen (különben nem tud támadni)
        if (attackRange <= 0) attackRange = 1;
    }


    // === SEBZŐDÉS ÉS ÉLET (VÁLTOZATLAN) ===
    public int GetHealth() { return currentHealth; }
    public int GetDamage() { return attackPower; }
    public bool TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log($"{name} sebződött, új HP: {currentHealth}");

        // === SLIDER FRISSÍTÉSE ===
        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }
        // === VÉGE: SLIDER FRISSÍTÉSE ===

        // StartCoroutine(DamageAnimation(amount));

        if (currentHealth <= 0)
        {
            Die();
        }
        return (currentHealth <= 0);
    }
    public bool IsAlive() { return currentHealth > 0; }
    // === VÉGE: SEBZŐDÉS ÉS ÉLET ===


    // === MOZGÁS ÉS AKCIÓK (VÁLTOZATLAN) ===
    public IEnumerator MoveToTile(Vector2Int targetTile)
    {
        if (gridManager == null) yield break;
        gridManager.MoveCharacter(gridPosition, targetTile, this);
        Vector3 targetWorldPos = gridManager.GridToWorld(targetTile);
        float moveTime = 0.4f;
        float elapsed = 0f;
        Vector3 startPos = baseWorldPosition;
        while (elapsed < moveTime)
        {
            transform.position = Vector3.Lerp(startPos, targetWorldPos, elapsed / moveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = targetWorldPos;
        baseWorldPosition = targetWorldPos;
        gridPosition = targetTile;
    }

    public IEnumerator AttackAnimation(Chessman target)
    {
        Vector3 startPos = baseWorldPosition;
        Vector3 targetPos = target.transform.position;
        Vector3 midPoint = startPos + (targetPos - startPos) * 0.3f;
        float moveTime = 0.2f;
        float elapsed = 0f;
        while (elapsed < moveTime)
        {
            transform.position = Vector3.Lerp(startPos, midPoint, elapsed / moveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        target.TakeDamage(attackPower);
        yield return new WaitForSeconds(0.1f);
        elapsed = 0f;
        while (elapsed < moveTime)
        {
            transform.position = Vector3.Lerp(midPoint, startPos, elapsed / moveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = baseWorldPosition;
    }

    void Die()
    {
        Debug.Log($"{name} meghalt.");
        if (gridManager != null) { gridManager.UnregisterCharacter(gridPosition); }
        gameObject.SetActive(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectionFrame != null) { selectionFrame.SetActive(selected); }
    }

    public void SetHighlight(bool highlight) { /* Ide jöhet a glow effekt */ }
    // === VÉGE: MOZGÁS ÉS AKCIÓK ===


    // -------------------------------------------------------------------
    // SAKK LÉPÉS LOGIKA (MÓDOSÍTVA)
    // -------------------------------------------------------------------

    private Chessman GetPieceAt(Vector2Int pos)
    {
        if (gridManager == null) gridManager = GridManager.Instance;
        return gridManager.GetCharacterAt(pos);
    }

    // JAVÍTVA: Most már a 'pieceType' (enum) alapján működik
    public HashSet<Vector2Int> GetValidMoveTiles()
    {
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();

        // A 'this.name' helyett az új 'pieceType'-ot használjuk
        switch (this.pieceType)
        {
            case PieceType.Queen:
                LineMove(tiles, null, 1, 0);
                LineMove(tiles, null, 0, 1);
                LineMove(tiles, null, 1, 1);
                LineMove(tiles, null, -1, 0);
                LineMove(tiles, null, 0, -1);
                LineMove(tiles, null, -1, -1);
                LineMove(tiles, null, -1, 1);
                LineMove(tiles, null, 1, -1);
                break;
            case PieceType.Knight:
                LMove(tiles, null);
                break;
            case PieceType.Bishop:
                LineMove(tiles, null, 1, 1);
                LineMove(tiles, null, 1, -1);
                LineMove(tiles, null, -1, 1);
                LineMove(tiles, null, -1, -1);
                break;
            case PieceType.King:
                SurroundMove(tiles, null);
                break;
            case PieceType.Rook:
                LineMove(tiles, null, 1, 0);
                LineMove(tiles, null, 0, 1);
                LineMove(tiles, null, -1, 0);
                LineMove(tiles, null, 0, -1);
                break;
            case PieceType.Pawn:
                // Az 'isEnemy' (fekete) a -X, a 'fehér' a +X irányba mozog
                // (Feltéve, hogy a fekete a 7-es, a fehér a 0-s oszlopon indul)
                PawnMove(tiles, null, gridPosition.x + (isEnemy ? -1 : 1), gridPosition.y);
                break;
        }
        return tiles;
    }

    // JAVÍTVA: Most már a 'pieceType' (enum) alapján működik
    public HashSet<Vector2Int> GetValidAttackTiles()
    {
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();

        switch (this.pieceType)
        {
            case PieceType.Queen:
                LineMove(null, tiles, 1, 0);
                LineMove(null, tiles, 0, 1);
                LineMove(null, tiles, 1, 1);
                LineMove(null, tiles, -1, 0);
                LineMove(null, tiles, 0, -1);
                LineMove(null, tiles, -1, -1);
                LineMove(null, tiles, -1, 1);
                LineMove(null, tiles, 1, -1);
                break;
            case PieceType.Knight:
                LMove(null, tiles);
                break;
            case PieceType.Bishop:
                LineMove(null, tiles, 1, 1);
                LineMove(null, tiles, 1, -1);
                LineMove(null, tiles, -1, 1);
                LineMove(null, tiles, -1, -1);
                break;
            case PieceType.King:
                SurroundMove(null, tiles);
                break;
            case PieceType.Rook:
                LineMove(null, tiles, 1, 0);
                LineMove(null, tiles, 0, 1);
                LineMove(null, tiles, -1, 0);
                LineMove(null, tiles, 0, -1);
                break;
            case PieceType.Pawn:
                PawnMove(null, tiles, gridPosition.x + (isEnemy ? -1 : 1), gridPosition.y);
                break;
        }
        return tiles;
    }

    // --- LOGIKAI SEGÉDFÜGGVÉNYEK (VÁLTOZATLAN) ---

    public void LineMove(HashSet<Vector2Int> moveTiles, HashSet<Vector2Int> attackTiles, int xIncrement, int yIncrement)
    {
        if (gridManager == null) gridManager = GridManager.Instance;
        Vector2Int nextPos = gridPosition;
        nextPos.x += xIncrement;
        nextPos.y += yIncrement;
        while (gridManager.IsValidTile(nextPos) && gridManager.GetCharacterAt(nextPos) == null)
        {
            moveTiles?.Add(nextPos);
            nextPos.x += xIncrement;
            nextPos.y += yIncrement;
        }
        if (gridManager.IsValidTile(nextPos))
        {
            Chessman piece = GetPieceAt(nextPos);
            if (piece != null && piece.isEnemy != this.isEnemy)
            {
                attackTiles?.Add(nextPos);
            }
        }
    }

    public void LMove(HashSet<Vector2Int> moveTiles, HashSet<Vector2Int> attackTiles)
    {
        PointMove(moveTiles, attackTiles, gridPosition.x + 1, gridPosition.y + 2);
        PointMove(moveTiles, attackTiles, gridPosition.x - 1, gridPosition.y + 2);
        PointMove(moveTiles, attackTiles, gridPosition.x + 2, gridPosition.y + 1);
        PointMove(moveTiles, attackTiles, gridPosition.x + 2, gridPosition.y - 1);
        PointMove(moveTiles, attackTiles, gridPosition.x + 1, gridPosition.y - 2);
        PointMove(moveTiles, attackTiles, gridPosition.x - 1, gridPosition.y - 2);
        PointMove(moveTiles, attackTiles, gridPosition.x - 2, gridPosition.y + 1);
        PointMove(moveTiles, attackTiles, gridPosition.x - 2, gridPosition.y - 1);
    }

    public void SurroundMove(HashSet<Vector2Int> moveTiles, HashSet<Vector2Int> attackTiles)
    {
        PointMove(moveTiles, attackTiles, gridPosition.x, gridPosition.y + 1);
        PointMove(moveTiles, attackTiles, gridPosition.x, gridPosition.y - 1);
        PointMove(moveTiles, attackTiles, gridPosition.x - 1, gridPosition.y + 0);
        PointMove(moveTiles, attackTiles, gridPosition.x - 1, gridPosition.y - 1);
        PointMove(moveTiles, attackTiles, gridPosition.x - 1, gridPosition.y + 1);
        PointMove(moveTiles, attackTiles, gridPosition.x + 1, gridPosition.y + 0);
        PointMove(moveTiles, attackTiles, gridPosition.x + 1, gridPosition.y - 1);
        PointMove(moveTiles, attackTiles, gridPosition.x + 1, gridPosition.y + 1);
    }

    public void PointMove(HashSet<Vector2Int> moveTiles, HashSet<Vector2Int> attackTiles, int x, int y)
    {
        if (gridManager == null) gridManager = GridManager.Instance;
        Vector2Int pos = new Vector2Int(x, y);
        if (gridManager.IsValidTile(pos))
        {
            Chessman piece = GetPieceAt(pos);
            if (piece == null)
            {
                moveTiles?.Add(pos);
            }
            else if (piece.isEnemy != this.isEnemy)
            {
                attackTiles?.Add(pos);
            }
        }
    }

    public void PawnMove(HashSet<Vector2Int> moveTiles, HashSet<Vector2Int> attackTiles, int xForward, int y)
    {
        if (gridManager == null) gridManager = GridManager.Instance;
        Vector2Int pos = new Vector2Int(xForward, y);

        if (gridManager.IsValidTile(pos) && gridManager.GetCharacterAt(pos) == null)
        {
            moveTiles?.Add(pos);
        }

        Vector2Int attackPos1 = new Vector2Int(xForward, y + 1);
        Vector2Int attackPos2 = new Vector2Int(xForward, y - 1);
        if (gridManager.IsValidTile(attackPos1))
        {
            Chessman piece = GetPieceAt(attackPos1);
            if (piece != null && piece.isEnemy != this.isEnemy)
            {
                attackTiles?.Add(attackPos1);
            }
        }
        if (gridManager.IsValidTile(attackPos2))
        {
            Chessman piece = GetPieceAt(attackPos2);
            if (piece != null && piece.isEnemy != this.isEnemy)
            {
                attackTiles?.Add(attackPos2);
            }
        }
    }
}