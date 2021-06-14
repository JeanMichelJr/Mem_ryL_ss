using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    public static EventManager instance = null;

    public MapDisplayer mapD;

    public GameObject EventMenu;
    public Text Title;
    public Text Desc;
    public GameObject Choices;
    public Text Result;
    public List<Event> Events;
    public Event FinalEvent;

    public Dictionary<int, EventItem> NumberItems;
    public List<String> chosenSpells { get; set; }

    private EventState currentState;

    public enum EventState
    {
        Close,
        Title,
        Desc,
        Choice,
        Conclusion
    }

    public enum EventType
    {
        Void,
        Heal,
        Letter,
        Spell,
        NumberSelect,
        Random,
        PlayerName
    }

    [Serializable]
    public class Event
    {
        public string Title;
        public string Desc;
        public GameObject Choices;
    }

    public class EventResult
    {
        public string conclusion = "";
        public string result = "";
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

        NumberItems = new Dictionary<int, EventItem>();
        chosenSpells = new List<string>();
    }

    public void TriggerNewEvent(bool final = false)
    {
        NumberItems.Clear();
        chosenSpells.Clear();

        Event ev;
        if (final)
        {
            ev = FinalEvent;
        }
        else
        {
            ev = Events.TakeNFirstRnd(1, x=>x).First();
        }

        EventMenu.SetActive(true);

        string desc = "";
        var list = ev.Desc.Split('|');
        foreach (var m in list)
        {
            desc += m;
            if(desc != list.LastOrDefault())
            {
                desc += "\n\n";
            }
        }
        desc.Replace("|", "");
        Desc.text = desc;

        Title.text = ev.Title;
        Instantiate(ev.Choices, Choices.transform);
        Desc.gameObject.SetActive(true);

        MenuManager.instance.reloadMenuItems();

        if (InputManager.instance != null)
        {
            InputManager.instance.onPressNumber += ChoseNumber;
            InputManager.instance.onPressEnter += OnPressEnter;
        }

        currentState = EventState.Desc;
    }

    public void NextState(EventResult res)
    {
        switch (currentState)
        {
            case EventState.Desc:
                Desc.gameObject.SetActive(false);
                Choices.SetActive(true);
                currentState = EventState.Choice;
                InputManager.instance.onPressEnter -= OnPressEnter;
                break;
            case EventState.Choice:
                foreach(Transform child in Choices.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
                Choices.SetActive(false);

                Result.text = res.result + "\n\n<i>" + res.conclusion + "</i>";
                Result.gameObject.SetActive(true);

                UIManager.instance.UpdateKeyBoard();
                UIManager.instance.UpdateName();
                UIManager.instance.UpdateSpellBook();

                currentState = EventState.Conclusion;
                InputManager.instance.onPressEnter += OnPressEnter;
                break;
            case EventState.Conclusion:
                Result.gameObject.SetActive(false);
                EventMenu.SetActive(false);
                currentState = EventState.Close;
                if (InputManager.instance != null)
                {
                    InputManager.instance.onPressNumber -= ChoseNumber;
                    InputManager.instance.onPressEnter -= OnPressEnter;
                }
                mapD.EventMenuClosed();
                break;
        }

        MenuManager.instance.reloadMenuItems();
    }

    public void OnPressEnter()
    {
        if(currentState == EventState.Desc || currentState == EventState.Conclusion)
        {
            NextState(null);
        }
    }

    private void ChoseNumber(int k)
    {
        if (currentState == EventState.Choice && NumberItems.ContainsKey(k))
        {
            NumberItems[k].Select();
        }
    }   

    private void OnDestroy()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.onPressNumber -= ChoseNumber;
            InputManager.instance.onPressEnter -= OnPressEnter;
        }
    }
}