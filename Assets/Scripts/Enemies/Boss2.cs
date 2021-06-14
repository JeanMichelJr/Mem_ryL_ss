using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boss2 : Enemy
{
    public enum Boss2Type
    {
        Case,
        Mouse,
        Screen,
        Keyboard
    }

    public Boss2Type type;
    public Animator FaceAnim;
    public SpriteRenderer CableRend;
    public SpriteRenderer LedRend;

    private SpriteRenderer rend;
    private List<Boss2> OtherParts;
    static private Coroutine cableCoroutine = null;

    public override void Initialize()
    {
        life.onDeath += DeathAction;
        if(type == Boss2Type.Case)
        {
            life.onDeath += CaseDeath;
        }
        else
        {
            life.onDeath += ComponentDeath;
        }

        OtherParts = transform.parent.GetComponentsInChildren<Boss2>().Where(x => x.type != this.type).ToList();
        rend = GetComponent<SpriteRenderer>();
    }

    private void DeathAction()
    {
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(scaleTween(transform.localScale.x, baseScale.x, 1f));
        StartCoroutine(mainColorTween(rend.color, new Color32(100, 90, 90, 255), 1));
        FaceAnim.Play("Death");
    }

    private void CaseDeath()
    {
        foreach (var component in OtherParts.Where(x => x.life.alive))
        {
            component.life.Damage(component.life.currentLife);
        }
    }

    private void ComponentDeath()
    {
        //if (cableCoroutine == null)
        //{
        //    cableCoroutine = StartCoroutine(cableTweenColor(Color.black, new Color32(255, 154, 0, 255), 0.5f));
        //}
        foreach (var component in OtherParts.Where(x => x.life.alive))
        {
            component.ResetCoolDown();
        }

        var c = OtherParts.FirstOrDefault(x => x.type == Boss2Type.Case);
        c.life.damageResistance--;
        LedRend.color = new Color32(200, 0, 0, 255);
        CableRend.gameObject.SetActive(false);
    }

    private IEnumerator mainColorTween(Color color1, Color color2, float timeToTween)
    {
        float timeLeft = timeToTween;
        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            rend.color = Color.Lerp(color1, color2, 1 - (timeLeft / timeToTween));
            yield return null;
        }

        rend.color = color2;
    }

    private IEnumerator cableTweenColor(Color color1, Color color2, float timeToTween)
    {
        float timeLeft = timeToTween;
        while (timeLeft > timeToTween / 2)
        {
            timeLeft -= Time.deltaTime;
            CableRend.color = Color.Lerp(color1, color2, 1 - (timeLeft / timeToTween));
            yield return null;
        }

        while (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            CableRend.color = Color.Lerp(color2, color1, 1 - (timeLeft / timeToTween));
            yield return null;
        }

        CableRend.color = color1;
        cableCoroutine = null;
    }

    public override void Delete()
    {
        life.onDeath -= DeathAction;
        if (type == Boss2Type.Case)
        {
            life.onDeath -= CaseDeath;
        }
        else
        {
            life.onDeath -= ComponentDeath;
        }
        if (cableCoroutine != null)
        {
            StopCoroutine(cableCoroutine);
            cableCoroutine = null;
        }
    }
}
