using System;
using UnityEngine;

[Serializable]
public class ManaHandler
{
    public int maxMana = 5;
    public int currentMana { get; private set; }

    public delegate void UpdateManaDelegate(float current_life, float last_update);
    public event UpdateManaDelegate onConsume;
    public event UpdateManaDelegate onRestore;

    public void Spawn()
    {
        Spawn(maxMana);
    }
    public void Spawn(int starting_mana)
    {
        Debug.Log($"Spawning with {starting_mana} mp");
        currentMana = starting_mana;
    }

    public float Consume(float amount)
    {
        return Consume(MathsUtils.RoundToIntNonZero(amount * maxMana));
    }
    public float Consume(int amount)
    {
        if (amount > currentMana)
            return -1f;

        currentMana -= amount;

        onConsume?.Invoke( currentMana, amount );

        return currentMana;
    }

    public float RestoreLeftMana(float amount)
    {
        return Restore(MathsUtils.RoundToIntNonZero(amount * (maxMana - currentMana)));
    }

    public float Restore(float amount)
    {
        return Restore(MathsUtils.RoundToIntNonZero(amount * maxMana));
    }
    public float Restore(int amount)
    {
        amount = Mathf.Min(amount, maxMana - currentMana);
        currentMana += amount;

        onRestore?.Invoke( currentMana, amount );

        return currentMana;
    }
}
