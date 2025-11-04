using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TurnOrderManager : MonoBehaviour
{
    public static TurnOrderManager Instance { get; private set; }

    [Header("Turn Order Settings")]
    public List<Chessman> turnOrder = new List<Chessman>();
    private int currentTurnIndex = 0;

    [Header("UI References")]
    public Transform turnOrderPanel;
    public GameObject turnOrderEntryPrefab;
    public Color currentTurnColor = Color.yellow;
    public Color upcomingTurnColor = Color.white;
    public Color pastTurnColor = Color.gray;

    private List<GameObject> turnOrderUIElements = new List<GameObject>();

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

    public void InitializeTurnOrder(List<Chessman> playerTeam, List<Chessman> enemyTeam)
    {
        turnOrder.Clear();
        currentTurnIndex = 0;

        List<Chessman> alivePlayers = playerTeam.FindAll(unit => unit != null && unit.IsAlive());
        List<Chessman> aliveEnemies = enemyTeam.FindAll(unit => unit != null && unit.IsAlive());

        int playerIndex = 0;
        int enemyIndex = 0;

        while (playerIndex < alivePlayers.Count || enemyIndex < aliveEnemies.Count)
        {
            if (playerIndex < alivePlayers.Count)
            {
                turnOrder.Add(alivePlayers[playerIndex]);
                playerIndex++;
            }

            if (enemyIndex < aliveEnemies.Count)
            {
                turnOrder.Add(aliveEnemies[enemyIndex]);
                enemyIndex++;
            }
        }

        UpdateTurnOrderUI();
        Debug.Log($"Turn Order inicializálva: {turnOrder.Count} bábu");
    }

    public Chessman GetCurrentUnit()
    {
        turnOrder.RemoveAll(unit => unit == null || !unit.IsAlive());

        if (turnOrder.Count == 0)
        {
            Debug.LogWarning("Nincs több élő bábu a körsorrendben!");
            return null;
        }

        if (currentTurnIndex >= turnOrder.Count)
        {
            currentTurnIndex = 0;
        }

        return turnOrder[currentTurnIndex];
    }

    public void NextTurn()
    {
        currentTurnIndex++;

        UpdateTurnOrderUI();
        // If the current unit had a temporary reflect/shield active from a previous turn,
        // deactivate it now — reflect lasts only until the unit's next turn.
    Chessman currentUnit = GetCurrentUnit();
        if (currentUnit != null)
        {
            currentUnit.DeactivateReflect();
        }
    }

    void RefreshTurnOrder()
    {
        turnOrder.RemoveAll(unit => unit == null || !unit.IsAlive());

        if (turnOrder.Count == 0)
        {
            Debug.LogWarning("Nincs több élő bábu a körsorrendben!");
            return;
        }

        Debug.Log($"Új kör kezdődik! {turnOrder.Count} élő bábu van.");
    }

    // Immediately remove a specific unit from the turn order (e.g. when it dies)
    public void RemoveUnit(Chessman unit)
    {
        if (unit == null) return;

        // Remove the exact unit and any null/dead leftovers
        turnOrder.RemoveAll(u => u == null || !u.IsAlive() || u == unit);

        // Clamp currentTurnIndex
        if (turnOrder.Count == 0)
        {
            currentTurnIndex = 0;
        }
        else if (currentTurnIndex >= turnOrder.Count)
        {
            currentTurnIndex = 0;
        }

        UpdateTurnOrderUI();
    }

    public void UpdateTurnOrderUI()
    {
        foreach (var element in turnOrderUIElements)
        {
            Destroy(element);
        }
        turnOrderUIElements.Clear();

        if (turnOrderPanel == null || turnOrderEntryPrefab == null) return;

        int displayCount = Mathf.Min(turnOrder.Count, 8);

        for (int i = 0; i < displayCount; i++)
        {
            int index = (currentTurnIndex + i) % turnOrder.Count;
            if (index >= turnOrder.Count) break;

            Chessman unit = turnOrder[index];
            if (unit == null || !unit.IsAlive()) continue;

            GameObject entry = Instantiate(turnOrderEntryPrefab, turnOrderPanel);
            turnOrderUIElements.Add(entry);

            TextMeshProUGUI textComponent = entry.GetComponent<TextMeshProUGUI>();
            Image imageComponent = entry.GetComponent<Image>();

            if (textComponent != null)
            {
                string prefix = unit.isEnemy ? "[E] " : "[J] ";
                textComponent.text = prefix + unit.GetName();

                if (i == 0)
                {
                    textComponent.color = currentTurnColor;
                    textComponent.fontStyle = FontStyles.Bold;
                }
                else
                {
                    textComponent.color = upcomingTurnColor;
                    textComponent.fontStyle = FontStyles.Normal;
                }
            }

            if (imageComponent != null)
            {
                if (i == 0)
                {
                    imageComponent.color = new Color(1f, 1f, 0f, 0.3f);
                }
                else
                {
                    imageComponent.color = new Color(1f, 1f, 1f, 0.1f);
                }
            }
        }
    }

    public bool IsPlayerTurn()
    {
        Chessman current = GetCurrentUnit();
        return current != null && !current.isEnemy;
    }
}
