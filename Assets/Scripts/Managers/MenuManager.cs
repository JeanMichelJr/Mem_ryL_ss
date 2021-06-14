using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    private List<MenuItem> currentItems = null;
    private MenuState menuState = MenuState.Initialization;
    public GameObject MainMenu;

    private List<MenuItem> MainMenuItems;

    public static MenuManager instance = null;

    private enum MenuState
    {
        Initialization,
        MainMenu,
        End
    }

    public enum ItemType
    {
        Play,
        Quit,
        Restart,
        None,
        Event,
        Spell,
        Credits
    }


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
    }

    private void Start()
    {
        menuState = MenuState.Initialization;
        currentItems = new List<MenuItem>();

        MainMenuItems = MainMenu.GetComponentsInChildren<MenuItem>().ToList();

        // Set user inputs
        if (InputManager.instance != null)
        {
            InputManager.instance.onPressLetter += OnPressLetter;
            InputManager.instance.onPressBack += OnPressBack;
        }

        menuState = MenuState.MainMenu;
    }

    private void OnPressLetter(char c)
    {
        if (currentItems.Count == 0 || !currentItems.Any(x => x.testLetter(c)))
        {
            List<MenuItem> newItems = new List<MenuItem>();

            switch (menuState)
            {
                case MenuState.MainMenu:
                    newItems = MainMenuItems.Where(i => i.word?.Length > 0 && i.word?.ToLower()[0] == c).ToList();
                    break;
                default:
                    break;
            }

            if(newItems.Count > 0)
            {
                if (newItems.Any(x => !currentItems.Contains(x)))
                {
                    currentItems.ForEach(x => x.ResetDisplay());
                    currentItems.Clear();
                }

                currentItems = newItems;
            }
        }

        if (currentItems.Count == 0)
            return;

        var itemsToRemove = new List<MenuItem>();
        bool itemUpdated = false;
        foreach (var item in currentItems)
        {
            if (item.testLetter(c))
            {
                item.UpdateDisplay(c);
                itemUpdated = true;
            }
            else
            {
                itemsToRemove.Add(item);
            }
        }

        if (itemUpdated)
            itemsToRemove.ForEach(x => { currentItems.Remove(x); x.ResetDisplay(); });

        if(currentItems.Any(x => x.Validate()))
        {
            ValidateItem();
        }
    }

    private void ValidateItem()
    {
        var currentItem = currentItems.FirstOrDefault(x => x.Validate());

        if (currentItem != null)
        {
            switch (menuState)
            {
                case MenuState.MainMenu:
                    switch (currentItem.type)
                    {
                        case ItemType.Play:
                            GameController.instance.MoveToState(GameState.Navigation);
                            menuState = MenuState.End;
                            break;
                        case ItemType.Restart:
                            GameController.instance.MoveToState(GameState.HomeMenu);
                            menuState = MenuState.End;
                            break;
                        case ItemType.Quit:
                            #if UNITY_EDITOR
                            UnityEditor.EditorApplication.isPlaying = false;
                            #else
                            Application.Quit();
                            #endif
                            break;
                        case ItemType.Event:
                            if(currentItem is EventItem)
                                ((EventItem)currentItem).Select();
                            break;
                        case ItemType.Spell:
                            UIManager.instance.DisplaySpellBook(currentItem.word);
                            break;
                        case ItemType.Credits:
                            GameController.instance.MoveToState(GameState.Credits);
                            menuState = MenuState.End;
                            break;
                        default:
                            break;
                    }
                    break;
                default:
                    break;
            }
            currentItem.ResetDisplay();
            currentItems.Clear();
        }

        currentItem = null;
    }

    private void OnPressBack()
    {
        currentItems.ForEach(x => x.ResetDisplay());
        currentItems.Clear();
    }

    public void reloadMenuItems()
    {
        if(MainMenu != null)
        {
            MainMenuItems = MainMenu.GetComponentsInChildren<MenuItem>().ToList();
            currentItems.ForEach(x => x.ResetDisplay());
            currentItems.Clear();
        }
    }

    private void OnDestroy()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.onPressLetter -= OnPressLetter;
            InputManager.instance.onPressBack -= OnPressBack;
        }
    }
}
