using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyBoard : MonoBehaviour
{
    public Dictionary<char, Image> Keys;
    public Color validColor = new Color32(150, 200, 255, 255);
    public Color invalidColor = new Color32(255, 120, 120, 255);
    public Color getColor = new Color32(220, 220, 220, 255);
    public Color baseColor = new Color32(200, 200, 200, 100);

    public void Initialize()
    {
        Keys = new Dictionary<char, Image>();
        
        for(char c = 'A'; c <= 'Z'; c++)
        {
            var key = GameObject.Find(c.ToString()).GetComponent<Image>();
            key.color = Player.instance.unlockedLetters.Contains(char.ToLower(c)) ? getColor : baseColor;
            Keys.Add(char.ToLower(c), key);
        }
    }

    public void UpdateDisplay()
    {
        foreach (var l in Keys)
        {
            List<char> unlockedLetters = CombatManager.instance != null ? CombatManager.instance.unlockedLetters : Player.instance.unlockedLetters;
            l.Value.color = unlockedLetters.Contains(l.Key) ? getColor : baseColor;
        }
    }
}
