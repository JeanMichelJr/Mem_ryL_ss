using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Coffee : Enemy
{
    public GameObject enemyNumber;

    private Boss3 Boss;
    private GameObject Conveyor;

    private float intensity = 5;
    private float cooldown = 5;

    private float timer;
    private float initialPos;
    private float targetPos;
    private bool init = false;
    private GameObject hud;

    public override void Initialize()
    {
        life.onDeath += StunBoss;
        timer = cooldown;

        //Affichage du hud
        hud = Instantiate(enemyNumber, CombatManager.instance.canvas.transform);
        setHUD(hud.GetComponent<EnemyHUD>(), CombatManager.instance.addEnemy(this));

        init = false;
    }

    public void SetUp(Boss3 boss, GameObject conveyor, float i, float t)
    {
        Boss = boss;
        Conveyor = conveyor;
        intensity = i;
        timer = t;

        //Positionnement
        Vector3 mPos = Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0));
        mPos.z = 0;
        mPos.x -= gameObject.GetComponent<SpriteRenderer>().bounds.size.x / 2;
        mPos.y = Conveyor.transform.position.y + 0.8f;
        transform.position = mPos;
        hud.transform.position = hudPos.transform.position;

        initialPos = transform.position.x;
        targetPos = Conveyor.transform.position.x;

        init = true;
    }

    private void StunBoss()
    {
        Boss.Stun(intensity);
    }

    public override void ForLateUpdate()
    {
        if (init)
        {
            timer -= Time.deltaTime;
            var newPos = Mathf.Lerp(initialPos, targetPos , 1 - (timer / cooldown));
            transform.position = new Vector3(newPos, transform.position.y , 1);
            hud.transform.position = hudPos.transform.position;

            if (timer <= 0)
            {
                Boss.PrepareNewCoffe();
                Disable();
            }
        }
    }

    public override void Delete()
    {
        life.onDeath -= StunBoss;
    }

    public void OnDisable()
    {
        CombatManager.instance.removeEnemy(this);
    }
}
