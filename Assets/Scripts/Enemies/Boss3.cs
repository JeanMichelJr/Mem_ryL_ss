using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boss3 : Enemy
{
    public Animator Anim;
    public GameObject Conveyor;
    public GameObject Coffee;

    public int CoffeAppartionMin = 10;
    public int CoffeAppartionMax = 20;

    public float StunCooldown = 5;
    public float CoffeeDuration = 5;

    private float stunTimer;
    private float nextJump;
    private float nextCoffe;

    private System.Random random = new System.Random();

    public override void Initialize()
    {
        stunTimer = 0;
        PrepareNewCoffe();
    }

    public override void ForLateUpdate()
    {
        //Gestion de l'animation de stun et du stun
        if (Anim.GetCurrentAnimatorStateInfo(0).IsTag("stun"))
        {
            stuned = true;
            ResetCoolDown();
        }
        else if(stuned)
        {
            stuned = false;
            life.damageResistance = 1;
            PrepareNewCoffe();
        }

        if (stuned && stunTimer > 0)
        {
            stunTimer -= Time.deltaTime;
        }
        else if(stuned && stunTimer <= 0)
        {
            Anim.SetBool("stuned", false);
        }

        //Gestion du saut
        if (nextJump > 0 && Anim.GetCurrentAnimatorStateInfo(0).IsTag("wait"))
        {
            nextJump -= Time.deltaTime;
            if(nextJump <= 0)
            {
                Anim.SetBool("jump", true);
            }
        }
        else if (Anim.GetCurrentAnimatorStateInfo(0).IsTag("jump"))
        {
            Anim.SetBool("jump", false);
            if (nextJump <= 0)
            {
                nextJump = random.Next(3, 10);
            }
        }

        if(nextCoffe > 0)
        {
            nextCoffe -= Time.deltaTime;
            if(nextCoffe <= 0)
            {
                var coffe = Instantiate(Coffee, transform.parent);
                coffe.GetComponent<Coffee>().SetUp(this, Conveyor, StunCooldown, CoffeeDuration);
            }
        }
    }

    public void Stun(float time)
    {
        stunTimer = time;
        life.damageResistance = 0.5f;
        Anim.SetBool("stuned", true);
    }

    public void PrepareNewCoffe()
    {
        nextCoffe = random.Next(CoffeAppartionMin, CoffeAppartionMax);
    }

    public override void Delete()
    {
    }
}
