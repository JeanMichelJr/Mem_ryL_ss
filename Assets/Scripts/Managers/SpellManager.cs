using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpellManager : MonoBehaviour
{
    public static SpellManager instance = null;

    public Dictionary<string, Spell> spellWarehouse = new Dictionary<string, Spell>()
    {
        ["flamme"] = new Spell
                        {
                            displyableName = "flamme",
                            requireTarget = true,
                            manaCost = 10,
                            body = (e, c) => e.life.Damage(.25f * Player.instance.spellAmplification),
                            description = "faibles dégâts ciblés",
                            color = Color.red,
                            anim = Spell.AnimType.Target
                        },
        ["murdeflammes"] = new Spell
                        {
                            displyableName = "mur de flammes",
                            requireTarget = false,
                            manaCost = 50,
                            body = (e, c) =>
                            {
                                foreach (var enemy in c)
                                {
                                    enemy.life.Damage(.15f * Player.instance.spellAmplification);
                                }
                            },
                            description = "faibles dégâts de zone",
                            color = Color.red,
                            anim = Spell.AnimType.Zone
        },
        ["soin"] = new Spell
                        {
                            displyableName = "soin",
                            requireTarget = false,
                            manaCost = 15,
                            body = (e, c) => Player.instance.life.Heal(.1f * Player.instance.spellAmplification),
                            description = "faibles soins",
                            color = Color.yellow,
                            anim = Spell.AnimType.Life
                        },
        ["benediction"] = new Spell
                        {
                            displyableName = "bénédiction",
                            requireTarget = false,
                            manaCost = 50,
                            body = (e, c) => Player.instance.life.Heal(1f),
                            description = "Régénère toute votre santé",
                            color = Color.yellow,
                            anim = Spell.AnimType.Life
        },
        ["garde"] = new Spell
                        {
                            displyableName = "garde",
                            requireTarget = false,
                            manaCost = 15,
                            body = (e, c) => SpellUtils.PlayerApplyDamageReduction(1f, 1),
                            description = "Vous protège de la prochaine attaque",
                            color = Color.yellow,
                            anim = Spell.AnimType.Life
        },
        ["invincible"] = new Spell
                        {
                            displyableName = "invincible",
                            requireTarget = false,
                            manaCost = 55,
                            body = (e, c) => SpellUtils.PlayerApplyDamageReduction(1f, 3),
                            description = "Vous protège des 3 prochaines attaques",
                            color = Color.yellow,
                            anim = Spell.AnimType.Life
        },
        ["poison"] = new Spell
                        {
                            displyableName = "poison",
                            requireTarget = true,
                            manaCost = 20,
                            body = (e, c) => e.life.DamageOverTime(.01f * Player.instance.spellAmplification, 5f),
                            description = "Inflige un faible poison à un ennemi pendant 5 secondes",
                            color = new Color32(75, 0, 130, 255),
                            anim = Spell.AnimType.Target
        },
        ["fumeestoxiques"] = new Spell
                        {
                            displyableName = "fumées toxiques",
                            requireTarget = false,
                            manaCost = 60,
                            body = (e, c) => 
                            {
                                foreach (var enemy in c)
                                {
                                    enemy.life.DamageOverTime(.01f * Player.instance.spellAmplification, 5f);
                                }
                            },
                            description = "Inflige un faible poison de zone pendant 5 secondes",
                            color = new Color32(75, 0, 130, 255),
                            anim = Spell.AnimType.Zone
                        },                
        ["concentration"] = new Spell
                        {
                            displyableName = "concentration",
                            requireTarget = false,
                            manaCost = 15,
                            body = (e, c) => 
                            {
                                Player.instance.spellAmplification = 2f;
                                Player.instance.spellAmplificationLeft = 1;
                            },
                            description = "Double les effets du prochain sort",
                            color = Color.cyan,
                            anim = Spell.AnimType.Mana
                        },
        ["polyglotte"] = new Spell
                        {
                            displyableName = "polyglotte",
                            requireTarget = false,
                            manaCost = 30,
                            body = (e, c) => 
                            {
                                if (CombatManager.instance != null)
                                {
                                    CombatManager.instance.unlockedLetters = Enumerable.Range(0, 26).Select( i => (char)('a' + i) ).ToList();
                                }
                                if(UIManager.instance != null)
                                {
                                    UIManager.instance.UpdateKeyBoard();
                                }
                            },
                            description = "Débloque toutes les lettres pour le reste du combat",
                            color = Color.cyan,
                            anim = Spell.AnimType.KeyBoard
        },
        ["ritueldepuissanceecrasanterebarbatif"] = new Spell
                        {
                            displyableName = "rituel de puissance écrasante rébarbatif",
                            requireTarget = false,
                            manaCost = 15,
                            body = (e, c) => 
                            {
                                CombatManager.instance.rprbCounter--;
                                if (CombatManager.instance.rprbCounter <= 0)
                                {
                                    foreach (var enemy in c)
                                    {
                                        enemy.life.Kill();
                                    }
                                }
                            },
                            description = "Ce sort doit bien faire quelque chose, il faut juste un peu de persévérance"
                        },
        ["revelucide"] = new Spell
                        {
                            displyableName = "rêve lucide",
                            requireTarget = false,
                            manaCost = 0,
                            body = (e, c) => Player.instance.mana.RestoreLeftMana(0.2f),
                            description = "Restaure 20% du mana manquant",
                            color = Color.cyan,
                            anim = Spell.AnimType.Mana
        }
    };

    private List<string> _spellNames = null;
    public List<string> spellNames
    {
        get
        {
            if (_spellNames == null)
            {
                _spellNames = spellWarehouse.Keys.ToList();
            }
            return _spellNames;
        }
    }

    private void Awake()
    {
        if (instance != this)
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
