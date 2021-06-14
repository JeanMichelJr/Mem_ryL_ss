using System;
public class EnemySpecial4 : Enemy
{
    public float healPercentage;

    public override void CastAttack()
    {
        Enemy lowEnemy = CombatManager.instance.getLowestHealthRatioEnemy();
        if (lowEnemy != this)
        {
            lowEnemy.life.Heal((int)Math.Floor(lowEnemy.life.maxLife * healPercentage));
        }
        else
        {
            SoundManager.instance.PlayOneShot("playerDamage");
            Player.instance.life.Damage(attackDamage);
        }
    }
}
