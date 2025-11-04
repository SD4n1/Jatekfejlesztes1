using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.Linq;

public enum AIActionType { Attack, Move, Wait }

public class AIAction
{
    public float Score;
    public AIActionType Type;

    public Chessman TargetToAttack;
    public Vector2Int TileToMoveTo;

    public AIAction(AIActionType type, float score)
    {
        Type = type;
        Score = score;
        TileToMoveTo = Vector2Int.zero;
        TargetToAttack = null;
    }

    public AIAction(float score, Vector2Int position)
    {
        Type = AIActionType.Move;
        Score = score;
        TileToMoveTo = position;
        TargetToAttack = null;
    }

    public AIAction(float score, Vector2Int position, Chessman target)
    {
        Type = AIActionType.Attack;
        Score = score;
        TileToMoveTo = position;
        TargetToAttack = target;
    }
}
public class CombatManager : MonoBehaviour
{
    #region Variables

    [Header("Teams")]
    public List<Chessman> playerTeam = new List<Chessman>();
    public List<Chessman> enemyTeam = new List<Chessman>();

    [Header("System References")]
    public GridManager gridManager;

    [Header("Turn Order")]
    public TurnOrderManager turnOrderManager;

    [Header("UI References")]
    public TextMeshProUGUI turnText;
    public TextMeshProUGUI messageText;
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    [Header("Action UI")]
    public GameObject actionPanel;
    public Button attackButton;
    public Button abilityButton1;
    public Button waitButton;

    [Header("Audio")]
    public AudioSource musicSource;
    public AudioClip combatMusic;
    public AudioClip attackSound;
    public AudioClip hitSound;

    // Private state variables
    private Chessman selectedAttacker;
    private Chessman selectedTarget;
    private bool isPlayerTurn = true;
    private bool isProcessing = false;
    private CombatState currentState;

    private HashSet<Vector2Int> reachableTiles;
    private HashSet<Vector2Int> attackRangeTiles;
    private bool characterHasActed;
    private Ability selectedAbility;

    private HashSet<Chessman> charactersWhoFinishedTurn = new HashSet<Chessman>();

    private enum CombatState
    {
        SelectingCharacter,
        CharacterSelected,
        SelectingMoveTile,
        MovingCharacter,
        SelectingTarget,
        Processing,
        GameOver
    }

    #endregion

    //--------------------------------------------------------------------------
    // Indítás és kör menedzsment
    //--------------------------------------------------------------------------

    void Start()
    {
        if (actionPanel != null) actionPanel.SetActive(false);
        if (attackButton != null) attackButton.onClick.AddListener(OnPrepareAttack);
        if (waitButton != null) waitButton.onClick.AddListener(OnWait);

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        if (gridManager == null)
        {
            gridManager = GridManager.Instance;
            if (gridManager == null) Debug.LogError("Nincs GridManager a jelenetben!");
        }

        playerTeam.Clear();
        enemyTeam.Clear();

        StartCoroutine(SetupTeams());

        if (musicSource != null && combatMusic != null)
        {
            musicSource.clip = combatMusic;
            musicSource.loop = true;
            musicSource.Play();
        }

        if (turnOrderManager == null)
        {
            turnOrderManager = TurnOrderManager.Instance;
        }

    }

    IEnumerator SetupTeams()
    {
        yield return null; // Várunk egy frame-et, hogy minden Chessman.Start() lefusson

        // === EZ A SOR VÁLTOZOTT ===
        // A régi "FindObjectsOfType<Chessman>()" helyett az újat használjuk
        Chessman[] allPieces = FindObjectsByType<Chessman>(FindObjectsSortMode.None);
        // === EDDIG TART A VÁLTOZÁS ===

        foreach (Chessman piece in allPieces)
        {
            if (piece == null) continue;

            if (piece.isEnemy)
            {
                enemyTeam.Add(piece);
            }
            else
            {
                playerTeam.Add(piece);
            }
        }
        Debug.Log($"Csapatok betöltve: {playerTeam.Count} játékos, {enemyTeam.Count} ellenség.");

        if (turnOrderManager != null)
        {
            turnOrderManager.InitializeTurnOrder(playerTeam, enemyTeam);
        }

        currentState = CombatState.SelectingCharacter;
        charactersWhoFinishedTurn.Clear();
        StartTurnBasedSystem();
    }

