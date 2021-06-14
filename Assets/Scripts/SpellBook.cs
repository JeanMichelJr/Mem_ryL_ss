using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpellBook : MonoBehaviour
{
    public GameObject spellItem;

    public GameObject spellContainer;
    public Text SpellName;
    public Text SpellDesc;
    public Text SpellCost;

    private List<GameObject> items = new List<GameObject>();

    private void Awake()
    {
        UpdateSpellList();
    }

    public void UpdateSpellList()
    {
        foreach(var go in items)
        {
            Destroy(go);
        }
        items.Clear();

        int index = 0;
        foreach(var spell in Player.instance.spells)
        {
            var go = Instantiate(spellItem, transform);
            go.transform.localPosition = new Vector2(15, -30 - index*15);
            go.GetComponent<MenuItem>().setWord(Player.instance.getFormattedText(spell));
            items.Add(go);
            index++;
        }

        MenuManager.instance.reloadMenuItems();
    }

    public void Display(string spell)
    {
        if (!Player.instance.spells.Contains(spell.ToLower()))
        {
            return;
        }

        Spell s = SpellManager.instance.spellWarehouse[spell.ToLower()];
        SpellName.text = s.displyableName;
        SpellDesc.text = s.description;
        SpellCost.text = s.manaCost.ToString();

        if (!spellContainer.activeSelf)
        {
            spellContainer.SetActive(true);
        }

        if (InputManager.instance != null)
        {
            InputManager.instance.onPressEnter += CloseSpellBook;
            InputManager.instance.blockTyping = true;
        }
    }

    public void CloseSpellBook()
    {
        spellContainer.SetActive(false);

        if (InputManager.instance != null)
        {
            InputManager.instance.onPressEnter -= CloseSpellBook;
            InputManager.instance.blockTyping = false;
        }
    }

    public void OnDestroy()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.onPressEnter -= CloseSpellBook;
            InputManager.instance.blockTyping = false;
        }
    }
}
