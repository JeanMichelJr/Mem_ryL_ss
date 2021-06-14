using System;
using UnityEngine;
using System.Collections;

[Serializable]
public class LifeHandler : MonoBehaviour
{
    public int maxLife = 5;
    public bool canBeInstantKilled = true; 
    public float damageModifier = 0f;
    public float damageResistance = 1;
    public int currentLife { get; private set; }
    public bool alive { get; private set; } = false;

    public delegate void DeathDelegate();
    public delegate void UpdateLifeDelegate(float current_life, float last_update);
    public event DeathDelegate onDeath;
    public event UpdateLifeDelegate onTakeDamage;
    public event UpdateLifeDelegate onHeal;

    public void Spawn()
    {
        Spawn(maxLife);
    }
    public void Spawn(int starting_life)
    {
        if (alive)
        {
            return;
        }

        Debug.Log($"Spawning with {starting_life} hp");
        currentLife = starting_life;
        alive = true;
    }

    public void Kill()
    {
        if (!canBeInstantKilled)
        {
            return;
        }

        currentLife = 0;
        Die();
    }

    public float Damage(float amount)
    {
        if (!alive)
            return -1f;
        return Damage(MathsUtils.RoundToIntNonZero(amount * maxLife));
    }
    public float Damage(int amount)
    {
        if (!alive)
            return -1f;

        currentLife -= Mathf.RoundToInt((amount * (1 + damageModifier) / (damageResistance > 0 ? damageResistance : 1)));

        if (currentLife <= 0)
        {
            currentLife = 0;
            Die();
        }

        onTakeDamage?.Invoke(currentLife, amount);

        return currentLife;
    }

    public float DamageOverTime(float amount, float time)
    {
        if (!alive)
            return -1f;

        return DamageOverTime(MathsUtils.RoundToIntNonZero(amount * maxLife), time);
    }
    public float DamageOverTime(int amount, float time)
    {
        if (!alive)
            return -1f;

        StartCoroutine(Tick(amount, time, true));

        return currentLife;
    }

    private IEnumerator Tick(int amount, float time, bool isTickDamage)
    {
        float tmp = 0;
        while (time > 0 && CombatManager.instance != null && !CombatManager.instance.isCombatOver())
        {
            if (isTickDamage)
            {
                this.Damage(amount);
            }
            else
            {
                this.Heal(amount);
            }
            tmp = Time.time;
            yield return new WaitForSeconds(1);
            time -= Time.time - tmp;
        }
    }

    public float Heal(float amount)
    {
        if (!alive)
            return -1f;

        return Heal(MathsUtils.RoundToIntNonZero(amount * maxLife));
    }
    public float Heal(int amount)
    {
        if (!alive)
            return -1f;

        currentLife += amount;
        if (currentLife > maxLife)
        {
            currentLife = maxLife;
        }

        onHeal?.Invoke(currentLife, amount);

        return currentLife;
    }

    public float HealOverTime(float amount, float time)
    {
        if (!alive)
            return -1f;

        return HealOverTime(MathsUtils.RoundToIntNonZero(amount * maxLife), time);
    }
    public float HealOverTime(int amount, float time)
    {
        if (!alive)
            return -1f;

        StartCoroutine(Tick(amount, time, false));

        return currentLife;
    }

    private void Die()
    {
        alive = false;
        onDeath?.Invoke();
    }
}
