using UnityEngine;
using UnityEngine.UI;

public class MenuItem : MonoBehaviour
{
    public string word { get; private set; }
    public int nextLetterIndex { get; private set; } = 0;
    public MenuManager.ItemType type;
    protected Text textUI;

    public void Awake()
    {
        textUI = this.GetComponent<Text>();
        BeforeAwake();
        word = textUI.text;
        nextLetterIndex = 0;
    }

    public void ResetDisplay()
    {
        textUI.text = word;
        nextLetterIndex = 0;
    }

    public virtual bool testLetter(char c)
    {
        return word.Length != 0 && nextLetterIndex < word.Length && c == word.ToLower()[nextLetterIndex];
    }

    public void UpdateDisplay(char letter)
    {
        if (testLetter(letter))
        {
            nextLetterIndex++;
            while(nextLetterIndex < word.Length && word[nextLetterIndex] == ' ')
            {
                nextLetterIndex++;
            }

            var result = word.Slice(nextLetterIndex);
            if (Validate())
            {
                textUI.text = $"<color=#aa0000><i>{result.Item1}</i></color>";
            }
            else
            {
                textUI.text = $"<color=#00AAff>{result.Item1}</color>{result.Item2}";
            }
            SoundManager.instance.PlayKeyHit(letter);
        }
    }

    public bool Validate()
    {
        if (nextLetterIndex >= word.Length)
        {
            return true;
        }

        return false;
    }

    public void setWord(string w)
    {
        word = w;
        ResetDisplay();
    }

    protected virtual void BeforeAwake() { }
}