    void StartTurnBasedSystem()
    {
        if (turnOrderManager == null) return;

        Chessman currentUnit = turnOrderManager.GetCurrentUnit();
        if (currentUnit == null)
        {
            Debug.LogError("Nincs aktuális bábu!");
            return;
        }

        if (currentUnit.isEnemy)
        {
            isPlayerTurn = false;
            UpdateTurnUI();
            StartCoroutine(ExecuteSingleEnemyTurn(currentUnit));
        }
        else
        {
            isPlayerTurn = true;
            UpdateTurnUI();
            ShowMessage($"A köröd: {currentUnit.GetName()}");
            SelectCharacter(currentUnit);
        }
    }

    IEnumerator ExecuteSingleEnemyTurn(Chessman enemyUnit)
    {
        ShowMessage($"{enemyUnit.GetName()} köre...");
        isProcessing = true;
        yield return new WaitForSeconds(0.75f);

        List<Chessman> alivePlayers = playerTeam.FindAll(p => p != null && p.IsAlive());
        if (alivePlayers.Count == 0)
        {
            CheckGameOver();
            yield break;
        }

        List<AIAction> allPossibleActions = new List<AIAction>();

        HashSet<Vector2Int> possibleMoveTiles = enemyUnit.GetValidMoveTiles();

        foreach (Vector2Int movePos in possibleMoveTiles)
        {
            EvaluateActionsFromPosition(enemyUnit, movePos, alivePlayers, allPossibleActions);
        }

        allPossibleActions.Add(new AIAction(AIActionType.Wait, 1.0f));

        AIAction bestAction = allPossibleActions.OrderByDescending(action => action.Score).First();

        if (bestAction.TileToMoveTo != enemyUnit.gridPosition && (bestAction.Type == AIActionType.Move || bestAction.Type == AIActionType.Attack))
        {
            enemyUnit.SetSelected(true);
            ShowMessage($"{enemyUnit.GetName()} lép.");
            yield return new WaitForSeconds(0.3f);
            yield return enemyUnit.MoveToTile(bestAction.TileToMoveTo);
            yield return new WaitForSeconds(0.3f);
        }

        if (bestAction.Type == AIActionType.Attack)
        {
            Chessman target = bestAction.TargetToAttack;
            enemyUnit.SetSelected(true);
            target.SetHighlight(true);
            ShowMessage($"{enemyUnit.GetName()} megtámadja {target.GetName()}-t!");
            yield return new WaitForSeconds(0.5f);

            if (attackSound != null && musicSource != null) musicSource.PlayOneShot(attackSound);
            yield return enemyUnit.AttackAnimation(target);
            if (hitSound != null && musicSource != null) musicSource.PlayOneShot(hitSound);

            target.SetHighlight(false);
            yield return new WaitForSeconds(0.5f);
        }

        if (bestAction.Type == AIActionType.Wait)
        {
            ShowMessage($"{enemyUnit.GetName()} várakozik.");
            yield return new WaitForSeconds(0.5f);
        }

        enemyUnit.SetSelected(false);

        isProcessing = false;
        if (!CheckGameOver())
        {
            EndCurrentTurn();
        }
    }

    private void EvaluateActionsFromPosition(Chessman aiUnit, Vector2Int simulatedPos, List<Chessman> allPlayers, List<AIAction> actionsList)
    {
        Chessman closestPlayer = FindClosestPlayer(aiUnit, allPlayers);
        if (closestPlayer == null) return; 

        int currentDistance = CalculateDistance(aiUnit.gridPosition, closestPlayer.gridPosition);
        int newDistance = CalculateDistance(simulatedPos, closestPlayer.gridPosition);
        float moveScore = (currentDistance - newDistance) * 10f; 

        foreach (Chessman player in allPlayers)
        {
            if (player.GetValidAttackTilesFrom(player.gridPosition).Contains(simulatedPos))
            {
                moveScore -= 50;
            }
        }
        
        HashSet<Vector2Int> attackTiles = aiUnit.GetValidAttackTilesFrom(simulatedPos);
        bool canAttack = false;

        foreach (Vector2Int attackTile in attackTiles)
        {
            Chessman target = gridManager.GetCharacterAt(attackTile);
            
            if (target != null && !target.isEnemy && target.IsAlive())
            {
                canAttack = true;
                
                float attackScore = 100f;
            
                attackScore += (target.GetMaxHealth() - target.GetCurrentHealth()) * 10; 
            
                if (target.GetCurrentHealth() <= aiUnit.GetDamage())
                {
                    attackScore += 1000f;
                }
                
                float totalScore = moveScore + attackScore;
                
                actionsList.Add(new AIAction(totalScore, simulatedPos, target));
            }
        }

        if (!canAttack && moveScore > 0)
        {
            actionsList.Add(new AIAction(moveScore, simulatedPos));
        }
    }


