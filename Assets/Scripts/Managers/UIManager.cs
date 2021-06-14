using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject SpellIcons;
    public GameObject Health;
    public GameObject Mana;
    public Text RoomNumber;
    public Text LevelNumber;
    public Text Name;
    public KeyBoard Letters;
    public SpellBook SpellBook;

    private List<char> unlockedLetters;

    public Transform HealthBar;
    private Text HealthAmount;
    private Animator HealthBarAnimator;

    public Transform ManaBar;
    private Text ManaAmount;
    private Animator ManaBarAnimator;

    public static UIManager instance = null;

    public void Awake()
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

        var player = Player.instance;
        unlockedLetters = player.unlockedLetters;

        if (Health != null)
        {
            HealthAmount = Health.GetComponentInChildren<Text>();
            HealthBarAnimator = Health.GetComponent<Animator>();

            player.life.onHeal += HealLifeBar;
            player.life.onTakeDamage += DamageLifeBar;
            UpdateLifeBar(Player.instance.life.currentLife, 0);
        }

        if(Mana != null)
        {
            ManaAmount = Mana.GetComponentInChildren<Text>();
            ManaBarAnimator = Mana.GetComponent<Animator>();

            player.mana.onConsume += DamageManaBar;
            player.mana.onRestore += HealManaBar;
            UpdateManaBar(Player.instance.mana.currentMana, 0);
        }

        if (Letters != null)
        {
            Letters.Initialize();
        }

        if(RoomNumber != null)
        {
            RoomNumber.text = Player.instance.graph.visitedRooms.Count.ToString();
        }

        if(LevelNumber != null)
        {
            LevelNumber.text = (Player.instance.roomLevel + 1).ToString();
        }

        if (Name != null)
        {
            UpdateName();
        }

        // Set user inputs
        if (InputManager.instance != null)
        {
            InputManager.instance.onPressLetter += PressKeyboardKey;
            InputManager.instance.onReleaseLetter += ReleaseKeyboardKey;
        }
    }

    //Player Bars
    private void HealLifeBar(float current_life, float last_update)
    {
        UpdateLifeBar(current_life, last_update);
    }

    private void DamageLifeBar(float current_life, float last_update)
    {
        UpdateLifeBar(current_life, last_update);
        HealthBarAnimator.Play("Damage");
    }

    private void UpdateLifeBar(float current_life, float last_update)
    {
        var max = Player.instance.life.maxLife;

        HealthAmount.text = current_life.ToString();
        HealthBar.localPosition = new Vector3(HealthBar.localPosition.x, current_life / max - 1, HealthBar.localPosition.z);
    }


    private void HealManaBar(float current_life, float last_update)
    {
        UpdateManaBar(current_life, last_update);
    }

    private void DamageManaBar(float current_life, float last_update)
    {
        UpdateManaBar(current_life, last_update);
        ManaBarAnimator.Play("Damage");
    }

    private void UpdateManaBar(float current_mana, float last_update)
    {
        var max = Player.instance.mana.maxMana;
        ManaAmount.text = current_mana.ToString();
        ManaBar.localPosition = new Vector3(ManaBar.localPosition.x, current_mana / max - 1, ManaBar.localPosition.z);
    }


    //Methodes pour la gestion du Keyboard
    public void PressKeyboardKey(char c)
    {
        Color baseColor;
        Color destColor;
        if (unlockedLetters.Contains(c))
        {
            baseColor = Letters.getColor;
            destColor = Letters.validColor;
        }
        else
        {
            baseColor = Letters.baseColor;
            destColor = Letters.invalidColor;
        }
        StartCoroutine(colorTween(Letters.Keys[char.ToLower(c)], baseColor, destColor, 0.1f));
    }

    public void ReleaseKeyboardKey(char c)
    {
        var baseColor = unlockedLetters.Contains(c) ? Letters.validColor : Letters.invalidColor;
        var destColor = unlockedLetters.Contains(c) ? Letters.getColor : Letters.baseColor;
        StartCoroutine(colorTween(Letters.Keys[char.ToLower(c)], baseColor, destColor, 0.1f));
    }

    public void UpdateKeyBoard()
    {
        Letters.UpdateDisplay();
    }

    public void UpdateName()
    {
        Name.text = Player.instance.getFormattedName("_");
    }

    public void UpdateSpellBook()
    {
        if (SpellBook == null)
        {
            return;
        }
        SpellBook.UpdateSpellList();
    }

    public void DisplaySpellBook(string spell)
    {
        if(SpellBook == null)
        {
            return;
        }
        SpellBook.Display(spell);
    }


    private IEnumerator colorTween(Image key, Color colorInit, Color color, float timeToTween)
    {
        float timeLeft = timeToTween;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            key.color = Color.Lerp(colorInit, color, 1 - timeLeft / timeToTween);
            yield return null;
        }
        key.color = color;
    }

    private void OnDestroy()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.onPressLetter -= PressKeyboardKey;
            InputManager.instance.onReleaseLetter -= ReleaseKeyboardKey;
        }

        var player = Player.instance;

        player.life.onHeal -= HealLifeBar;
        player.life.onTakeDamage -= DamageLifeBar;

        player.mana.onConsume -= DamageManaBar;
        player.mana.onRestore -= HealManaBar;

    }

}
