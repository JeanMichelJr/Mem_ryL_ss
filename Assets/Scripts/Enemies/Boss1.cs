using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boss1 : Enemy
{
    public List<GameObject> Parts;
    public List<String> Patterns;
    private int nbParts;
    private float lifeSteps;
    private int baseAttackDamage;

    public override void Initialize()
    {
        nbParts = Parts.Count();
        lifeSteps = life.maxLife / (nbParts + 1);
        baseAttackDamage = attackDamage / nbParts;
        baseAttackDamage = baseAttackDamage == 0 ? 1 : baseAttackDamage;
    }

    private IEnumerator fadeOut(SpriteRenderer rend, float timeToFade)
    {
        Color color1 = rend.color;
        Color color2 = new Color32((byte)color1.r, (byte)color1.g, (byte)color1.b, 0);
        float timeLeft = timeToFade;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            rend.color = Color.Lerp(color1, color2, 1 - (timeLeft / timeToFade));
            yield return null;
        }
    }

    public override void ForLateUpdate()
    {
        if (life.currentLife < Parts.Count * lifeSteps)
        {
            attackDamage = baseAttackDamage * Parts.Count();

            pattern = Patterns.FirstOrDefault();
            Patterns.Remove(pattern);

            GameObject part = Parts.FirstOrDefault();
            Parts.Remove(part);
            foreach (SpriteRenderer rend in part.GetComponentsInChildren<SpriteRenderer>())
            {
                StartCoroutine(fadeOut(rend, 1));
            }
        }
    }

    public override void Delete()
    {
    }
}