    void Update()
    {
        if (!isPlayerTurn || isProcessing || currentState == CombatState.GameOver) return;
        HandlePlayerInput();
    }

    //--------------------------------------------------------------------------
    // Játékos Input Kezelése
    //--------------------------------------------------------------------------

    void HandlePlayerInput()
    {
        Vector3 mp = Input.mousePosition;
        mp.z = -Camera.main.transform.position.z;
        Vector3 wp3 = Camera.main.ScreenToWorldPoint(mp);
        Vector2 wp = new Vector2(wp3.x, wp3.y);

        ClearAllHighlights();

        if (UnityEngine.EventSystems.EventSystem.current != null && !UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            Collider2D hoverCol = Physics2D.OverlapPoint(wp);
            Chessman hoveredChar = (hoverCol != null) ? hoverCol.GetComponentInParent<Chessman>() : null;

                if (hoveredChar != null && hoveredChar.IsAlive())
                {
                    // Only highlight the current unit when selecting a character so players cannot select other friendly units
                    Chessman currentUnit = (turnOrderManager != null) ? turnOrderManager.GetCurrentUnit() : null;

                    if (currentState == CombatState.SelectingCharacter && currentUnit != null && hoveredChar == currentUnit && !hoveredChar.isEnemy && !charactersWhoFinishedTurn.Contains(hoveredChar))
                        hoveredChar.SetHighlight(true);
                    else if (currentState == CombatState.SelectingTarget && hoveredChar.isEnemy)
                        hoveredChar.SetHighlight(true);
                    else if (currentState == CombatState.SelectingMoveTile && hoveredChar == selectedAttacker)
                        hoveredChar.SetHighlight(true);
                }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

            Collider2D col = Physics2D.OverlapPoint(wp);
            Chessman clickedChar = (col != null) ? col.GetComponentInParent<Chessman>() : null;
            Vector2Int gridPos = gridManager.WorldToGrid(wp3);

            // 1. BÁBU VÁLASZTÁS
            if (currentState == CombatState.SelectingCharacter)
            {
                // Only allow selecting the unit whose turn it is (from TurnOrderManager)
                Chessman currentUnit = (turnOrderManager != null) ? turnOrderManager.GetCurrentUnit() : null;
                if (clickedChar != null && clickedChar.IsAlive() && !clickedChar.isEnemy && !charactersWhoFinishedTurn.Contains(clickedChar) && clickedChar == currentUnit)
                {
                    SelectCharacter(clickedChar);
                }
            }
            // 2. LÉPÉS-HELY VÁLASZTÁS
            else if (currentState == CombatState.SelectingMoveTile)
            {
                // Allow moving to any reachable tile that is not occupied, except
                // allow the current tile (occupied by the selected attacker) to
                // be treated as a "stay" and open the action UI.
                if (reachableTiles != null && reachableTiles.Contains(gridPos) && (!gridManager.IsTileOccupied(gridPos) || gridPos == selectedAttacker?.gridPosition))
                {
                    if (selectedAttacker != null && gridPos == selectedAttacker.gridPosition)
                    {
                        // Clicked the current tile -> show action UI (stay)
                        ShowActionUI();
                    }
                    else
                    {
                        StartCoroutine(MoveCharacter(gridPos));
                    }
                }
                else if (clickedChar == selectedAttacker)
                {
                    ShowActionUI();
                }
            }
            // 3. CÉLPONT VÁLASZTÁS
            else if (currentState == CombatState.SelectingTarget)
            {
                // Determine what we're currently targeting: ability vs normal attack
                Ability.TargetType desiredTargetType = Ability.TargetType.Enemy; // default for normal attacks
                if (selectedAbility != null)
                {
                    desiredTargetType = selectedAbility.targetType;
                }

                // If this is a tile-targeting ability, allow clicking empty tiles as direction choices
                if (desiredTargetType == Ability.TargetType.Tile)
                {
                    // clicked an empty tile -> try to use as direction selection
                    if (gridManager == null || selectedAttacker == null)
                    {
                        ShowMessage("Hiba: nincs grid vagy kiválasztott bábu.");
                        return;
                    }

                    // Accept any tile that lies orthogonally from the attacker (same row or same column),
                    // not the attacker's own tile. This allows clicking 1,2,3 tiles ahead to pick that direction.
                    Vector2Int delta = gridPos - selectedAttacker.gridPosition;
                    bool isOrthogonalLine = (delta.x == 0 && delta.y != 0) || (delta.y == 0 && delta.x != 0);
                    if (isOrthogonalLine)
                    {
                        SelectTile(gridPos);
                    }
                    else
                    {
                        ShowMessage("Válassz egy irányt (fel/le/jobbra/balra) egy vonal mentén a bábutól!");
                    }

                    return;
                }

                // Non-tile targeting: require a clicked character
                if (clickedChar == null)
                {
                    gridManager.ClearAttackTiles();
                    ShowActionUI();
                    return;
                }

                bool validTarget = false;
                switch (desiredTargetType)
                {
                    case Ability.TargetType.Ally:
                        validTarget = !clickedChar.isEnemy;
                        if (!validTarget) ShowMessage("Csak csapattársat választhatsz!");
                        break;
                    case Ability.TargetType.Self:
                        validTarget = clickedChar == selectedAttacker;
                        if (!validTarget) ShowMessage("Ezt csak magadon használhatod!");
                        break;
                    case Ability.TargetType.Enemy:
                        validTarget = clickedChar.isEnemy && clickedChar.IsAlive();
                        if (!validTarget) ShowMessage("Őt nem támadhatod meg!");
                        break;
                }

                if (!validTarget) return;

                int distance = CalculateDistance(selectedAttacker.gridPosition, clickedChar.gridPosition);
                int currentAttackRange = (selectedAbility != null) ? selectedAbility.range : selectedAttacker.GetAttackRange();

                if (distance <= currentAttackRange)
                {
                    SelectTarget(clickedChar);
                }
                else
                {
                    ShowMessage("Nincs elég közel a támadáshoz!");
                }
            }
        }
    }

