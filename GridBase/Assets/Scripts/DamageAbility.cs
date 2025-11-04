using UnityEngine;

[CreateAssetMenu(fileName = "New Damage Ability", menuName = "Abilities/Damage Ability")]
public class DamageAbility : Ability
{
    public int damageAmount;
    public GameObject effectPrefab; // Pl. robban�s effekt

    // Fel�l�rjuk az Activate funkci�t, Chessman-t haszn�lva
    public override bool Activate(Chessman user, Chessman target)
    {
        // Ha nincs c�lpont VAGY a c�lpont sz�vets�ges (bar�t)
        if (target == null || target.player == user.player)
        {
            // De ha a TargetType 'Self', akkor engedj�k (b�r sebz�sn�l fura)
            if (targetType != TargetType.Self)
            {
                return false; // Sikertelen haszn�lat, nem ellens�gre c�loztunk
            }
        }

        Debug.Log($"{user.name} haszn�lja: {abilityName} -> {target.name} (-{damageAmount} HP)");

    // A Chessman.cs-ben l�v� TakeDamage f�ggv�nyt h�vjuk (include attacker so reflect works)
    target.TakeDamage(damageAmount, user);

        if (effectPrefab != null)
        {
            Instantiate(effectPrefab, target.transform.position, Quaternion.identity);
        }

        return true; // Sikeres haszn�lat
    }
}