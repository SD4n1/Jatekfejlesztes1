using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chessman : MonoBehaviour
{
    public GameObject controller;
    public GameObject movePlate;

    private int xBoard = -1;
    private int yBoard = -1;

    //Variable for keeping track of the player it belongs to "black" or "white"
    private string player;

    // Életerő (HP) és sebzés értékek minden bábuhoz. Inspectorból is módosíthatóak.
    public int health = 5;
    public int damage = 1;

    public Sprite black_queen, black_knight, black_bishop, black_king, black_rook, black_pawn;
    public Sprite white_queen, white_knight, white_bishop, white_king, white_rook, white_pawn;

    // Aktiválja a bábot: megtalálja a játékvezérlőt, beállítja a világ-koordinátákat és kiválasztja a megfelelő sprite-ot a név alapján.
    public void Activate()
    {
        //Get the game controller
        controller = GameObject.FindGameObjectWithTag("GameController");

        SetCoords();

        switch (this.name)
        {
            case "black_queen": this.GetComponent<SpriteRenderer>().sprite = black_queen; player = "black"; health = 8; damage = 4; break;
            case "black_knight": this.GetComponent<SpriteRenderer>().sprite = black_knight; player = "black"; health = 5; damage = 3; break;
            case "black_bishop": this.GetComponent<SpriteRenderer>().sprite = black_bishop; player = "black"; health = 5; damage = 2; break;
            case "black_king": this.GetComponent<SpriteRenderer>().sprite = black_king; player = "black"; health = 12; damage = 3; break;
            case "black_rook": this.GetComponent<SpriteRenderer>().sprite = black_rook; player = "black"; health = 7; damage = 3; break;
            case "black_pawn": this.GetComponent<SpriteRenderer>().sprite = black_pawn; player = "black"; health = 3; damage = 1; break;
            case "white_queen": this.GetComponent<SpriteRenderer>().sprite = white_queen; player = "white"; health = 8; damage = 4; break;
            case "white_knight": this.GetComponent<SpriteRenderer>().sprite = white_knight; player = "white"; health = 5; damage = 3; break;
            case "white_bishop": this.GetComponent<SpriteRenderer>().sprite = white_bishop; player = "white"; health = 5; damage = 2; break;
            case "white_king": this.GetComponent<SpriteRenderer>().sprite = white_king; player = "white"; health = 12; damage = 3; break;
            case "white_rook": this.GetComponent<SpriteRenderer>().sprite = white_rook; player = "white"; health = 7; damage = 3; break;
            case "white_pawn": this.GetComponent<SpriteRenderer>().sprite = white_pawn; player = "white"; health = 3; damage = 1; break;
        }
    }

    // Visszaadja a bábu aktuális életerejét.
    public int GetHealth()
    {
        return health;
    }

    // Visszaadja a bábu sebzését.
    public int GetDamage()
    {
        return damage;
    }

    // Csökkenti a bábu életét amount-tal. Visszatér true-val, ha a bábu meghalt (health <= 0).
    public bool TakeDamage(int amount)
    {
        health -= amount;
        return (health <= 0);
    }

    // Átalakítja a mátrixbeli (xBoard,yBoard) koordinátákat Unity világkoordinátává és beállítja a Transform pozíciót.
    public void SetCoords()
    {
        float x = xBoard;
        float y = yBoard;

    // Adjust by tile spacing and origin offset.
    // Board scale changed from 3 -> 5, so spacings were scaled accordingly (0.66 -> 1.1, offset -2.3 -> -3.833333).
    x *= 1.1f;
    y *= 1.1f;

    // Add constants (pos 0,0) adjusted for larger board scale
    x += -3.833333f;
    y += -3.833333f;

        this.transform.position = new Vector3(x, y, -1.0f);
    }

    public int GetXBoard()
    {
        return xBoard;
    }

    public int GetYBoard()
    {
        return yBoard;
    }

    public void SetXBoard(int x)
    {
        xBoard = x;
    }

    public void SetYBoard(int y)
    {
        yBoard = y;
    }

    // Esemény: a bábra kattintáskor hívódik. Törli a korábbi mozgásjelzőket és létrehozza az újakat,
    // ha a játék nincs vége és a bábu a soron következő játékoshoz tartozik.
    private void OnMouseUp()
    {
        Debug.Log("Clicked on: " + this.name);
        
        if (!controller.GetComponent<Game>().IsGameOver() && controller.GetComponent<Game>().GetCurrentPlayer() == player)
        {
            //Remove all moveplates relating to previously selected piece
            DestroyMovePlates();

            //Create new MovePlates
            InitiateMovePlates();
            // Kijelöljük a bábut a Game UI számára
            controller.GetComponent<Game>().SelectPiece(gameObject);
        }
    }

    // Törli az összes mozgás/ütés jelző objektumot a jelenlegi jelenetből.
    public void DestroyMovePlates()
    {
        //Destroy old MovePlates
        GameObject[] movePlates = GameObject.FindGameObjectsWithTag("MovePlate");
        for (int i = 0; i < movePlates.Length; i++)
        {
            Destroy(movePlates[i]); //Be careful with this function "Destroy" it is asynchronous
        }
    }

    // Létrehozza az adott bábuhoz tartozó lehetséges lépésekhez/ütésekhez tartozó MovePlate objektumokat a szabályok szerint.
    // A logika típustól függően külön segédfüggvényeket hív.
    public void InitiateMovePlates()
    {
        switch (this.name)
        {
            case "black_queen":
            case "white_queen":
                LineMovePlate(1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(1, 1);
                LineMovePlate(-1, 0);
                LineMovePlate(0, -1);
                LineMovePlate(-1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(1, -1);
                break;
            case "black_knight":
            case "white_knight":
                LMovePlate();
                break;
            case "black_bishop":
            case "white_bishop":
                LineMovePlate(1, 1);
                LineMovePlate(1, -1);
                LineMovePlate(-1, 1);
                LineMovePlate(-1, -1);
                break;
            case "black_king":
            case "white_king":
                SurroundMovePlate();
                break;
            case "black_rook":
            case "white_rook":
                LineMovePlate(1, 0);
                LineMovePlate(0, 1);
                LineMovePlate(-1, 0);
                LineMovePlate(0, -1);
                break;
            case "black_pawn":
                PawnMovePlate(xBoard - 1, yBoard);
                break;
            case "white_pawn":
                PawnMovePlate(xBoard + 1, yBoard);
                break;
        }
    }

    // Sorban haladó lépésekhez használatos: a megadott irányban (xIncrement,yIncrement)
    // végig létrehozza a MovePlate-eket amíg üres mezőt talál, majd ha ellenséges bábu van az út végén, azt támadó MovePlate-et hoz létre.
    public void LineMovePlate(int xIncrement, int yIncrement)
    {
        Game sc = controller.GetComponent<Game>();

        int x = xBoard + xIncrement;
        int y = yBoard + yIncrement;

        while (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y) == null)
        {
            MovePlateSpawn(x, y);
            x += xIncrement;
            y += yIncrement;
        }

        if (sc.PositionOnBoard(x, y) && sc.GetPosition(x, y).GetComponent<Chessman>().player != player)
        {
            MovePlateAttackSpawn(x, y);
        }
    }

    // L-lépések létrehozása (lovas mozgás): minden lehetséges L-dalakulat pozícióra PointMovePlate-et hív.
    public void LMovePlate()
    {
        PointMovePlate(xBoard + 1, yBoard + 2);
        PointMovePlate(xBoard - 1, yBoard + 2);
        PointMovePlate(xBoard + 2, yBoard + 1);
        PointMovePlate(xBoard + 2, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard - 2);
        PointMovePlate(xBoard - 1, yBoard - 2);
        PointMovePlate(xBoard - 2, yBoard + 1);
        PointMovePlate(xBoard - 2, yBoard - 1);
    }

    // Környezet vizsgálata: a király (vagy más, egy mezővel körülvevő) lépések létrehozása a 8 szomszédos mezőre.
    public void SurroundMovePlate()
    {
        PointMovePlate(xBoard, yBoard + 1);
        PointMovePlate(xBoard, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard + 0);
        PointMovePlate(xBoard - 1, yBoard - 1);
        PointMovePlate(xBoard - 1, yBoard + 1);
        PointMovePlate(xBoard + 1, yBoard + 0);
        PointMovePlate(xBoard + 1, yBoard - 1);
        PointMovePlate(xBoard + 1, yBoard + 1);
    }

    // Egyetlen pont lépés vizsgálata: ha a mező üres, létrehoz mozgó MovePlate-et, ha ellenséges bábú van, létrehoz támadó MovePlate-et.
    public void PointMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.PositionOnBoard(x, y))
        {
            GameObject cp = sc.GetPosition(x, y);

            if (cp == null)
            {
                MovePlateSpawn(x, y);
            }
            else if (cp.GetComponent<Chessman>().player != player)
            {
                MovePlateAttackSpawn(x, y);
            }
        }
    }

    // Gyalog lépések kezelése: az előre haladó mezőt (x,y) ellenőrzi és létrehozza a mozgó MovePlate-et,
    // valamint a gyaloghoz tartozó ütési lehetőségeket a y±1 pozíciókban (mivel előre az x irányban van a board-átalakítás szerint).
    public void PawnMovePlate(int x, int y)
    {
        Game sc = controller.GetComponent<Game>();
        if (sc.PositionOnBoard(x, y))
        {
            if (sc.GetPosition(x, y) == null)
            {
                MovePlateSpawn(x, y);
            }

            if (sc.PositionOnBoard(x, y + 1) && sc.GetPosition(x, y + 1) != null && sc.GetPosition(x, y + 1).GetComponent<Chessman>().player != player)
            {
                MovePlateAttackSpawn(x, y + 1);
            }

            if (sc.PositionOnBoard(x, y - 1) && sc.GetPosition(x, y - 1) != null && sc.GetPosition(x, y - 1).GetComponent<Chessman>().player != player)
            {
                MovePlateAttackSpawn(x, y - 1);
            }
        }
    }

    // Létrehoz egy mozgás-jelző (MovePlate) objektumot a megadott mátrixpozícióra (nem támadó változat),
    // beállítja a referencia bábút és a koordinátákat a MovePlate scriptben.
    public void MovePlateSpawn(int matrixX, int matrixY)
    {
        float x = matrixX;
        float y = matrixY;

    // Adjust by tile spacing and origin offset for new board scale
    x *= 1.1f;
    y *= 1.1f;

    // Add constants (pos 0,0) adjusted for larger board scale
    x += -3.833333f;
    y += -3.833333f;

        GameObject mp = Instantiate(movePlate, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }

    // Létrehoz egy támadó MovePlate-et (piros), beállítja attack=true és rögzíti a referencia bábut és koordinátákat.
    public void MovePlateAttackSpawn(int matrixX, int matrixY)
    {
        float x = matrixX;
        float y = matrixY;

    // Adjust by tile spacing and origin offset for new board scale
    x *= 1.1f;
    y *= 1.1f;

    // Add constants (pos 0,0) adjusted for larger board scale
    x += -3.833333f;
    y += -3.833333f;

        GameObject mp = Instantiate(movePlate, new Vector3(x, y, -3.0f), Quaternion.identity);

        MovePlate mpScript = mp.GetComponent<MovePlate>();
        mpScript.attack = true;
        mpScript.SetReference(gameObject);
        mpScript.SetCoords(matrixX, matrixY);
    }
}
