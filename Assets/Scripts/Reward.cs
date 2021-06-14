using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Reward : MonoBehaviour
{
    public int nRewards = 3;

    private List<char> allLetters = Enumerable.Range(0, 26).Select( i => (char)('a' + i)).ToList();
    private List<char> rewards = new List<char>();
    public GameObject RewardMenu;
    public List<Text> keysMenu = new List<Text>();

    public void OnEnable()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.onPressLetter += OnPressLetter;
            InputManager.instance.onPressEnter += Heal;
        }
    }

    public void GetReward()
    {
        rewards = allLetters.TakeNFirstRnd(nRewards, c => c, Player.instance.unlockedLetters);

        var sb = new StringBuilder();

        for (int i = 0; i < rewards.Count; i++)
        {
            char ul = rewards[i];
            keysMenu[i].text = ul.ToString().ToUpper();
            sb.AppendLine($"{i+1} : {ul}");

        }
        Debug.Log(sb.ToString());
        RewardMenu.SetActive(true);
    }

    public void Heal()
    {
        Player.instance.life.Heal(0.15f);
        Player.instance.mana.Restore(0.15f);
        GameController.instance.MoveToState(GameState.Navigation);
    }

    private void OnPressLetter(char c)
    {
        if (!rewards.Contains(c))
            return;

        var unlocked = c;

        var sb = new StringBuilder();
        sb.AppendLine($"Unlocked {unlocked}");

        Player.instance.unlockedLetters.Add(unlocked);
        sb.Append($"Player : \n\t");

        foreach (var ul in Player.instance.unlockedLetters)
        {
            sb.Append($"{ul},");
        }
        Debug.Log(sb.ToString());
        InputManager.instance.onPressLetter -= OnPressLetter;
        InputManager.instance.onPressEnter -= Heal;
        GameController.instance.MoveToState(GameState.Navigation);
    }

    private void OnDisable()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.onPressLetter -= OnPressLetter;            InputManager.instance.onPressEnter -= Heal;
        }
        if (RewardMenu != null)
            RewardMenu.SetActive(false);
        rewards.Clear();
    }
    private void OnDestroy()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.onPressLetter -= OnPressLetter;            InputManager.instance.onPressEnter -= Heal;
        }
    }   
}