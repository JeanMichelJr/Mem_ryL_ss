using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    private class InternalEnemy
    {
        public Enemy enemy;
        public string word;

        public InternalEnemy(Enemy e)
        {
            enemy = e;
            SetWord();
        }

        public void SetWord()
        {
            var picked_letters = new HashSet<char>();
            var mapping = new Dictionary<char, char>();
            foreach (var c in enemy.pattern.Distinct())
            {
                char new_char;

                do
                {
                    var i = UnityEngine.Random.Range(0, CombatManager.instance.unlockedLetters.Count);
                    new_char = CombatManager.instance.unlockedLetters[i];
                }
                while (!picked_letters.Add(new_char));

                mapping[c] = new_char;
            }

            word = new string(enemy.pattern.Select(c => mapping[c]).ToArray());
        }
    }
    private enum HeroState
    {
        Initialization,
        WaitingBeforeActions,
        WritingSpell,
        SelectingAfterSpell,
        SelectingPattern,
        TypingWord,
        End
    }

    private class CombatStateMachine : AbstractStateMachine<HeroState>
    {
        public CombatStateMachine() : base()
        {}

        public CombatStateMachine(HeroState value) : base(value)
        {}
        public override bool CheckTransition(HeroState next_state)
        {
            if (next_state == HeroState.End)
            {
                return true;
            }

            if (currentState == HeroState.End)
            {
                return false;
            }

            if (next_state == HeroState.WaitingBeforeActions)
            {
                return true;
            }

            switch (currentState)
            {
                case HeroState.WaitingBeforeActions:
                    return next_state == HeroState.WritingSpell
                        || next_state == HeroState.SelectingPattern;
                case HeroState.WritingSpell:
                    return next_state == HeroState.WritingSpell
                        || next_state == HeroState.SelectingAfterSpell
                        || next_state == HeroState.TypingWord;
                case HeroState.SelectingPattern:
                    return next_state == HeroState.SelectingPattern
                        || next_state == HeroState.TypingWord;
                case HeroState.TypingWord:
                    return next_state == HeroState.TypingWord;
            }

            return false;
        }
    }

    public GameObject enemySet;
    public const int maxEnemiesPerRoom = 5;
    public Text text;
    public Text tuto;
    public Text sequence;
    public GameObject spellBook;
    public Text rewardTuto;
    public static CombatManager instance = null;
    public GameObject enemyHUD;
    public Canvas canvas;
    public Reward reward;

    public List<char> unlockedLetters = new List<char>();
    private List<InternalEnemy> internalEnemies = new List<InternalEnemy>();
    private List<Enemy> _allEnemies = null;
    private List<Enemy> allEnemies
    {
        get
        {
            if (_allEnemies == null)
            {
                _allEnemies = internalEnemies.Select( ie => ie.enemy).ToList();
            }
            return _allEnemies;
        }
    }
    public int rprbCounter = 3;
    public float stunTime = 1f;
    public float chainBonus = .2f;
    private InternalEnemy currentTarget { get; set; } = null;
    private CombatStateMachine combatStateMachine = new CombatStateMachine();
    private int nextLetterIndex = 0;
    private string currentSpell = string.Empty;
    private Spell castingSpell;
    public bool isHeroSilenced { get; set; } = false;
    private bool EnemiesStarted { get; set; }
    private Queue<int> targetPattern = new Queue<int>();
    private float currentBonus = 0f;
    public void Initialization()
    {
        // Create foe (Instantiate) will be updated
        var roomType = Player.instance.graph.currentRoomType;
        List<Enemy> enemies;
        if (roomType == Graph.RoomType.Boss)
        {
            enemies = generateBossRoom(Player.instance.roomLevel);
        }
        else
        {
            var potentialEnemies = EnemyManager.instance.enemyLibrary.ToList();

            int x = Player.instance.levelHeight * Player.instance.roomLevel + Player.instance.graph.visitedRooms.Count() - 1;
            int fx = Mathf.RoundToInt(Mathf.Sqrt(x) * Mathf.Log(2 * x));
            int roomLevel = EnemyManager.instance.minLevel + (fx > 0 ? fx : 0);
            int maxEnemyLevel = Mathf.RoundToInt((Player.instance.roomLevel + 1) / 3f * (EnemyManager.instance.maxLevel - EnemyManager.instance.minLevel)) 
                                    + EnemyManager.instance.minLevel;

            enemies = generateRoom(potentialEnemies, roomLevel, maxEnemyLevel);
        }

        isHeroSilenced = false;

        if (Player.instance != null)
        {
            unlockedLetters = Player.instance.unlockedLetters.ConvertAll(c => c);
        }
        foreach (var e in enemies)
        {
            internalEnemies.Add(new InternalEnemy(e));
            e.life.onDeath += OnEnemyDeath;
        }
        generateHUD();

        Debug.Log($"You will face {enemies.Count} foes", this);

        // Set user inputs
        if (InputManager.instance != null)
        {
            InputManager.instance.onPressNumber += OnPressNumber;
            InputManager.instance.onPressLetter += OnPressLetter;
            InputManager.instance.onPressEnter += OnPressEnter;
            InputManager.instance.onPressBack += OnPressBack;
        }

        if (Player.instance.graph.currentRoom.Item1 == 0 && Player.instance.roomLevel == 0)
        {
            tuto.gameObject.SetActive(true);
        }
        else
        {
            tuto.gameObject.SetActive(false);
        }

        combatStateMachine.onStateChange += OnCombatStateChange;
    }

    private List<Enemy> generateRoom(List<Enemy> potentialEnemies, int roomLevel, int maxEnemyLevel)
    {
        List<Enemy> result = new List<Enemy>();
        IEnumerable<Enemy> filteredEnemies;
        while (roomLevel > 0 
                && result.Count < maxEnemiesPerRoom
                && (filteredEnemies = potentialEnemies.Where( e => e.pattern.Length <= roomLevel && e.pattern.Length <= maxEnemyLevel)).Count() > 0)
        {
            var sEnemies = filteredEnemies.ToList().Shuffle(e => e);

            var enemy = sEnemies[0];
            var enemyToAdd = GameObject.Instantiate(enemy, transform);
            result.Add(enemyToAdd);

            roomLevel -= enemy.pattern.Length;
        }

        int numEnemy = 1;
        float enemyCenter = (result.Count + 1) / 2f;
        foreach (Enemy enemy in result)
        {
            enemy.transform.localPosition += Vector3.left * 3.5f * (enemyCenter - numEnemy);
            numEnemy++;
        }

        return result;
    }

    private void OnCombatStateChange(HeroState state)
    {
        switch (state)
        {
            case HeroState.End:
                SoundManager.instance.PlayMenuMusic();
                sequence.text = "";
                if (Player.instance.graph.currentRoom.Item1 == 0 && Player.instance.roomLevel == 0)
                {
                    rewardTuto.gameObject.SetActive(true);
                }
                if (Player.instance.graph.currentRoomType == Graph.RoomType.Boss
                    && Player.instance.roomLevel == Player.instance.finalRoomLevel)
                {
                    GameController.instance.MoveToState(GameState.Victory);
                }
                reward.gameObject.SetActive(true);
                reward.GetReward();
                if (Player.instance != null)
                {
                    unlockedLetters = Player.instance.unlockedLetters.ConvertAll(c => c);
                }
                break;
            case HeroState.WaitingBeforeActions:
                if (combatStateMachine.currentState == HeroState.WaitingBeforeActions)
                {
                    currentTarget = null;
                }
                targetPattern.Clear();
                currentBonus = 0f;
                break;
        }
    }

    private void StartEnemies()
    {
        spellBook?.SetActive(false);
        EnemiesStarted = true;
        SoundManager.instance.PlayBattleMusic();
        foreach (var enemy in internalEnemies.Select(x => x.enemy))
        {
            enemy.CombatStarted = true;
        }
    }

    private List<Enemy> generateBossRoom(int bossLvl)
    {
        var boss = GameObject.Instantiate(EnemyManager.instance.bossLibrary[bossLvl], transform);
        return boss.GetComponentsInChildren<Enemy>().ToList();
    }

    private void generateHUD()
    {
        int index = 1;
        foreach (var enemy in internalEnemies.Select(x => x.enemy))
        {
            var hud = Instantiate(enemyHUD, canvas.transform);
            hud.transform.position = enemy.hudPos.transform.position;
            enemy.setHUD(hud.GetComponent<EnemyHUD>(), index);
            index++;
        }
    }

    private void OnPressBack()
    {
        if (combatStateMachine.currentState == HeroState.TypingWord)
        {
            return;
        }

        combatStateMachine.MoveTo(HeroState.WaitingBeforeActions);
        if (currentTarget != null)
        {
            currentTarget.enemy.Unselect();
            currentTarget = null;
        }
    }

    private void OnPressNumber(int k)
    {
        if (!EnemiesStarted)
        {
            StartEnemies();
        }

        switch (combatStateMachine.currentState)
        {
            case HeroState.SelectingAfterSpell:
                SelectAndCast(k);
                return;
            case HeroState.WaitingBeforeActions:
            case HeroState.SelectingPattern:
            case HeroState.WritingSpell:
                AddToPattern(k);
                break;
        }
    }

    private void AddToPattern(int k)
    {
        combatStateMachine.MoveTo(HeroState.SelectingPattern);
        switch (combatStateMachine.currentState)
        {
            case HeroState.Initialization:
            case HeroState.End:
                return;
            case HeroState.WaitingBeforeActions:
            case HeroState.SelectingPattern:
                currentTarget = null;
                break;
        }
        
        if (k > internalEnemies.Count)
        {
            Debug.Log($"{k} > {internalEnemies.Count}", this);
            return;
        }

        var enemy = internalEnemies[k - 1];
        if (!enemy.enemy.life.alive)
        {
            Debug.Log("You can't select a dead monster");
            return;
        }

        targetPattern.Enqueue(k);
    }

    private void OnPressEnter()
    {
        if (!EnemiesStarted)
        {
            StartEnemies();
        }

        switch (combatStateMachine.currentState)
        {
            case HeroState.SelectingPattern:
                combatStateMachine.MoveTo(HeroState.TypingWord);
                SelectMonster(targetPattern.Dequeue());
                break;
            case HeroState.WritingSpell:
                ValidSpell();
                break;
            default:
                return;
        }
    }

    private void OnPressLetter(char c)
    {
        if (!EnemiesStarted)
        {
            StartEnemies();
        }

        switch (combatStateMachine.currentState)
        {
            case HeroState.TypingWord:
                TypeMonsterWord(c);
                break;
            case HeroState.WaitingBeforeActions:
                currentSpell = string.Empty;
                castingSpell = null;
                TypeSpellWord(c);
                break;
            case HeroState.WritingSpell:
                TypeSpellWord(c);
                break;
            default:
                return;
        }
    }

    // State Utilities
    private void DamageEnemy(int k = -1)
    {
        int x = unlockedLetters.Count;
        float dmg = k != -1 
                        ? k 
                        : Mathf.Log(x) + Mathf.Sqrt(x) + x + 5;

        currentTarget.enemy.Stun(stunTime);
        currentTarget.enemy.life.Damage(Mathf.RoundToInt(dmg * (1 + currentBonus)));

        if (currentTarget != null && currentTarget.enemy.life.alive)
        {
            SoundManager.instance.PlayOneShot("enemyDamage");
            currentTarget.enemy.Unselect();
        }
    }

    private void OnEnemyDeath()
    {
        foreach (var dead in internalEnemies.Where( ie => !ie.enemy.life.alive && ie.enemy.isActiveAndEnabled))
        {
            if (dead == currentTarget)
            {
                currentTarget = null;
            }
            dead.enemy.life.onDeath -= OnEnemyDeath;
            dead.enemy.Disable();
        }

        SoundManager.instance.PlayOneShot("enemyDeath");

        if (internalEnemies.All(ie => !ie.enemy.life.alive))
        {
            combatStateMachine.MoveTo(HeroState.End);
        }

        if (targetPattern.Count > 0)
        {
            combatStateMachine.MoveTo(HeroState.TypingWord);
        }
        else
        {
            combatStateMachine.MoveTo(HeroState.WaitingBeforeActions);
        }
    }

    public bool isCombatOver()
    {
        return internalEnemies.All(ie => !ie.enemy.life.alive);
    }
    private void TypeMonsterWord(char letter)
    {
        if (currentTarget == null)
        {
            return;
        }
        
        if (!unlockedLetters.Contains(letter))
        {
            return;
        }

        if (nextLetterIndex >= currentTarget.word.Length)
        {
            return;
        }

        if (currentTarget.word[nextLetterIndex] == letter)
        {
            nextLetterIndex++;
            if (nextLetterIndex >= currentTarget.word.Length)
            {
                AnimAttackEnnemy(currentTarget.word);
                DamageEnemy();
                if (combatStateMachine.currentState == HeroState.End)
                {
                    return;
                }
                if (currentTarget != null)
                {
                    currentTarget.enemy.Unselect();
                    currentTarget = null;
                }
                while (targetPattern.Count > 0 && !SelectMonster(targetPattern.Dequeue()))
                {}
                if (currentTarget == null)
                {
                    combatStateMachine.MoveTo(HeroState.WaitingBeforeActions);
                }
                else
                {
                    currentBonus += chainBonus;
                }
            }
            else
            {
                combatStateMachine.MoveTo(HeroState.TypingWord);
            }
            SoundManager.instance.PlayKeyHit(letter);
        }
    }

    private void TypeSpellWord(char letter)
    {
        // Comment this for a debug purpose
        if (!unlockedLetters.Contains(letter))
        {
            return;
        }

        SoundManager.instance.PlayKeyHit(letter);

        combatStateMachine.MoveTo(HeroState.WritingSpell);

        currentSpell = currentSpell + letter;

        var pool = isHeroSilenced ? new List<string>() : Player.instance.spells.Where(s => s.StartsWith(currentSpell));

        var sb = new StringBuilder();
        foreach (var s in pool)
        {
            sb.AppendLine(s);
        }
        Debug.Log($"Available spells :\n{sb.ToString()}");
    }

    private bool SelectMonster(int k)
    {
        if (combatStateMachine.currentState == HeroState.Initialization || combatStateMachine.currentState == HeroState.End)
        {
            return false;
        }

        if (combatStateMachine.currentState == HeroState.WaitingBeforeActions)
        {
            currentTarget = null;
        }

        if (k > internalEnemies.Count)
        {
            Debug.Log($"{k} > {internalEnemies.Count}", this);
            return false;
        }

        sequence.text = "<color=cyan><size=20>" + k + "</size></color>";
        targetPattern.Where(x => internalEnemies[x-1].enemy.life.alive).ToList().ForEach(x => sequence.text += " " + x);

        var enemy = internalEnemies[k - 1];
        if (!enemy.enemy.life.alive)
        {
            Debug.Log("You can't select a dead monster");
            return false;
        }

        if(currentTarget != null)
        {
            currentTarget.enemy.Unselect();
        }
        currentTarget = enemy;
        currentTarget.enemy.Select();

        if (combatStateMachine.currentState == HeroState.TypingWord)
        {
            SetWord();
        }

        return true;
    }

    private void ValidSpell()
    {
        var spellIdx = Player.instance.spells.FindIndex(s => s.ToLower() == currentSpell);

        if (spellIdx < 0)
        {
            combatStateMachine.MoveTo(HeroState.WaitingBeforeActions);
            return;
        }
        
        if (SpellManager.instance.spellWarehouse.TryGetValue(Player.instance.spells[spellIdx], out castingSpell))
        {
            if (!castingSpell.requireTarget)
            {
                CastSpell(castingSpell);
            }
            else
            {
                if (castingSpell.requireTarget)
                {
                    combatStateMachine.MoveTo(HeroState.SelectingAfterSpell);
                }
                else
                {
                    CastSpell(castingSpell);
                }
            }
        }
    }

    public void CastSpell(Spell spell)
    {
        // TODO : Decrease spell related counters
        Player.instance.spellAmplificationLeft--;
        
        if (Player.instance.mana.currentMana < spell.manaCost)
        {
            combatStateMachine.MoveTo(HeroState.WaitingBeforeActions);
            return;
        }

        Player.instance.mana.Consume(spell.manaCost);
        AnimSpell(spell);

        if (!spell.requireTarget)
        {
            spell.body(null, allEnemies);
        }
        else
        {
            spell.body(currentTarget.enemy, allEnemies);   
        }
        
        if (Player.instance.spellAmplificationLeft <= 0)
        {
            Player.instance.spellAmplificationLeft = 0;
            Player.instance.spellAmplification = 1f;
        }
        

        if (combatStateMachine.currentState != HeroState.End)
        {
            combatStateMachine.MoveTo(HeroState.WaitingBeforeActions);
        }
    }
    private void SelectAndCast(int k)
    {
        currentTarget = null;
        if (!SelectMonster(k))
        {
            return;
        }

        CastSpell(castingSpell);
    }

    public Enemy getLowestHealthRatioEnemy()
    {
        Enemy lowestHealthEnemy = internalEnemies.First().enemy;
        foreach (InternalEnemy intEnemy in internalEnemies)
        {
            if ((lowestHealthEnemy.life.currentLife / lowestHealthEnemy.life.maxLife)
                    > (intEnemy.enemy.life.currentLife / intEnemy.enemy.life.maxLife))
            {
                lowestHealthEnemy = intEnemy.enemy;
            }
        }
        return lowestHealthEnemy;
    }

    public int addEnemy(Enemy e)
    {
        internalEnemies.Add(new InternalEnemy(e));
        return internalEnemies.Count();
    }

    public bool removeEnemy(Enemy e)
    {
        if(!internalEnemies.Any(x => x.enemy == e))
        {
            return false;
        }
        if(currentTarget != null && currentTarget.enemy == e)
        {
            OnPressBack();
        }
        internalEnemies.RemoveAll(x => x.enemy == e);
        return true;
    }

    // Utilities
    private void SetWord()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Unlocked letters :");
        foreach (var l in unlockedLetters)
        {
            sb.Append($"{l};");
        }
        Debug.Log(sb.ToString());

        currentTarget.SetWord();
        nextLetterIndex = 0;
    }

    public void StartCombat()
    {
        combatStateMachine.MoveTo(HeroState.WaitingBeforeActions);
        reward.gameObject.SetActive(false);
    }

    // Base methods
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

        Initialization();
        GameController.instance.SubscribeToGameReady(StartCombat);
    }

    public void LateUpdate()
    {
        string text_to_display = string.Empty;
        switch (combatStateMachine.currentState)
        {
            case HeroState.WaitingBeforeActions:
                sequence.text = "";
                text_to_display = "<color=cyan>Sélectionnez des monstres\nou lancez un sort</color>";
                if (tuto.gameObject.activeSelf)
                {
                    tuto.text = "Pour sélectionner un monstre, appuyez sur un chiffre.\nPour lancer un sort, écrivez le. (attention aux fautes !)\nAppuyez sur <color=#dcdcdc><b>←</b></color> pour annuler.";
                }
                break;
            case HeroState.WritingSpell:
                text_to_display = $"<color=#00AAff><b>{currentSpell.ToUpper()}</b></color>";
                if (tuto.gameObject.activeSelf)
                {
                    tuto.text = "Ecriver le nom du sort à lancer.\n(Vous devez connaître une lettre pour l'écrire)\nAppuyez sur <color=#dcdcdc><b>←</b></color> pour annuler et sur <color=#dcdcdc>ENTRÉE</color> pour valider.";
                }
                break;
            case HeroState.SelectingAfterSpell:
                text_to_display = $"<color=#aa0000><i>{currentSpell.ToUpper()}</i></color>";
                break;
            case HeroState.SelectingPattern:
                if (tuto.gameObject.activeSelf)
                {
                    tuto.text = "Tapez une séquence de chiffres pour choisir l'ordre de vos prochaines attaques.\n Vous ne pourrez pas annuler la séquence !\n(Appuyez sur <color=#dcdcdc>ENTRÉE</color> pour valider)";
                }
                var sb = new StringBuilder();
                foreach (var i in targetPattern)
                {
                    sb.Append(i);
                }
                text_to_display = $"<color=#00AAff><b>{sb.ToString()}</b></color>";
                break;
            default:
                if (currentTarget != null)
                {
                    var result = currentTarget.word.ToUpper().Slice(nextLetterIndex);
                    text_to_display = $"<color=#00AAff>{result.Item1}</color>{result.Item2}";
                    if (tuto.gameObject.activeSelf)
                    {
                        tuto.text = "(remplissez le modèle pour attaquer)\n(Vous pouvez annuler à tout moment avec <color=#dcdcdc><b>←</b></color>)";
                    }
                }
                break;
        }
        text.text = text_to_display;
    }

    private void OnDestroy()
    {
        if (InputManager.instance != null)
        {
            InputManager.instance.onPressNumber -= OnPressNumber;
            InputManager.instance.onPressLetter -= OnPressLetter;
            InputManager.instance.onPressEnter -= OnPressEnter;
            InputManager.instance.onPressBack -= OnPressBack;
        }

        foreach (var ie in internalEnemies)
        {
            ie.enemy.life.onDeath -= OnEnemyDeath;
        }

        GameController.instance.UnsubscribeToGameReady(StartCombat);
    }

    void AnimAttackEnnemy(string word)
    {
        string textR = word;
        var random = new System.Random();
        foreach (var letter in ConvertTypingToObject(Color.red, word))
        {
            StartCoroutine(MoveTo(letter.transform, currentTarget.enemy.transform.position, random.Next(5, 10) / 100f, random.Next(10, 100) / 100));
        }
    }

    void AnimSpell(Spell spell)
    {
        var random = new System.Random();
        var enemies = allEnemies.Where(x => x.life.alive).OrderBy(x => x.transform.position.x).Select(x => x.transform);
        var list = ConvertTypingToObject(spell.color, SpellManager.instance.spellWarehouse.First(x => x.Value == spell).Key);
        int nbLetters = list.Count;
        int index = 0;
        foreach (var letter in list)
        {
            switch (spell.anim)
            {
                case Spell.AnimType.Target:
                    StartCoroutine(MoveTo(letter.transform, currentTarget.enemy.transform.position, random.Next(5, 10) / 100f, random.Next(10, 100) / 100));
                    break;
                case Spell.AnimType.TargettedZone:
                    StartCoroutine(MoveTo(letter.transform, currentTarget.enemy.transform.position, random.Next(5, 10) / 100f, random.Next(10, 100) / 100, enemies.First(), enemies.Last()));
                    break;
                case Spell.AnimType.Zone:
                    var delta = enemies.Last().position.x - enemies.First().position.x;
                    StartCoroutine(MoveTo(letter.transform, enemies.First().position + new Vector3(delta*(index/(nbLetters-1)), 0, 0), random.Next(5, 10) / 100f, random.Next(10, 100) / 100, enemies.First(), enemies.Last()));
                    break;
                case Spell.AnimType.KeyBoard:
                    StartCoroutine(MoveTo(letter.transform, UIManager.instance.Letters.transform.position, random.Next(5, 10) / 100f, random.Next(10, 100) / 100));
                    break;
                case Spell.AnimType.Mana:
                    StartCoroutine(MoveTo(letter.transform, UIManager.instance.ManaBar.position, random.Next(5, 10) / 100f, random.Next(10, 100) / 100));
                    break;
                case Spell.AnimType.Life:
                    StartCoroutine(MoveTo(letter.transform, UIManager.instance.HealthBar.position, random.Next(5, 10) / 100f, random.Next(10, 100) / 100));
                    break;
                default:
                    Destroy(letter);
                    break;
            }
            index++;
        }
    }

    private List<Text> ConvertTypingToObject(Color color, string word)
    {
        List<Text> Letters = new List<Text>();
        string textR = word;
        TextGenerator textGen = new TextGenerator(textR.Length);
        Vector2 extents = text.gameObject.GetComponent<RectTransform>().rect.size;
        textGen.Populate(textR, text.GetGenerationSettings(extents));

        var random = new System.Random();

        int index = 1;
        foreach (char c in textR)
        {
            int indexOfTextQuad = (index * 4) - 4;
            if (indexOfTextQuad < textGen.vertexCount)
            {
                Vector3 avgPos = (textGen.verts[indexOfTextQuad].position +
                    textGen.verts[indexOfTextQuad + 1].position +
                    textGen.verts[indexOfTextQuad + 2].position +
                    textGen.verts[indexOfTextQuad + 3].position) / 4f;

                var letter = Instantiate(text, text.transform.parent.transform);
                var letterText = letter.GetComponent<Text>();
                letterText.text = "<i>" + c + "</i>";
                letterText.color = color;
                letter.transform.localPosition = new Vector3(avgPos.x, text.transform.localPosition.y);
                Letters.Add(letter);
            }
            else
            {
                Debug.LogError("Out of text bound");
            }
            index++;
        }

        return Letters;
    }

    private IEnumerator MoveTo(Transform trans, Vector3 dest, float time, float begin, Transform leftTarget = null, Transform rightTarget = null)
    {
        float timeLeft = time + begin;
        Vector3 initialPos = trans.position;
        while(timeLeft > time)
        {
            timeLeft -= Time.deltaTime;
            yield return null;
        }
        while (timeLeft > 0)
        {
            var x = Mathf.Lerp(initialPos.x, dest.x, 1 - (timeLeft / time));
            var y = Mathf.Lerp(initialPos.y, dest.y, 1 - (timeLeft / time));
            trans.SetPositionAndRotation(new Vector3(x, y, trans.position.z), trans.rotation);
            timeLeft -= Time.deltaTime;
            yield return null;
        }

        if(leftTarget != null && rightTarget != null)
        {
            var target = (dest - leftTarget.position).x > (rightTarget.position - dest).x ? leftTarget.position : rightTarget.position; 
            timeLeft = time;
            while (timeLeft > 0)
            {
                var x = Mathf.Lerp(trans.position.x, target.x, 1 - (timeLeft / time));
                var y = trans.position.y;
                trans.SetPositionAndRotation(new Vector3(x, y, trans.position.z), trans.rotation);
                timeLeft -= Time.deltaTime;
                yield return null;
            }

            target = (dest - leftTarget.position).x < (rightTarget.position - dest).x ? leftTarget.position : rightTarget.position;
            timeLeft = time;
            while (timeLeft > 0)
            {
                var x = Mathf.Lerp(trans.position.x, target.x, 1 - (timeLeft / time));
                var y = trans.position.y;
                trans.SetPositionAndRotation(new Vector3(x, y, trans.position.z), trans.rotation);
                timeLeft -= Time.deltaTime;
                yield return null;
            }
        }
        Destroy(trans.gameObject);

    }
}