    //--------------------------------------------------------------------------
    // Játékos Akciók
    //--------------------------------------------------------------------------

    void SelectCharacter(Chessman character)
    {
        ClearSelection();
        selectedAttacker = character;
        selectedAttacker.SetSelected(true);
        characterHasActed = false;
        ShowMoveOptions();
    }

    void ShowMoveOptions()
    {
        currentState = CombatState.SelectingMoveTile;
        if (gridManager != null && selectedAttacker != null)
        {
            reachableTiles = selectedAttacker.GetValidMoveTiles();
            gridManager.ShowMoveTiles(reachableTiles, selectedAttacker.gridPosition);
        }
        ShowMessage("Lépj egy mezőre, vagy kattints a bábudra az akcióhoz.");
    }

    IEnumerator MoveCharacter(Vector2Int targetTile)
    {
        isProcessing = true;
        currentState = CombatState.MovingCharacter;
        gridManager.ClearMoveTiles();
        yield return selectedAttacker.MoveToTile(targetTile);
        isProcessing = false;
        ShowActionUI();
    }

    void ShowActionUI()
    {
        currentState = CombatState.CharacterSelected;
        gridManager.ClearMoveTiles();

        if (characterHasActed)
        {
            OnWait();
            return;
        }

        if (actionPanel != null) actionPanel.SetActive(true);

        if (attackButton != null) attackButton.interactable = !characterHasActed;

        if (abilityButton1 != null)
        {
            bool canUseAbility = !characterHasActed && selectedAttacker != null && selectedAttacker.GetAbilities().Count > 0;
            abilityButton1.gameObject.SetActive(canUseAbility);
            if (canUseAbility)
            {
                var buttonText = abilityButton1.GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null) buttonText.text = selectedAttacker.GetAbilities()[0].abilityName;
                abilityButton1.onClick.RemoveAllListeners();
                abilityButton1.onClick.AddListener(() => OnPrepareAbility(selectedAttacker.GetAbilities()[0]));
            }
        }

        if (waitButton != null) waitButton.interactable = !characterHasActed;

