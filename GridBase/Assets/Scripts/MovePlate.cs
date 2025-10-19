using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovePlate : MonoBehaviour
{
    public GameObject controller;

    GameObject reference = null;

    int matrixX;
    int matrixY;

    //false: movement, true: attacking
    public bool attack = false;

    public void Start()
    {
        // Start(): ha ez a MovePlate támadó változat (attack == true), átállítja a sprite színét pirosra.
        if (attack)
        {
            gameObject.GetComponent<SpriteRenderer>().color = new Color(1.0f, 0.0f, 0.0f, 1.0f);
        }
    }

    public void OnMouseUp()
    {
        // OnMouseUp(): amikor a player rákattint egy MovePlate-re, végrehajtja a lépést:
        // - ha támadó MovePlate, eltávolítja a célbábút,
        // - üresíti a kiinduló mezőt a mátrixban,
        // - áthelyezi a referenciabábut az új koordinátára, frissíti a pozícióját,
        // - frissíti a positions mátrixot, sorcserét végez és törli a MovePlate-eket.
        controller = GameObject.FindGameObjectWithTag("GameController");

        // Ha támadásról van szó, a célpont életerejét csökkentjük a támadó sebzése alapján.
        if (attack)
        {
            GameObject cp = controller.GetComponent<Game>().GetPosition(matrixX, matrixY);
            if (cp != null)
            {
                Chessman target = cp.GetComponent<Chessman>();
                Chessman attacker = reference.GetComponent<Chessman>();

                bool targetDied = target.TakeDamage(attacker.GetDamage());

                if (targetDied)
                {
                    // Célpont meghalt: töröljük és folytatjuk a lépést (a támadó mozog a helyére)
                    Destroy(cp);
                }
                else
                {
                    // Célpont túlélte: nem mozog be a támadó, de a kör lejár
                    controller.GetComponent<Game>().NextTurn();
                    reference.GetComponent<Chessman>().DestroyMovePlates();
                    return;
                }
            }
        }

        // Áthelyezzük a támadót (vagy egyszerű mozgást végzünk)
        controller.GetComponent<Game>().SetPositionEmpty(reference.GetComponent<Chessman>().GetXBoard(),
            reference.GetComponent<Chessman>().GetYBoard());

        reference.GetComponent<Chessman>().SetXBoard(matrixX);
        reference.GetComponent<Chessman>().SetYBoard(matrixY);
        reference.GetComponent<Chessman>().SetCoords();

        controller.GetComponent<Game>().SetPosition(reference);
        controller.GetComponent<Game>().NextTurn();
        reference.GetComponent<Chessman>().DestroyMovePlates();
    }

    public void SetCoords(int x, int y)
    {
        // Beállítja ezt a MovePlate-et egy adott mátrix koordinátára.
        matrixX = x;
        matrixY = y;
    }

    public void SetReference(GameObject obj)
    {
        // A MovePlate-hez társítja a referenciabábut (azt a bábut, amelyik létrehozta a MovePlate-et).
        reference = obj;
    }

    public GameObject GetReference()
    {
        // Visszaadja a referenciabábut.
        return reference;
    }
}
