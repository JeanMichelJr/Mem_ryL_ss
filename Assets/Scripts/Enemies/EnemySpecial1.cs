public class EnemySpecial1 : Enemy
{
    public float dotDuration;

    public override void CastAttack()
    {
        Player.instance.life.DamageOverTime(attackDamage, dotDuration);
        SoundManager.instance.PlayOneShot("playerDamage");
    }
}
