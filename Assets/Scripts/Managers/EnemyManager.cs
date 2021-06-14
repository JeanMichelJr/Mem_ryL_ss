using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance = null;

    public Enemy[] enemyLibrary;

    public GameObject[] bossLibrary;

    public GameObject enemyContainer;

    public int minLevel { get; private set; }
    public int maxLevel { get; private set; }

    public int enemyCount { get => enemyLibrary.Length; }

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

        minLevel = enemyLibrary.Min(e => e.pattern.Length);
        maxLevel = enemyLibrary.Max(e => e.pattern.Length);
    }
}
