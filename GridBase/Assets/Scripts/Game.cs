using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Game : MonoBehaviour
{
    public GameObject chesspiece;

    private GameObject[,] positions = new GameObject[8, 8];
    private GameObject[] playerBlack = new GameObject[6];
    private GameObject[] playerWhite = new GameObject[6];

    //current turn
    private string currentPlayer = "white";

    //Game Ending
    private bool gameOver = false;

    // UI elemek a kiválasztott bábu adatainak megjelenítéséhez (Inspectorból rendelhető)
    public GameObject selectedPanel; // opcionális panel, amit be lehet kapcsolni/ki
    public TextMeshProUGUI selectedNameText;
    public TextMeshProUGUI selectedHealthText;
    public TextMeshProUGUI selectedDamageText;

    // Tárolt referenciája az éppen kiválasztott bábunak
    private GameObject selectedPiece = null;

    // Inicializálja a játékot: létrehozza a bábukat a tábla kezdőpozícióiba és feltölti a positions mátrixot.
    public void Start()
    {
        // Place white pieces on the left side (x = 0 and x = 1 columns)
        playerWhite = new GameObject[] { Create("white_pawn", 0, 1),
            Create("white_bishop", 0, 2), Create("white_queen", 0, 3), Create("white_king", 0, 4),
            Create("white_rook", 0, 5), Create("white_knight", 0, 6) };

        // Place black pieces on the right side (x = 7 and x = 6 columns)

        playerBlack = new GameObject[] { Create("black_pawn", 7, 1),
            Create("black_bishop", 7, 2), Create("black_queen", 7, 3), Create("black_king", 7, 4),
            Create("black_rook", 7, 5), Create("black_knight", 7, 6) };

        //Set all piece positions on the positions board
        for (int i = 0; i < playerBlack.Length; i++)
        {
            SetPosition(playerBlack[i]);
            SetPosition(playerWhite[i]);
        }
    }

    // Létrehoz (példányosít) egy chesspiece GameObject-et, beállítja a nevét és a mátrixkoordinátáit,
    // majd meghívja a bábu Activate() metódusát. Visszaadja az elkészült GameObject-et.
    public GameObject Create(string name, int x, int y)
    {
        GameObject obj = Instantiate(chesspiece, new Vector3(0, 0, -1), Quaternion.identity);
        Chessman cm = obj.GetComponent<Chessman>(); 
        cm.name = name; 
        cm.SetXBoard(x);
        cm.SetYBoard(y);
        cm.Activate(); 
        return obj;
    }

    // A positions mátrixba beállítja a megadott GameObject helyét a benne található Chessman script GetXBoard/GetYBoard értékei alapján.
    public void SetPosition(GameObject obj)
    {
        Chessman cm = obj.GetComponent<Chessman>();

        positions[cm.GetXBoard(), cm.GetYBoard()] = obj;
    }

    // A positions mátrix adott mezőjét törli (beállítja null-ra), jelezve, hogy oda nincs bábu.
    public void SetPositionEmpty(int x, int y)
    {
        positions[x, y] = null;
    }

    // Visszaadja a positions mátrix (x,y) pozícióján található GameObject-et (vagy null-t ha üres).
    public GameObject GetPosition(int x, int y)
    {
        return positions[x, y];
    }

    // Ellenőrzi, hogy az (x,y) koordináták a tábla határain belül vannak-e.
    public bool PositionOnBoard(int x, int y)
    {
        if (x < 0 || y < 0 || x >= positions.GetLength(0) || y >= positions.GetLength(1)) return false;
        return true;
    }

    // Visszaadja, hogy melyik játékos van soron ("white" vagy "black").
    public string GetCurrentPlayer()
    {
        return currentPlayer;
    }

    // Jelzi, hogy a játék véget ért-e.
    public bool IsGameOver()
    {
        return gameOver;
    }

    // Váltja a soron következő játékost (white <-> black).
    public void NextTurn()
    {
        if (currentPlayer == "white")
        {
            currentPlayer = "black";
        }
        else
        {
            currentPlayer = "white";
        }
    }

    // Unity Update: ha a játék véget ért és a felhasználó kattint, újratölti a Game jelenetet.
    public void Update()
    {
        if (gameOver == true && Input.GetMouseButtonDown(0))
        {
            gameOver = false;

            SceneManager.LoadScene("Game");
        }
    }
    
    // Eljárás meghívása, amikor valaki nyer: beállítja a gameOver flag-et és megjelenítheti a feliratokat.
    public void Winner(string playerWinner)
    {
        gameOver = true;
    }

    // Kiválaszt egy bábut és frissíti a UI-t az élet/sebzés értékekkel.
    public void SelectPiece(GameObject piece)
    {
        selectedPiece = piece;

        if (selectedPanel != null) selectedPanel.SetActive(true);

        if (selectedNameText != null) selectedNameText.text = piece.name;

        Chessman cm = piece.GetComponent<Chessman>();
        if (cm != null)
        {
            if (selectedHealthText != null) selectedHealthText.text = "HP: " + cm.GetHealth().ToString();
            if (selectedDamageText != null) selectedDamageText.text = "DMG: " + cm.GetDamage().ToString();
        }
    }

    // Törli a kiválasztást és elrejti a panelt
    public void ClearSelection()
    {
        selectedPiece = null;
        if (selectedPanel != null) selectedPanel.SetActive(false);
        if (selectedNameText != null) selectedNameText.text = "";
        if (selectedHealthText != null) selectedHealthText.text = "";
        if (selectedDamageText != null) selectedDamageText.text = "";
    }
}
