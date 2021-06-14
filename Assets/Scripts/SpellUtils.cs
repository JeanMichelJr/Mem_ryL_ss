using UnityEngine;

public static class SpellUtils
{
    public static void PlayerApplyDamageReduction(float damageReduction, int n_attacks)
    {
        Player.instance.life.damageModifier = -damageReduction;
        Player.instance.damageModificationLeft = n_attacks * Mathf.RoundToInt(Player.instance.spellAmplification);

        void OnTakeDamage(float current_life, float last_update)
        {
            Player.instance.damageModificationLeft--;

            if (Player.instance.damageModificationLeft <= 0)
            {
                Player.instance.damageModificationLeft = 0;
                Player.instance.life.damageModifier = 0f;
                Player.instance.life.onTakeDamage -= OnTakeDamage;
            }
        }

        Player.instance.life.onTakeDamage += OnTakeDamage;
    }
}