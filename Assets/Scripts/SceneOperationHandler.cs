using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneOperationHandler : MonoBehaviour
{
    public enum CommandType
    {
        Load,
        Unload
    }

    public class SceneCommand
    {
        public CommandType type;
        public string name;
        public AsyncOperation op;
    }

    private Coroutine coroutine;
    private List<SceneCommand> commands = new List<SceneCommand>();

    public event Action onStart;
    public event Action<IReadOnlyList<SceneCommand>> onComplete;

    public float currentProgression { get; private set; }
    public float realProgression { get; private set; }
    public bool isRunning { get => coroutine != null; }

    public void Add(IEnumerable<string> scenes, CommandType type)
    {
        foreach (var name in scenes)
        {
            var command = new SceneCommand()
            {
                name = name,
                type = type,
                op = type == CommandType.Load ?
                                SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive) :
                                SceneManager.UnloadSceneAsync(name)
            };

            commands.Add(command);
        }

        if (coroutine == null)
        {
            if (onStart != null)
            {
                onStart.Invoke();
            }

            coroutine = StartCoroutine(WaitUntilOperationComplete());
        }
    }

    private IEnumerator WaitUntilOperationComplete()
    {
        while (true)
        {
            var ops = commands.Where(c => c.op != null).Select(c => c.op);
            realProgression = ops.Sum(o => o.progress) / ops.Count();
            currentProgression = realProgression > currentProgression ? realProgression : currentProgression;

            if (!ops.All(o => o.isDone))
            {
                yield return null;
            }
            else
            {
                break;
            }
            Debug.Log($"RealP = {realProgression}\ncurrentP = {currentProgression}", this);
        }

        if (onComplete != null)
        {
            onComplete.Invoke(commands.AsReadOnly());
        }

        commands.Clear();
        realProgression = 0f;
        currentProgression = 0f;
        coroutine = null;

        GC.Collect();
    }
}
