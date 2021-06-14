using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SceneOperationHandler))]
public class GameController : MonoBehaviour
{
    [Serializable]
    private class LevelEntry
    {
        public GameState gameState;
        public string[] levels;
    }

    [Serializable]
    public class LevelDictionary : IDictionary<GameState, string[]>
    {
        [SerializeField]
        private List<LevelEntry> levelEntries = new List<LevelEntry>();
        public string[] this[GameState key]
        {
            get
            {
                foreach (var le in levelEntries)
                {
                    if (le.gameState == key)
                        return le.levels;
                }

                throw new KeyNotFoundException($"{key} not found");
            }
            set
            {
                foreach (var le in levelEntries)
                {
                    if (le.gameState == key)
                        le.levels = value;
                }

                Add(key, value);
            }
        }

        public ICollection<GameState> Keys => levelEntries.Select(le => le.gameState).ToArray();
        public ICollection<string[]> Values => levelEntries.Select(le => le.levels).ToArray();

        public int Count => levelEntries.Count;

        public bool IsReadOnly => false;

        public void Add(GameState key, string[] value)
        {
            if (ContainsKey(key))
            {
                throw new ArgumentException();
            }

            levelEntries.Add(new LevelEntry()
            {
                gameState = key,
                levels = value
            });
        }

        public void Add(KeyValuePair<GameState, string[]> item) => Add(item.Key, item.Value);

        public void Clear() => levelEntries.Clear();

        public bool Contains(KeyValuePair<GameState, string[]> item) => ContainsKey(item.Key);

        public bool ContainsKey(GameState key) => levelEntries.Any(le => le.gameState == key);

        public void CopyTo(KeyValuePair<GameState, string[]>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<GameState, string[]>> GetEnumerator() => levelEntries.Select(le => new KeyValuePair<GameState, string[]>(le.gameState, le.levels)).GetEnumerator();

        public bool Remove(GameState key) => levelEntries.RemoveAll(le => le.gameState == key) > 0;

        public bool Remove(KeyValuePair<GameState, string[]> item) => Remove(item.Key);

        public bool TryGetValue(GameState key, out string[] value)
        {
            var temp = levelEntries.Where(le => le.gameState == key).GetEnumerator();
            
            if (!temp.MoveNext())
            {
                value = default;
                return false;
            }
            else
            {
                value = temp.Current.levels;
                return true;
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => levelEntries.Select(le => new KeyValuePair<GameState, string[]>(le.gameState, le.levels)).GetEnumerator();
    }

    private class GameStateMachine : AbstractStateMachine<GameState>
    {
        public GameStateMachine() : base()
        {}
        public GameStateMachine(GameState value) : base(value)
        {}

        public override bool CheckTransition(GameState next_state)
        {
            switch (currentState)
            {
                case GameState.Initialization:
                    return next_state == GameState.Start;
                case GameState.Start:
                    return next_state == GameState.HomeMenu;
                case GameState.HomeMenu:
                    return next_state == GameState.Navigation
                        || next_state == GameState.Credits;
                case GameState.Navigation:
                    return next_state == GameState.HomeMenu 
                        || next_state == GameState.Combat
                        || next_state == GameState.GameOver;
                case GameState.Combat:
                    return next_state == GameState.Navigation 
                        || next_state == GameState.GameOver 
                        || next_state == GameState.Victory;
                case GameState.Victory:
                    return next_state == GameState.Credits;
                case GameState.GameOver:
                    return next_state == GameState.HomeMenu;
                case GameState.Credits:
                    return next_state == GameState.HomeMenu;
            }

            return false;
        }
    }

    public SceneOperationHandler sceneOperation;
    public LevelDictionary levelDictionary;
    public static GameController instance = null;

    private const string SINGLETON = "0_Singleton";
    private List<String> BASE_SCENES = new List<string>(){

    };
    private List<String> BASE_GAME_SCENES = new List<string>(){
        "1_BaseGameSingleton",
        "1_GameSingleton"
    };

    private event Action onGameReady;

    private GameStateMachine gameState = new GameStateMachine();
    private GameState nextState = GameState.Invalid;

    private bool _gameReady = false;
    private bool gameReady
    {
        get => _gameReady;
        set
        {
            if (_gameReady == false && value == true)
            {
                if (onGameReady != null)
                {
                    onGameReady.Invoke();
                    onGameReady = null;
                }
                Debug.Log("GameReady to rumble");
            }
            
            _gameReady = value;
        }
    }


#if UNITY_EDITOR
    private void OnValidate()
    {
        SetUnityVar();
    }

    private void Reset()
    {
        SetUnityVar();
    }

    private void SetUnityVar()
    {
        if (sceneOperation == null)
        {
            sceneOperation = GetComponent<SceneOperationHandler>();

            if (sceneOperation == null)
            {
                sceneOperation = gameObject.AddComponent<SceneOperationHandler>();
            }
        }
    }
#endif

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

#if !UNITY_EDITOR
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
#endif
    }

    private void Start()
    { 
        InitialLoading();
    }

    private void InitialLoading()
    {
        UnloadAll();
        LoadScenes(BASE_SCENES);
        MoveToState(GameState.Start);
    }

    private void UnloadAll(Predicate<string> except = null)
    {
        var names = new List<string>();

        for (var i = 0; i < SceneManager.sceneCount; ++i)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.name != SINGLETON)
            {
                names.Add(scene.name);
            }
        }