        ShowMessage("Válassz akciót!");
    }

    void HideActionUI()
    {
        if (actionPanel != null) actionPanel.SetActive(false);
    }

    void OnPrepareAttack()
    {
        if (characterHasActed) return;

        selectedAbility = null;
        HideActionUI();
        if (gridManager != null && selectedAttacker != null)
        {
            attackRangeTiles = selectedAttacker.GetValidAttackTiles();
            gridManager.ShowAttackTiles(attackRangeTiles);
        }
        currentState = CombatState.SelectingTarget;
        ShowMessage("Válaszd ki a célpontot!");
    }

    void OnPrepareAbility(Ability ability)
    {
        if (characterHasActed) return;

        selectedAbility = ability;
        HideActionUI();
        // If this is a self-targeting ability (no target click needed), execute immediately
        if (ability != null && ability.targetType == Ability.TargetType.Self)
        {
            StartCoroutine(ExecutePlayerAbility(ability, selectedAttacker));
            return;
        }

        if (gridManager != null && selectedAttacker != null)
        {
            Debug.Log($"KÉPESSÉG ELŐKÉSZÍTÉS: Bábu={selectedAttacker.GetName()}, Képesség={ability.abilityName}, Hatótáv={ability.range}");
            attackRangeTiles = gridManager.FindReachableTiles(selectedAttacker.gridPosition, ability.range, true);
            gridManager.ShowAttackTiles(attackRangeTiles);
        }
        else
        {
            Debug.LogError("Hiba az OnPrepareAbility során: gridManager vagy selectedAttacker hiányzik!");
        }
        currentState = CombatState.SelectingTarget;
        ShowMessage($"Válaszd ki a célpontot: {ability.abilityName}");
    }

    void OnWait()
    {
        if (characterHasActed) return;
        HideActionUI();
        StartCoroutine(ExecuteWait());
    }

    void SelectTarget(Chessman target)
    {
        selectedTarget = target;
        gridManager.ClearAttackTiles();

        if (selectedAbility != null)
        {
            StartCoroutine(ExecutePlayerAbility(selectedAbility, target));
        }
        else
        {
            StartCoroutine(ExecutePlayerAttack());
        }
    }

    void SelectTile(Vector2Int tilePos)
    {
        gridManager.ClearAttackTiles();
        StartCoroutine(ExecutePlayerAbility(selectedAbility, tilePos));
    }

    //--------------------------------------------------------------------------
    // Játékos Akció Coroutine-ok (Animációk)
    //--------------------------------------------------------------------------

    IEnumerator ExecuteWait()
    {
        isProcessing = true;
        currentState = CombatState.Processing;
        characterHasActed = true;
        ShowMessage($"{selectedAttacker.GetName()} várakozik.");
        yield return new WaitForSeconds(0.5f);
        CheckPlayerTurnEnd();
    }

    IEnumerator ExecutePlayerAttack()
    {
        isProcessing = true;
        currentState = CombatState.Processing;
        characterHasActed = true;
        ShowMessage($"{selectedAttacker.GetName()} megtámadja {selectedTarget.GetName()}-t!");
        if (attackSound != null && musicSource != null) musicSource.PlayOneShot(attackSound);

        yield return selectedAttacker.AttackAnimation(selectedTarget);

        if (hitSound != null && musicSource != null) musicSource.PlayOneShot(hitSound);

        CheckPlayerTurnEnd();
    }

    IEnumerator ExecutePlayerAbility(Ability ability, Chessman target)
    {
        isProcessing = true;
        currentState = CombatState.Processing;
        characterHasActed = true;
        ShowMessage($"{selectedAttacker.GetName()} használja: {ability.abilityName}!");


        yield return selectedAttacker.AbilityAnimation(target, ability);

        CheckPlayerTurnEnd();
    }

    IEnumerator ExecutePlayerAbility(Ability ability, Vector2Int tile)
    {
        isProcessing = true;
        currentState = CombatState.Processing;
        characterHasActed = true;
        ShowMessage($"{selectedAttacker.GetName()} használja: {ability.abilityName}!");

        // Play user's activation animation only if the ability allows it
        yield return selectedAttacker.AbilityAnimation(null, ability);

        // Call the tile-based activation hook
        try
        {
            ability.ActivateOnTile(selectedAttacker, tile);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Hiba a képesség ActivateOnTile meghívásakor: {ex}");
        }

        CheckPlayerTurnEnd();
    }

    bool IsOrthogonalNeighbor(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx == 1 && dy == 0) || (dx == 0 && dy == 1);
    }

    void CheckPlayerTurnEnd()
    {

        ClearSelection(); // Töröljük a kiválasztást, jelzőket
        isProcessing = false; // Újra lehet inputot fogadni

        if (CheckGameOver()) return;
        EndCurrentTurn();
    }
    
    void EndCurrentTurn()
    {
        if (turnOrderManager != null)
        {
            turnOrderManager.NextTurn();
            StartTurnBasedSystem();
        }
    }


    void ClearSelection()
    {
        HideActionUI();
        if (gridManager != null)
        {
            gridManager.ClearMoveTiles();
            gridManager.ClearAttackTiles();
        }

        if (selectedAttacker != null) selectedAttacker.SetSelected(false);

        selectedTarget = null;
        selectedAbility = null;
        reachableTiles?.Clear();
        attackRangeTiles?.Clear();
    }

    void ClearAllHighlights()
    {
        var allChars = new List<Chessman>(playerTeam);
        allChars.AddRange(enemyTeam);
        foreach (var character in allChars)
        {
            if (character != null && character != selectedAttacker)
            {
                character.SetHighlight(false);
            }
        }
    }

    bool CheckGameOver()
    {
        bool allPlayersDead = playerTeam.TrueForAll(p => p == null || !p.IsAlive());
        bool allEnemiesDead = enemyTeam.TrueForAll(e => e == null || !e.IsAlive());

        if (allPlayersDead || allEnemiesDead)
        {
            currentState = CombatState.GameOver;
            ShowMessage(allPlayersDead ? "VERESÉG!" : "GYŐZELEM!");
            if (allPlayersDead && defeatPanel != null) defeatPanel.SetActive(true);
            if (allEnemiesDead && victoryPanel != null) victoryPanel.SetActive(true);
            isProcessing = true;
            return true;
        }
        return false;
    }

    void UpdateTurnUI()
    {
        if (turnText != null)
        {
            turnText.text = isPlayerTurn ? "JÁTÉKOS KÖRE" : "ELLENSÉG KÖRE";
            turnText.color = isPlayerTurn ? new Color(0.2f, 1f, 0.2f) : new Color(1f, 0.2f, 0.2f);
        }
    }

    void ShowMessage(string message)
    {
        if (messageText != null) messageText.text = message;
    }

    public void RestartBattle()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    int CalculateDistance(Vector2Int posA, Vector2Int posB)
    {
        return Mathf.Max(Mathf.Abs(posA.x - posB.x), Mathf.Abs(posA.y - posB.y));
    }

    Chessman FindClosestPlayer(Chessman attacker, List<Chessman> alivePlayers)
    {
        Chessman closest = null;
        int minDistance = int.MaxValue;
        foreach (var player in alivePlayers)
        {
            if (player == null || !player.IsAlive()) continue;

            int distance = CalculateDistance(attacker.gridPosition, player.gridPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = player;
            }
        }
        return closest;
    }

    Vector2Int? FindPathStep(Vector2Int start, Vector2Int target, int maxMove)
    {
        if (gridManager == null || start == target || maxMove <= 0) return null;

        int dx = target.x - start.x;
        int dy = target.y - start.y;
        Vector2Int firstStep = start;

        if (Mathf.Abs(dx) > Mathf.Abs(dy))
        {
            firstStep.x += (int)Mathf.Sign(dx);
        }
        else if (Mathf.Abs(dy) > 0)
        {
            firstStep.y += (int)Mathf.Sign(dy);
        }
        else
        {
            return null;
        }

        if (gridManager.IsValidTile(firstStep) && !gridManager.IsTileOccupied(firstStep))
        {
            return firstStep;
        }

        if (Mathf.Abs(dx) <= Mathf.Abs(dy) && Mathf.Abs(dx) > 0)
        {
            firstStep = start;
            firstStep.x += (int)Mathf.Sign(dx);
        }
        else if (Mathf.Abs(dx) > Mathf.Abs(dy) && Mathf.Abs(dy) > 0)
        {
            firstStep = start;
            firstStep.y += (int)Mathf.Sign(dy);
        }

        if (firstStep != start && gridManager.IsValidTile(firstStep) && !gridManager.IsTileOccupied(firstStep))
        {
            return firstStep;
        }

        return null;
    }
}