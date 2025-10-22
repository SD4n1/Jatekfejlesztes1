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

        List<Chessman> allUnits = new List<Chessman>();
        allUnits.AddRange(playerTeam);
        allUnits.AddRange(enemyTeam);

        allUnits = allUnits.FindAll(unit => unit != null && unit.IsAlive());

        int playerIndex = 0;
        int enemyIndex = 0;

        while (playerIndex < playerTeam.Count || enemyIndex < enemyTeam.Count)
        {
            if (playerIndex < playerTeam.Count && playerTeam[playerIndex] != null && playerTeam[playerIndex].IsAlive())
            {
                turnOrder.Add(playerTeam[playerIndex]);
                playerIndex++;
            }

            if (enemyIndex < enemyTeam.Count && enemyTeam[enemyIndex] != null && enemyTeam[enemyIndex].IsAlive())
            {
                turnOrder.Add(enemyTeam[enemyIndex]);
                enemyIndex++;
            }
        }

        UpdateTurnOrderUI();
        Debug.Log($"Turn Order inicializálva: {turnOrder.Count} bábu");
    }

    public Chessman GetCurrentUnit()
    {
        if (turnOrder.Count == 0) return null;

        while (currentTurnIndex < turnOrder.Count)
        {
            Chessman current = turnOrder[currentTurnIndex];
            if (current != null && current.IsAlive())
            {
                return current;
            }
            currentTurnIndex++;
        }

        return null;
    }

    public void NextTurn()
    {
        currentTurnIndex++;

        if (currentTurnIndex >= turnOrder.Count)
        {
            currentTurnIndex = 0;
            RefreshTurnOrder();
        }

        UpdateTurnOrderUI();
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
                textComponent.text = prefix + unit.characterName;

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
