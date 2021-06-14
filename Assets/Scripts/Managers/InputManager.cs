using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputManager : MonoBehaviour
{
    public static InputManager instance = null;

    public event Action onPressEnter;
    public event Action onPressBack;
    public event Action<char> onPressLetter;
    public event Action<char> onReleaseLetter;
    public event Action<int> onPressNumber;
    public event Action<int> onUIVertical;

    public bool blockTyping;

    private HashSet<KeyControl> pressedKeys = new HashSet<KeyControl>();
    private Dictionary<KeyControl, char> pressedLetters = new Dictionary<KeyControl, char>();

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
        Keyboard.current.onTextInput += OnTextInput;
    }

    private void OnDisable()
    {
        Keyboard.current.onTextInput -= OnTextInput;
    }

    private void Update()
    {
        var result = GetDigitKey();
        if (result.Item1)
        {
            if (!blockTyping && onPressNumber != null)
            {
                onPressNumber.Invoke(result.Item2);
            }
        }

        if (Keyboard.current.backspaceKey.wasPressedThisFrame)
        {
            if (onPressBack != null)
            {
                onPressBack.Invoke();
            }
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame)
        {
            if (onPressEnter != null)
            {
                onPressEnter.Invoke();
            }
        }

        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.sKey.wasPressedThisFrame)
        {
            if (!blockTyping && onUIVertical != null)
            {
                if (Keyboard.current.sKey.IsPressed() && Keyboard.current.wKey.IsPressed())
                {
                    onUIVertical.Invoke(0);
                }
                else if (Keyboard.current.sKey.IsPressed())
                {
                    onUIVertical.Invoke(-1);
                }
                else
                {
                    onUIVertical.Invoke(1);
                }
            }
        }
        else if (Keyboard.current.wKey.wasReleasedThisFrame || Keyboard.current.sKey.wasReleasedThisFrame)
        {
            if (!blockTyping && onUIVertical != null)
            {
                if (Keyboard.current.wKey.IsPressed())
                {
                    onUIVertical.Invoke(1);
                }
                else if (Keyboard.current.sKey.IsPressed())
                {
                    onUIVertical.Invoke(-1);
                }
                else
                {
                    onUIVertical.Invoke(0);
                }
            }
        }

        if(!blockTyping && onReleaseLetter != null)
        {
            var releasedLetters = pressedLetters.Where(x => x.Key.wasReleasedThisFrame).Select(x => x.Value);
            foreach (var c in releasedLetters)
            {
                onReleaseLetter.Invoke(c);
            }
        }
        pressedLetters = pressedLetters.Where(x => !x.Key.wasReleasedThisFrame).ToDictionary(x => x.Key, x => x.Value);
        pressedKeys.RemoveWhere(x => x.wasReleasedThisFrame);
    }

    private Tuple<bool, int> GetDigitKey()
    {
        var keyboard = Keyboard.current;
        var digit = -1;
        if (keyboard.digit1Key.wasPressedThisFrame)
        {
            digit = 1;
        }
        else if (keyboard.digit2Key.wasPressedThisFrame)
        {
            digit = 2;
        }
        else if (keyboard.digit3Key.wasPressedThisFrame)
        {
            digit = 3;
        }
        else if (keyboard.digit4Key.wasPressedThisFrame)
        {
            digit = 4;
        }
        else if (keyboard.digit5Key.wasPressedThisFrame)
        {
            digit = 5;
        }
        else if (keyboard.digit6Key.wasPressedThisFrame)
        {
            digit = 6;
        }
        else if (keyboard.digit7Key.wasPressedThisFrame)
        {
            digit = 7;
        }
        else if (keyboard.digit8Key.wasPressedThisFrame)
        {
            digit = 8;
        }
        else if (keyboard.digit9Key.wasPressedThisFrame)
        {
            digit = 9;
        }
        else if (keyboard.digit0Key.wasPressedThisFrame)
        {
            digit = 0;
        }

        return Tuple.Create(digit != -1, digit);
    }

    private void OnTextInput(char c)
    {
        c = char.ToLower(c);

        if (c < 'a' || c > 'z')
            return;
        if (!pressedKeys.Add(Keyboard.current.FindKeyOnCurrentKeyboardLayout(c.ToString())))
            return;

        pressedLetters[Keyboard.current.FindKeyOnCurrentKeyboardLayout(c.ToString())] = c;

        if (!blockTyping && onPressLetter != null)
        {
            onPressLetter.Invoke(c);
        }
    }
}