        if (names.Count <= 0)
        {
            return;
        }

        if (except != null)
        {
            sceneOperation.Add(names.Where( name => !except(name) ), SceneOperationHandler.CommandType.Unload);
        }
        else
        {
            sceneOperation.Add(names, SceneOperationHandler.CommandType.Unload);
        }
    }

    private void Unload(IEnumerable<string> names, Predicate<string> except = null)
    {
        if (names.Count() <= 0)
        {
            return;
        }

        var effective_names = names.Where( name => SceneManager.GetSceneByName(name).isLoaded );
        if (except != null)
        {
            effective_names = effective_names.Where( name => !except(name) );
        }
        sceneOperation.Add(effective_names, SceneOperationHandler.CommandType.Unload);
    }

    private void LoadScenes(string name, Predicate<string> except = null)
    {
        LoadScenes(new List<string>() {name});
    }

    private void LoadScenes(IEnumerable<string> names, Predicate<string> except = null)
    {        
        if (names.Count() <= 0)
        {
            return;
        }

        var effective_names = names.Where( name => !SceneManager.GetSceneByName(name).isLoaded );
        if (except != null)
        {
            effective_names = effective_names.Where( name => !except(name) );
        }
        sceneOperation.Add(effective_names, SceneOperationHandler.CommandType.Load);
    }

    public void MoveToState(GameState state)
    {
        if (gameState.currentState != GameState.Initialization && !gameReady)
        {
            return;
        }

        if (gameState.currentState == GameState.Invalid || state == GameState.Invalid)
        {
            return;
        }

        if (state == gameState.currentState)
        {
            return;
        }

        if (!gameState.CheckTransition(state))
        {
            return;
        }

        var names = new List<string>();

        if (gameState.currentState == GameState.HomeMenu && state != GameState.Credits)
        {
            names.AddRange(BASE_GAME_SCENES);
        }

        if (levelDictionary.TryGetValue(state, out var val))
        {
            names.AddRange(val);
        }
        else
        {
            throw new Exception($"Missing State \"{state}\" in levelDictionary");
        }

        void ChangeState(IReadOnlyList<SceneOperationHandler.SceneCommand> commands)
        {
            Debug.Log($"LoadingComplete {gameState.currentState} -> {nextState}", this);
            gameState.MoveTo(nextState);
            this.NextFrame( () => gameReady = true);
            sceneOperation.onComplete -= ChangeState;
        }

        gameReady = false;

        nextState = state;

        if (!sceneOperation.isRunning)
        {
            sceneOperation.onComplete += ChangeState;
        }

        if (state == GameState.HomeMenu)
        {
            UnloadAll();
        }
        else
        {
            UnloadAll( name => BASE_SCENES.Contains(name) || BASE_GAME_SCENES.Contains(name) );
        }
        LoadScenes( names );
    }

    public void SubscribeToGameReady(Action action)
    {
        if(action == null)
        {
            return;
        }

        if (gameReady)
        {
            action();
        }
        else
        {
            onGameReady += action;
        }
    }

    public void UnsubscribeToGameReady(Action action)
    {
        if (action == null)
        {
            return;
        }

        onGameReady -= action;
    }
}
