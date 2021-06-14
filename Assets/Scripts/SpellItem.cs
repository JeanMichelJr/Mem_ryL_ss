public class SpellItem : MenuItem
{
    public override bool testLetter(char c)
    {
        return Player.instance.unlockedLetters.Contains(c) && base.testLetter(c);
    }
}
