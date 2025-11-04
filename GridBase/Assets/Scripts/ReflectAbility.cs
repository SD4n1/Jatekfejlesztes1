using UnityEngine;

[CreateAssetMenu(fileName = "ReflectAbility", menuName = "Abilities/ReflectAbility")]
public class ReflectAbility : Ability
{
    public override bool Activate(Chessman user, Chessman target)
    {
        if (user == null) return false;
        // Pass this ability instance so the Chessman can call back into the ability
        // when the reflect actually triggers (to play VFX/sound specific to the ability).
        user.ActivateReflect(this);
        return true;
    }

    void OnEnable()
    {
        abilityName = string.IsNullOrEmpty(abilityName) ? "Reflect" : abilityName;
        targetType = TargetType.Self;
        range = 0;
        // Do not play the user's activation animation for reflect; the visual should play when the unit is attacked
        playActivationAnimation = false;
    }

    // When the reflect actually triggers (the unit is attacked), play a visual/sound.
    public override void OnReflect(Chessman user, Chessman attacker)
    {
        // Default behaviour: trigger the user's "Reflect" animation trigger and play ability sound.
        // If you want custom VFX or different triggers, expand this method.
        try { user.SetAnimationTrigger("Ability"); } catch { }
        user.PlayAbilitySound();
    }
}
