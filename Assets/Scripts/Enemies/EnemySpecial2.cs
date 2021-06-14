public class EnemySpecial2 : Enemy
{
    private bool isSilenceReady = true;

    public override void CastAttack()
    {
        if (isSilenceReady && this.life.currentLife < (0.5 * this.life.maxLife))
        {
            CombatManager.instance.isHeroSilenced = true;

            isSilenceReady = false;
        }
        SoundManager.instance.PlayOneShot("playerDamage");
        Player.instance.life.Damage(attackDamage);
    }
}
