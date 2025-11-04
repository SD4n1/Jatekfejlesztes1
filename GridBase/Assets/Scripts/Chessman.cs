using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// -------------------------------------------------------------------
// ENUM: Kasztok
// -------------------------------------------------------------------
public enum CastType
{
    None,
    Tank,
    Fighter,
    Ranger,
    Healer
}

// ÚJ: A [RequireComponent] biztosítja, hogy ezen a GameObjecten mindig legyen
// egy AudioSource komponens, ami a hangokat lejátssza.
[RequireComponent(typeof(AudioSource))]
public class Chessman : MonoBehaviour
{
    // A logikai adatokat tároló objektum
    private BaseCharacter _characterData;
    public List<Ability> abilities = new List<Ability>();
    public string GetName()
    {
        return _characterData?.Name ?? "Ismeretlen";
    }
    public int GetAttackRange()
    {
        return _characterData?.AttackRange ?? 0;
    }
    public List<Ability> GetAbilities()
    {
        return abilities;
    }
    public int GetCurrentHealth()
    {
        return _characterData?.CurrentHealth ?? 0;
    }
    public int GetMaxHealth()
    {
        return _characterData?.MaxHealth ?? 0;
    }

    [Header("Bábu Kasztja")]
    [Tooltip("Válaszd ki a bábu kasztját! Ez határozza meg a mozgását és a statjait.")]
    public CastType castType = CastType.None;

    [Header("Stats & Visuals")]
    [Tooltip("Pipáld be, ha ez a bábu az ellenség (fekete)")]
    public bool isEnemy = false;

    // Spritelisták
    public Sprite black_tank, black_fighter, black_ranger, black_healer;
    public Sprite white_tank, white_fighter, white_ranger, white_healer;

    [Header("Visuals - UI")]
    public GameObject selectionFrame;
    public Slider healthSlider;

    // --- ÚJ SZEKCIÓ ---
    [Header("Audio")]
    [Tooltip("A támadáskor lejátszandó hang.")]
    public AudioClip attackSound; // ÚJ

    [Tooltip("A képesség használatakor lejátszandó hang.")]
    public AudioClip abilitySound; // ÚJ
    // --- ÚJ SZEKCIÓ VÉGE ---

    [Header("System")]
    private GridManager gridManager;
    private Vector3 baseWorldPosition;
    [HideInInspector]
    public Vector2Int gridPosition;
    public string player; // black vagy white


    private Animator animator;
    private AudioSource audioSource; // ÚJ: Referencia a hangforráshoz

    void Awake()
    {
        // 1. Létrehozzuk a logikai karakterobjektumot (BaseCharacter leszármazottat)
        InitializeCharacterData();

        // Animator referencia lekérése
        animator = GetComponentInChildren<Animator>();

        // ÚJ SOROK: AudioSource referencia lekérése
        // A [RequireComponent] miatt ennek már léteznie kell
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // Biztonsági beállítás, ne induljon el a játékkal
    }

    public void SetAnimationBool(string nev, bool ertek)
    {
        if (animator != null)
        {
            animator.SetBool(nev, ertek);
        }
    }

    public void SetAnimationTrigger(string nev)
    {
        if (animator != null)
        {
            animator.SetTrigger(nev);
        }
    }


    void Start()
    {
        if (_characterData == null)
        {
            Debug.LogError("A karakter adatobjektum nem inicializálódott! Ellenőrizd a CastType beállítást.");
            return;
        }

        gridManager = GridManager.Instance;
        if (gridManager == null)
        {
            Debug.LogError("Nincs GridManager a jelenetben!");
            return;
        }

        // 2. UI és pozíció beállítása a logikai adatok alapján
        if (healthSlider != null)
        {
            healthSlider.maxValue = _characterData.MaxHealth;
            healthSlider.value = _characterData.CurrentHealth;
        }

        // Pozíció beállítása a GridManager segítségével
        baseWorldPosition = transform.position;
        gridPosition = gridManager.WorldToGrid(baseWorldPosition);
        baseWorldPosition = gridManager.GridToWorld(gridPosition);
        transform.position = baseWorldPosition;

        // Regisztráció a GridManager-nél
        gridManager.RegisterCharacter(gridPosition, this);

        // Játékos-szín beállítása
        player = isEnemy ? "white" : "black";

        // Sprite beállítása
        SetSprite();

        SetSelected(false);
    }

