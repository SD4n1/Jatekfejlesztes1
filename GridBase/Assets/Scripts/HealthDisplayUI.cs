using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HealthDisplayUI : MonoBehaviour
{
    [Header("UI Referenciák")]
    public Transform playerHealthPanel;
    public Transform enemyHealthPanel;
    public GameObject healthEntryPrefab;

    private Dictionary<Chessman, TextMeshProUGUI> healthTexts = new Dictionary<Chessman, TextMeshProUGUI>();

    void Start()
    {
        UpdateHealthDisplay();
        InvokeRepeating("UpdateHealthDisplay", 0f, 0.5f);
    }

    void UpdateHealthDisplay()
    {
        ClearHealthDisplay();

        GameObject playerParent = GameObject.Find("Jatekos");
        if (playerParent != null)
        {
            DisplayHealthForTeam(playerParent, playerHealthPanel, false);
        }

        GameObject enemyParent = GameObject.Find("Ellenseg");
        if (enemyParent != null)
        {
            DisplayHealthForTeam(enemyParent, enemyHealthPanel, true);
        }
    }

    void DisplayHealthForTeam(GameObject teamParent, Transform panel, bool isEnemy)
    {
        Chessman[] chessmen = teamParent.GetComponentsInChildren<Chessman>();

        foreach (Chessman piece in chessmen)
        {
            if (!piece.gameObject.activeSelf || !piece.IsAlive()) continue;

            GameObject entry = Instantiate(healthEntryPrefab, panel);
            TextMeshProUGUI textComponent = entry.GetComponent<TextMeshProUGUI>();

            if (textComponent != null)
            {
                string displayText = $"{piece.GetName()}: {piece.GetCurrentHealth()}/{piece.GetMaxHealth()}";
                textComponent.text = displayText;
                healthTexts[piece] = textComponent;
            }
        }
    }

    void ClearHealthDisplay()
    {
        foreach (Transform child in playerHealthPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (Transform child in enemyHealthPanel)
        {
            Destroy(child.gameObject);
        }

        healthTexts.Clear();
    }
}

