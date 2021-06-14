using System.Collections.Generic;
using System.Linq;
using System;

public class EventItem : MenuItem
{
    public List<EventManager.EventType> eventTypes;
    public float lifeHeal;
    public float manaHeal;
    public int probaOppositeHeal = 0;
    public int nbLetters;
    public string result;

    private string Spell;

    public int keyBoardNumber;

    protected override void BeforeAwake()
    {
        if (eventTypes.Contains(EventManager.EventType.Spell))
        {
            if (textUI.text == "" || textUI.text == null)
            {
                Spell = SpellManager.instance.spellNames.TakeNFirstRnd(1, x => x, Player.instance.spells.Union(EventManager.instance.chosenSpells)).FirstOrDefault();
                if(Spell != null)
                {
                    EventManager.instance.chosenSpells.Add(Spell);
                    textUI.text = Spell.ToUpper();
                }
                else
                {
                    List<char> list = "αβγΓδΔεζηθΘ℩ικλΛμνξΞοπΠρσςΣτυφΦχψΨωΩ".ToList().TakeNFirstRnd(5, x => x);
                    textUI.text = String.Concat(list);
                }
            }
            else
            {
                Spell = SpellManager.instance.spellNames.TakeNFirstRnd(1, x => x, Player.instance.spells).FirstOrDefault();
            }
        }

        if (eventTypes.Contains(EventManager.EventType.PlayerName))
        {
            textUI.text = Player.instance.getFormattedName();
        }

        if (eventTypes.Contains(EventManager.EventType.Random))
        {
            var choices = textUI.text.Split(',').ToList();
            textUI.text = choices.TakeNFirstRnd(1, x => x.ToUpper().Replace(',', '\0')).FirstOrDefault();
        }

        if (eventTypes.Contains(EventManager.EventType.NumberSelect) && keyBoardNumber >= 0 && keyBoardNumber <= 10)
        {
            EventManager.instance.NumberItems.Add(keyBoardNumber, this);
        }

        if (!eventTypes.Contains(EventManager.EventType.Void))
        {
            textUI.text = Player.instance.getFormattedText(textUI.text);
        }
    }

    public override bool testLetter(char c)
    {
        return base.testLetter(c);// && (eventTypes.Contains(EventManager.EventType.Void) ? true : Player.instance.unlockedLetters.Contains(c));
    }

    public void Select()
    {
        var EventResult = new EventManager.EventResult();

        if (eventTypes.Contains(EventManager.EventType.Heal))
        {
            if(probaOppositeHeal > 0)
            {
                var rand = new System.Random();
                if (rand.Next(100) < probaOppositeHeal)
                {
                    lifeHeal = -lifeHeal/2;
                    manaHeal = -manaHeal/2;
                }
            }

            bool isLHeal = lifeHeal > 0; 
            bool isMHeal = manaHeal > 0;


            float lifeAmount = (isLHeal ? lifeHeal : -lifeHeal);
            float manaAmount = (isMHeal ? manaHeal : -manaHeal);
            if (lifeAmount != Math.Round(lifeAmount))
            {
                lifeAmount = (float)Math.Round((isLHeal ? Player.instance.life.maxLife : Player.instance.life.currentLife) * lifeAmount);
            }
            if (manaAmount != Math.Round(manaAmount))
            {
                manaAmount = (float)Math.Round((isMHeal ? Player.instance.mana.maxMana : Player.instance.mana.currentMana) * manaAmount);
            }

            if(lifeAmount != 0)
            {
                //Soins
                if (isLHeal)
                {
                    Player.instance.life.Heal((int)lifeAmount);
                    EventResult.conclusion += "Vous regagnez " + (int)lifeAmount + " points de vie\n";
                }
                //Perte de vie
                else
                {
                    Player.instance.life.onDeath += SetUpDeathReason;
                    Player.instance.life.Damage((int)lifeAmount);
                    EventResult.conclusion += "Vous perdez " + (int)lifeAmount + " points de vie\n";
                }
            }

            if(manaAmount != 0)
            {
                //Récupération de mana
                if (isMHeal)
                {
                    Player.instance.mana.Restore((int)manaAmount);
                    EventResult.conclusion += "Vous regagnez " + (int)manaAmount + " points de mana\n";
                }
                //Perte de mana
                else
                {
                    Player.instance.mana.Consume((int)manaAmount);
                    EventResult.conclusion += "Vous perdez " + (int)manaAmount + " points de mana\n";
                }
            }
        }

        if (eventTypes.Contains(EventManager.EventType.Letter))
        {
            if(nbLetters > 0)
            {
                List<char> allLetters = Enumerable.Range(0, 26).Select( i => (char)('a' + i)).ToList();
                List<char> letter = allLetters.TakeNFirstRnd(nbLetters, c => c, Player.instance.unlockedLetters);
                letter.ForEach(x => Player.instance.unlockedLetters.Add(x));
                if(letter.Count > 1)
                {
                    EventResult.conclusion += "Vous vous souvenez des lettres ";
                }
                else if(letter.Count > 0)
                {
                    EventResult.conclusion += "Vous vous souvenez de la lettre ";
                }
                else if( letter.Count == 0 && nbLetters != 0)
                {
                    EventResult.conclusion += "Vous vous rappelez déjà de toutes les lettres !\n";
                }

                foreach (var l in letter)
                {
                    EventResult.conclusion += l.ToString().ToUpper();
                    EventResult.conclusion += l == letter.Last() ? "\n" : ", ";
                }
            }
            if(nbLetters < 0)
            {
                if(Player.instance.unlockedLetters.Count <= 5)
                {
                    EventResult.conclusion += "Vous ne pouvez pas oubliez plus de lettres...\n";
                }
                else
                {
                    List<char> letter = Player.instance.unlockedLetters.TakeNFirstRnd(-nbLetters, c => c);
                    if (letter.Count > 1)
                    {
                        EventResult.conclusion += "Vous oubliez les lettres ";
                    }
                    else if (letter.Count > 0)
                    {
                        EventResult.conclusion += "Vous vous oubliez la lettre ";
                    }

                    foreach (var l in letter)
                    {
                        Player.instance.unlockedLetters.Remove(l);
                        EventResult.conclusion += l.ToString().ToUpper();
                        EventResult.conclusion += l == letter.Last() ? "\n" : ", ";
                    }
                }
            }
        }

        if (eventTypes.Contains(EventManager.EventType.Spell))
        {
            if(Spell != null && SpellManager.instance.spellWarehouse.ContainsKey(Spell))
            {
                Player.instance.spells.Add(Spell);
                EventResult.conclusion += "Vous obtenez le sort " + Spell.ToUpper() + "\n";
            }
            else
            {
                EventResult.conclusion += "Vous connaissez déjà tous les sorts !\n";
            }
        }

        EventResult.result = result;
        EventManager.instance.NextState(EventResult);
    }

    private void SetUpDeathReason()
    {
        Player.instance.deathReason = result;
    }

    private void OnDestroy()
    {
        if(Player.instance != null)
        {
            Player.instance.life.onDeath -= SetUpDeathReason;
        }
    }
}