    // Létrehozza a megfelelő BaseCharacter leszármazottat a CastType alapján
    private void InitializeCharacterData()
    {
        string charName = this.gameObject.name;

        // A BaseCharacter konstruktorban fut le az InitializeStats()
        switch (castType)
        {
            case CastType.Tank:
                _characterData = new TankCharacter(charName, isEnemy);
                break;
            case CastType.Fighter:
                _characterData = new FighterCharacter(charName, isEnemy);
                break;
            case CastType.Ranger:
                _characterData = new RangerCharacter(charName, isEnemy);
                break;
            case CastType.Healer:
                _characterData = new HealerCharacter(charName, isEnemy);
                break;
            case CastType.None:
            default:
                Debug.LogError("Nincs beállított CastType!");
                break;
        }
    }

    public void SetSprite()
    {
        Sprite targetSprite = null;
        switch (this.castType)
        {
            case CastType.Tank: targetSprite = isEnemy ? white_tank : black_tank; break;
            case CastType.Fighter: targetSprite = isEnemy ? white_fighter : black_fighter; break;
            case CastType.Ranger: targetSprite = isEnemy ? white_ranger : black_ranger; break;
            case CastType.Healer: targetSprite = isEnemy ? white_healer : black_healer; break;
            case CastType.None:
            default:
                break;
        }

        if (targetSprite != null)
        {
            this.GetComponent<SpriteRenderer>().sprite = targetSprite;
        }
    }


    public int GetHealth() { return _characterData?.CurrentHealth ?? 0; }
    public int GetDamage() { return _characterData?.AttackPower ?? 0; }
    // attacker may be null (e.g., environmental damage). If attacker is provided and this
    // has an active reflect shield, the incoming damage is prevented and the attacker
    // takes 1 damage instead.
    public bool TakeDamage(int amount, Chessman attacker = null)
    {
        if (_characterData == null) return false;

        // Reflect logic: if reflect is active and an attacker is present, block damage
        // and damage the attacker by 1 instead. Do not cause recursive reflection.
        if (attacker != null && reflectActive)
        {
            Debug.Log($"{_characterData.Name} blocked the attack and reflects 1 damage to {attacker.GetName()}");
            attacker.TakeDamage(1, null);
            return false;
        }

        bool died = _characterData.TakeDamage(amount);

        if (healthSlider != null)
        {
            healthSlider.value = _characterData.CurrentHealth;
        }

        Debug.Log($"{_characterData.Name} sebződött, új HP: {_characterData.CurrentHealth}");

        if (died)
        {
            Die();
        }
        return died;
    }
    public bool IsAlive() { return _characterData?.IsAlive() ?? false; }

    // Heal this chessman by amount (updates UI)
    public void Heal(int amount)
    {
        if (_characterData == null) return;
        _characterData.Heal(amount);
        if (healthSlider != null)
        {
            healthSlider.value = _characterData.CurrentHealth;
        }
        Debug.Log($"{_characterData.Name} healed by {amount}, current HP: {_characterData.CurrentHealth}");
    }


