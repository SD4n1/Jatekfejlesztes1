using UnityEngine;

// Ez egy "sablon" a képességeknek.
public abstract class Ability : ScriptableObject
{
    public string abilityName;
    [TextArea]
    public string description;
    public Sprite icon;
    public int range = 1; // Hány mezõre hat
    public enum TargetType { Self, Enemy, Ally, Tile }
    public TargetType targetType;
    public int cost = 0; // Pl. Mana cost

    // Ezt a függvényt fogja minden képesség megvalósítani
    // FIGYELEM: Character helyett Chessman-t használunk!
    public abstract bool Activate(Chessman user, Chessman target);
}