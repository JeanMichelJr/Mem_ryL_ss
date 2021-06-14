using System;
public class EnemySpecial3 : Enemy
{
    public float rateOfGrowth;
    public override void CastAttack()
    {
        Player.instance.life.Damage(attackDamage);
        SoundManager.instance.PlayOneShot("playerDamage");
        attackDamage = (int)Math.Floor(attackDamage * rateOfGrowth);
    }
}
