using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour
{
    public string enemyName = "defaultName";
    public string pattern = "abcba";
    public LifeHandler life;
    public int attackDamage;
    public int difficulty;
    protected float attackCooldown;
    public float attackTimer;
    public bool stayAfterDeath = false;
    public SpriteRenderer whiteRenderer;
    protected bool stuned = false;

    public Transform hudPos;
    private EnemyHUD hud { get; set; }

    public bool CombatStarted { get; set; } = false;
    private bool isScaling = false;
    protected Vector3 baseScale;

    private Coroutine colorCoroutine = null;
    protected Coroutine scaleCoroutine = null;

    private Color attackColor;
    private Color lightBlue = new Color32(44, 250, 250, 255);
    private Color yellow = Color.yellow;

    public void Awake()
    {
        life.onTakeDamage += UpdateHealthHud;
        life.onHeal += UpdateHealthHud;

        // attackColor = lightBlue;
        attackColor = yellow;
    }
    public void OnEnable()
    {
        life.Spawn();
        Initialize();
    }

    private void Start()
    {
        attackCooldown = attackTimer;
        baseScale = transform.localScale;
    }

    public void Stun(float time)
    {
        if (colorCoroutine != null || scaleCoroutine != null)
        {
            return;
        }

        attackCooldown += time;
    }

    private void Update()
    {
        if (!CombatStarted || !life.alive || attackTimer == 0 || stuned)
        {
            return;
        }

        attackCooldown -= Time.deltaTime;
        if (attackCooldown < 0 && colorCoroutine == null && scaleCoroutine == null)
        {
            scaleCoroutine = StartCoroutine(scaleTween(transform.localScale.x, transform.localScale.x * 1.35f, 0.1f));
            isScaling = true;
            CastAttack();
            colorCoroutine = StartCoroutine(colorTween(attackColor, Color.white, 0.25f));
            attackCooldown = attackTimer;

        }
        if (attackCooldown < (attackTimer * 0.3) && colorCoroutine == null && scaleCoroutine == null)
        {
            scaleCoroutine = StartCoroutine(scaleTween(transform.localScale.x, 1.2f * transform.localScale.x, 0.2f));
            colorCoroutine = StartCoroutine(colorTween(Color.white, attackColor, (attackTimer * 0.3f)));
        }
        if (isScaling && scaleCoroutine == null)
        {
            scaleCoroutine = StartCoroutine(scaleTween(transform.localScale.x, transform.localScale.x / 1.62f, 0.3f));
            isScaling = false;
        }
    }

    private IEnumerator colorTween(Color color1, Color color2, float timeToTween)
    {
        float timeLeft = timeToTween;
        while (timeLeft > 0)
        {
            yield return null;
            timeLeft -= Time.deltaTime;
            if (whiteRenderer != null)
            {
                whiteRenderer.color = Color.Lerp(color1, color2, 1 - (timeLeft / timeToTween));
            }
        }

        if (whiteRenderer != null)
        {
            whiteRenderer.color = color2;
        }

        colorCoroutine = null;
    }

    protected IEnumerator scaleTween(float scale1, float scale2, float timeToTween)
    {
        // Vector3 oldScale = transform.localScale;
        float timeLeft = timeToTween;
        while (timeLeft > 0)
        {
            yield return null;
            timeLeft -= Time.deltaTime;
            var newScale = Mathf.Lerp(scale1, scale2, 1 - (timeLeft / timeToTween));
            transform.localScale = new Vector3(newScale, newScale, 1);
            // transform.localScale = oldScale * ((scale1 * (timeLeft / timeToTween)) + (scale2 * (1 - (timeLeft / timeToTween))));
        }
        transform.localScale = new Vector3(scale2, scale2, 1);
        scaleCoroutine = null;
    }

    private void LateUpdate()
    {
        ForLateUpdate();
        if (hud != null)
        {
            hud.UpdateCooldownBar(attackCooldown / attackTimer);
        }

    }

    public virtual void CastAttack()
    {
        Player.instance.life.Damage(attackDamage);
        SoundManager.instance.PlayOneShot("playerDamage");
    }

    //Methode virtuelle pure, utilis�es par les ennemis sp�ciaux si besoin (cf boss2)
    public virtual void Initialize() { }
    public virtual void Delete() { }
    public virtual void ForLateUpdate() { }

    public void ResetCoolDown()
    {
        attackCooldown = attackTimer;
        if (scaleCoroutine != null)
        {
            StopCoroutine(scaleCoroutine);
        }
        scaleCoroutine = StartCoroutine(scaleTween(transform.localScale.x, baseScale.x, 0.2f));
    }

    public void setHUD(EnemyHUD ehud, int index)
    {
        hud = ehud;
        hud.UpdateCooldownBar(attackCooldown / attackTimer);
        hud.UpdateHealthBar(life.currentLife / (float)life.maxLife, life.currentLife);
        hud.SetEnemyNumber(index);
    }

    private void UpdateHealthHud(float current, float last)
    {
        hud.UpdateHealthBar(current / life.maxLife, (int)current);
    }

    public void Disable()
    {
        Unselect();
        if (!stayAfterDeath)
        {
            gameObject.SetActive(false);
        }
        hud.gameObject.SetActive(false);
    }

    public void Select()
    {
        hud.Select();
    }

    public void Unselect()
    {
        hud.Unselect();
    }

    public void OnDestroy()
    {
        Delete();
        life.onHeal -= UpdateHealthHud;
        life.onTakeDamage -= UpdateHealthHud;
    }
}
