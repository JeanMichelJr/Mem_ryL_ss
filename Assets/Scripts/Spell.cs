using System;
using System.Collections.Generic;
using UnityEngine;

public class Spell
{
    public enum AnimType
    {
        None,
        Target,
        TargettedZone,
        Zone,
        Life,
        Mana,
        KeyBoard
    }

    public string displyableName = string.Empty;
    public bool requireTarget = true;
    public int manaCost = 5;
    public Action<Enemy, IEnumerable<Enemy>> body = null; 
    public string description;
    public AnimType anim;
    public Color color = Color.red;
}