    public IEnumerator MoveToTile(Vector2Int targetTile)
    {
        if (gridManager == null) yield break;
        gridManager.MoveCharacter(gridPosition, targetTile, this);
        Vector3 targetWorldPos = gridManager.GridToWorld(targetTile);

        float moveTime = 0.4f;
        float elapsed = 0f;
        Vector3 startPos = baseWorldPosition;
        SetAnimationBool("IsWalking", true);

        while (elapsed < moveTime)
        {
            transform.position = Vector3.Lerp(startPos, targetWorldPos, elapsed / moveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        SetAnimationBool("IsWalking", false);

        transform.position = targetWorldPos;
        baseWorldPosition = targetWorldPos;
        gridPosition = targetTile;
    }

    public IEnumerator AttackAnimation(Chessman target)
    {
        // ... (A AttackAnimation marad, használva a GetDamage() metódust) ...
        Vector3 startPos = baseWorldPosition;
        Vector3 targetPos = target.transform.position;
        Vector3 midPoint = startPos + (targetPos - startPos) * 0.3f;
        float moveTime = 0.2f;
        float elapsed = 0f;

    SetAnimationTrigger("Attack");

        // ÚJ SOR: Támadás hang lejátszása
        PlaySound(attackSound);

        while (elapsed < moveTime)
        {
            transform.position = Vector3.Lerp(startPos, midPoint, elapsed / moveTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        target.TakeDamage(GetDamage(), this); // A GetDamage() lekéri a logikai adatot; pass attacker for reflect
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

    public IEnumerator AbilityAnimation(Chessman target, Ability abilityToUse)
    {

        SetAnimationTrigger("Ability");
        PlayAbilitySound();

        if (target != null && target != this)
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


            if (abilityToUse != null)
            {
                abilityToUse.Activate(this, target);
            }

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
        else
        {

            if (abilityToUse != null)
            {
                abilityToUse.Activate(this, target);
            }

            yield return new WaitForSeconds(0.3f);
        }
    }

    void Die()
    {
        Debug.Log($"{_characterData.Name} meghalt.");
        if (gridManager != null) { gridManager.UnregisterCharacter(gridPosition); }

        // Remove from turn order immediately so UI and logic don't try to act on dead unit
        if (TurnOrderManager.Instance != null)
        {
            TurnOrderManager.Instance.RemoveUnit(this);
        }

        gameObject.SetActive(false);
    }

    // Reflect/Shield state: when true, incoming attacks are blocked and attacker loses 1 HP.
    private bool reflectActive = false;

    public void ActivateReflect()
    {
        reflectActive = true;
        Debug.Log($"{GetName()} reflect activated.");
    }

    public void DeactivateReflect()
    {
        if (reflectActive)
        {
            reflectActive = false;
            Debug.Log($"{GetName()} reflect deactivated.");
        }
    }

    public void SetSelected(bool selected)
    {
        if (selectionFrame != null) { selectionFrame.SetActive(selected); }
    }

    public void SetHighlight(bool highlight) { /* Ide jöhet a glow effekt */ }

    public void PlaySound(AudioClip clipToPlay)
    {
        if (audioSource != null && clipToPlay != null)
        {
            audioSource.PlayOneShot(clipToPlay);
        }
    }

    public void PlayAbilitySound()
    {
        PlaySound(abilitySound);
    }

    public HashSet<Vector2Int> GetValidMoveTiles()
    {
        if (_characterData == null || gridManager == null) return new HashSet<Vector2Int>();
        var tiles = _characterData.GetValidMoveTiles(gridPosition, gridManager);
        if (tiles == null)
            tiles = new HashSet<Vector2Int>();
        tiles.Add(gridPosition);
        return tiles;
    }

    public HashSet<Vector2Int> GetValidAttackTiles()
    {
        if (_characterData == null || gridManager == null) return new HashSet<Vector2Int>();
        return _characterData.GetValidAttackTiles(gridPosition, gridManager);
    }
    public HashSet<Vector2Int> GetValidAttackTilesFrom(Vector2Int simulatedPos)
    {
        if (_characterData == null || gridManager == null) return new HashSet<Vector2Int>();
        // A _characterData logikáját hívjuk, de a "szimulált" pozícióval
        return _characterData.GetValidAttackTiles(simulatedPos, gridManager);
    }
}