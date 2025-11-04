using UnityEngine;

// Ez egy "sablon" a k�pess�geknek.
public abstract class Ability : ScriptableObject
{
    public string abilityName;
    [TextArea]
    public string description;
    public Sprite icon;
    public int range = 1; // H�ny mez�re hat
    // If false, the user's activation animation/sound should not play when the ability is used.
    public bool playActivationAnimation = true;
    public enum TargetType { Self, Enemy, Ally, Tile }
    public TargetType targetType;
    public int cost = 0; // Pl. Mana cost

    // Ezt a f�ggv�nyt fogja minden k�pess�g megval�s�tani
    // FIGYELEM: Character helyett Chessman-t haszn�lunk!
    public abstract bool Activate(Chessman user, Chessman target);

    // Optional hook called when a reflect-type ability actually reflects an attack.
    // Default implementation does nothing. Abilities that need custom visuals/sounds
    // when the reflect triggers can override this.
    public virtual void OnReflect(Chessman user, Chessman attacker) { }

    // Optional hook for abilities that target a tile (e.g., directional abilities).
    // By default it falls back to the normal Activate with a null target.
    public virtual bool ActivateOnTile(Chessman user, UnityEngine.Vector2Int tile)
    {
        return Activate(user, null);
    }
}