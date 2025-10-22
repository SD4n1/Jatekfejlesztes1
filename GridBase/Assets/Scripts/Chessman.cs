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
// Kasztok: tank, fighter, ranger, healer
public enum CastType
{
    None,
    Tank,
    Fighter,
    Ranger,
    Healer
}


// -------------------------------------------------------------------
// A MÓDOSÍTOTT CHESSMAN OSZTÁLY
// -------------------------------------------------------------------
public class Chessman : MonoBehaviour
{
    // EZ AZ ÚJ LEGERDÜLŐ MENÜ!
    [Header("Bábu Kasztja")]
    [Tooltip("Válaszd ki a bábu kasztját! Ez határozza meg a mozgását és a statjait.")]
    public CastType castType = CastType.None;

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
    public Sprite black_tank, black_fighter, black_ranger, black_healer;
    public Sprite white_tank, white_fighter, white_ranger, white_healer;

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
        switch (this.castType)
        {
            case CastType.Tank:
                this.GetComponent<SpriteRenderer>().sprite = isEnemy ? black_tank : white_tank;
                break;
            case CastType.Fighter:
                this.GetComponent<SpriteRenderer>().sprite = isEnemy ? black_fighter : white_fighter;
                break;
            case CastType.Ranger:
                this.GetComponent<SpriteRenderer>().sprite = isEnemy ? black_ranger : white_ranger;
                break;
            case CastType.Healer:
                this.GetComponent<SpriteRenderer>().sprite = isEnemy ? black_healer : white_healer;
                break;
            case CastType.None:
            default:
                // leave sprite as-is
                break;
        }

        // 2. Statok beállítása: Csak akkor állít be defaultot, ha az Inspectorban 0 van
        switch (this.castType)
        {
            case CastType.Tank:
                if (maxHealth <= 0) maxHealth = 15;
                if (attackPower <= 0) attackPower = 3;
                if (moveRange <= 0) moveRange = 3;
                if (attackRange <= 0) attackRange = 1;
                break;
            case CastType.Fighter:
                if (maxHealth <= 0) maxHealth = 8;
                if (attackPower <= 0) attackPower = 4;
                if (moveRange <= 0) moveRange = 4;
                if (attackRange <= 0) attackRange = 1;
                break;
            case CastType.Ranger:
                if (maxHealth <= 0) maxHealth = 6;
                if (attackPower <= 0) attackPower = 3;
                if (moveRange <= 0) moveRange = 4;
                if (attackRange <= 0) attackRange = 3;
                break;
            case CastType.Healer:
                if (maxHealth <= 0) maxHealth = 7;
                if (attackPower <= 0) attackPower = 1;
                if (moveRange <= 0) moveRange = 4;
                if (attackRange <= 0) attackRange = 1;
                break;

            case CastType.None:
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

        // JAVÍTVA: Most már a 'castType' (enum) alapján működik
    public HashSet<Vector2Int> GetValidMoveTiles()
    {
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();

        // A 'this.name' helyett az új 'castType'-ot használjuk
        switch (this.castType)
        {
            case CastType.Ranger:
                // Ranged: moves in lines (like queen) to represent range
                SurroundMove(tiles, null);
                break;
            case CastType.Fighter:
                // Fighter: agile melee - use knight-like L move
                LMove(tiles, null);
                break;
            case CastType.Tank:
                // Tank: slow melee - adjacent tiles
                SurroundMove(tiles, null);
                break;
            case CastType.Healer:
                // Healer: support unit - adjacent movement
                SurroundMove(tiles, null);
                break;
        }
        return tiles;
    }

    // JAVÍTVA: Most már a 'pieceType' (enum) alapján működik
    public HashSet<Vector2Int> GetValidAttackTiles()
    {
        HashSet<Vector2Int> tiles = new HashSet<Vector2Int>();

        switch (this.castType)
        {
            case CastType.Ranger:
                LineMove(null, tiles, 1, 0);
                LineMove(null, tiles, 0, 1);
                LineMove(null, tiles, 1, 1);
                LineMove(null, tiles, -1, 0);
                LineMove(null, tiles, 0, -1);
                LineMove(null, tiles, -1, -1);
                LineMove(null, tiles, -1, 1);
                LineMove(null, tiles, 1, -1);
                break;
            case CastType.Fighter:
                LMove(null, tiles);
                break;
            case CastType.Tank:
                SurroundMove(null, tiles);
                break;
            case CastType.Healer:
                SurroundMove(null, tiles);
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
}