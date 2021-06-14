using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System;
public class Player : MonoBehaviour
{
    public static Player instance;
    public List<char> unlockedLetters { get; private set; } = new List<char>();
    public LifeHandler life;
    public ManaHandler mana = new ManaHandler() { maxMana = 100 };
    public Graph graph;

    public int levelHeight = 20;
    public int roomLevel { get; private set; }
    public int finalRoomLevel = 2;

    public List<string> FirstNames;
    public List<string> LastNames;
    public string pName;
    public string deathReason;

    [Space, Header("Debug")]
    [Range(3, 26)]
    public int nbLetters = 3;

    public int startingSpells = 3;

    //public int damage => ((int)Math.Floor(Math.Log(unlockedLetters.Count) * unlockedLetters.Count));

    public int damageModificationLeft = 0;
    public float spellAmplification = 1f;
    public int spellAmplificationLeft = 0;

    public List<string> spells { get; private set; } = new List<string>();

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

    private void OnEnable()
    {
        life.Spawn();
        life.onTakeDamage += OnTakeDamage;
        life.onHeal += OnHeal;
        life.onDeath += OnDeath;

        mana.Spawn();
        mana.onRestore += OnManaRestore;
        mana.onConsume += OnManaConsume;

        deathReason = "Vous avez été tué...";

        unlockedLetters = Enumerable.Range(0, 26).Select(c => (char)('a' + c)).ToList().TakeNFirstRnd(nbLetters, x => x).ToList();

        var sb = new StringBuilder();

        if (SpellManager.instance != null)
        {

            spells = SpellManager.instance.spellNames.TakeNFirstRnd(startingSpells, x => x);
            //spells.AddRange(SpellManager.instance.spellNames);

            sb.AppendLine("Unlocked spells :");

            foreach (var spell in spells)
            {
                sb.AppendLine($"{spell}");
            }
            Debug.Log(sb.ToString());
        }

        sb.Clear();
        foreach (var ul in unlockedLetters)
        {
            sb.Append($"{ul};");
        }
        Debug.Log($"Unlocked letters {sb.ToString()}");

        roomLevel = 0;
        graph = new Graph(levelHeight);

        pName = FirstNames.TakeNFirstRnd(1, c => c).FirstOrDefault() + "  " + LastNames.TakeNFirstRnd(1, c => c).FirstOrDefault();
    }
    
    private void OnDeath()
    {
        Debug.Log("You Died", this);
        SoundManager.instance.PlayMenuMusic();
        GameController.instance.MoveToState(GameState.GameOver);
    }

    private void OnHeal(float current_life, float last_update)
    {
        Debug.Log($"+{last_update}hp => {current_life}", this);
    }

    private void OnTakeDamage(float current_life, float last_update)
    {
        Debug.Log($"-{last_update}hp => {current_life}", this);
    }

    private void OnManaRestore(float current_mana, float last_update)
    {
        Debug.Log($"+{last_update}mp => {current_mana}", this);
    }

    private void OnManaConsume(float current_mana, float last_update)
    {
        Debug.Log($"-{last_update}mp => {current_mana}", this);
    }

    public void visitNextLevel()
    {
        roomLevel++;
        graph = new Graph(levelHeight);
        life.maxLife += 50;
        life.Heal(1f);
        mana.maxMana += 50;
        mana.Restore(1f);

    }

    public string getFormattedName(string u = "•")
    {
        return getFormattedText(pName, u);
    }
    public string getFormattedText(string s, string u = "•")
    {
        string text = "";
        foreach (var c in s.ToLower())
        {
            if (c >= 'a' && c <= 'z' && !Player.instance.unlockedLetters.Contains(c))
            {
                text += u;
            }
            else
            {
                text += c.ToString().ToUpper();
            }
        }
        return text;
    }
}

