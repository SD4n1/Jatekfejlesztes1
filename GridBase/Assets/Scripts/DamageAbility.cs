using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Ability", menuName = "Abilities/Damage Ability")]
public class DamageAbility : Ability
{
    public int damageAmount;
    public GameObject effectPrefab; // Pl. robbanás effekt

    // Felülírjuk az Activate funkciót, Chessman-t használva
    public override bool Activate(Chessman user, Chessman target)
    {
        // Ha nincs célpont VAGY a célpont szövetséges (barát)
        if (target == null || target.player == user.player)
        {
            // De ha a TargetType 'Self', akkor engedjük (bár sebzésnél fura)
            if (targetType != TargetType.Self)
            {
                return false; // Sikertelen használat, nem ellenségre céloztunk
            }
        }

        Debug.Log($"{user.name} használja: {abilityName} -> {target.name} (-{damageAmount} HP)");

        // A Chessman.cs-ben lévõ TakeDamage függvényt hívjuk
        target.TakeDamage(damageAmount);

        if (effectPrefab != null)
        {
            Instantiate(effectPrefab, target.transform.position, Quaternion.identity);
        }

        return true; // Sikeres használat
    }
}