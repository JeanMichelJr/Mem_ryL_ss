using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHUD : MonoBehaviour
{
    public Transform HealthBar;
    public Text HealthAmount; 
    public Transform CooldownBar;
    public Text EnemyNumber;
    private int num;

    public void UpdateHealthBar(float percent, int amount)
    {
        if (HealthAmount != null)
            HealthAmount.text = amount.ToString();

        if(HealthBar != null)
            HealthBar.localPosition = new Vector3(-1 + percent, HealthBar.localPosition.y, HealthBar.localPosition.z);
    }

    public void UpdateCooldownBar(float percent)
    {
        if (CooldownBar != null)
            CooldownBar.localPosition = new Vector3(-percent, CooldownBar.localPosition.y, CooldownBar.localPosition.z);
    }

    public void SetEnemyNumber(int num)
    {
        this.num = num;
        EnemyNumber.text = num.ToString();
    }

    public void Select()
    {
        EnemyNumber.text = $"<color=#00AAff>{num}</color>";
    }

    public void Unselect()
    {
        EnemyNumber.text = num.ToString();
    }
}
