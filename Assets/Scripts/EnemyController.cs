using Pathfinding;
using System.Collections;
using UnityEngine;


public class EnemyController : MonoBehaviour, Warrior
{
    public enum EnemyType
    {
        LowZombie,  
        MediumZombie,
        HighZombie,
        EvilSpirit,
        Demon   
    }

    private EnemyType enemyType;

    public LayerMask playerLayer;  // player layer için referans

    public AIPath AIPath;             // A* için referans
    public AIDestinationSetter AIDestinationSetter;

    public bool isReached;           // enemy playera vurmak için vardý mý
    private bool isAttacking;        // Saldýrý yapýlýp yapýlmadýðýný kontrol eder  
    private Animator animator;       // enemy animasyonlarýný yönetmek için

    public int maxHealth;      // Max can deðeri

    public HealthBarEnemies healthBar;      //UI can barý referansý


    public float moveSpeed;
    public int health;
    private bool canAttack; // Saldýrý yapýlýp yapýlamayacaðýný kontrol eden deðiþken
    private float attackCooldown; // Saldýrý bekleme süresi
    public int damage;
    public bool isDamaged;// Hasar yediði an true olacak ve animasyon çalýþacak
    private int expValue; //düþmanýn oyuncuya vereceði tecrübepuaný

    private Rigidbody2D rb;
    private Vector3 moveDirection;

    private void CheckIfReached()// enemy baktýðý yön ve oyuncuya varýþ kontrolü
    {
        moveDirection = rb.velocity.normalized; // Hareket yönünü al

        if (AIPath.desiredVelocity.x >= 0.01f)// A* velocity yönüne göre sprite yönü ayarlamasý
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (AIPath.desiredVelocity.x <= -0.01f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        // Playera eriþildi mi kontrol et 
        if(AIPath.reachedEndOfPath)
        {
            isReached = true;
            StartCoroutine(Attack(attackCooldown));
        }
        else
        {
            isReached = false;
        }
        animator.SetBool("isReached", isReached);
    }
    private void WhichEnemy()// düþman tipine göre özelliklerini belirleriz
    {
        string objectTag = gameObject.tag;

        switch (objectTag)
        {
            case "LowZombie":
                enemyType = EnemyType.LowZombie;          
                maxHealth = 12;
                attackCooldown = 1f;
                damage = 2;
                expValue = 1;
                break;
            case "MediumZombie":
                enemyType = EnemyType.MediumZombie;
                maxHealth = 21;
                attackCooldown = 0.9f;
                damage = 2;
                expValue = 3;
                break;
            case "HighZombie":
                enemyType = EnemyType.HighZombie;
                maxHealth = 24;
                attackCooldown = 0.9f;
                damage = 4;
                expValue = 5;
                break;
            case "EvilSpirit":
                enemyType = EnemyType.EvilSpirit;
                maxHealth = 46;
                attackCooldown = 0.7f;
                damage = 6;
                expValue = 10;
                break;
            case "Demon":
                enemyType = EnemyType.Demon;
                maxHealth = 300;
                attackCooldown = 1.1f;
                damage = 50;
                expValue = 666;
                break;
            default:
                break;
        }

    }

    private void Awake()
    {
        moveSpeed = AIPath.maxSpeed;
        animator = GetComponent<Animator>(); // Animator bileþenini al
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        canAttack = true;
        WhichEnemy();
        healthBar.SetMaxHealth(maxHealth);
        health = maxHealth;
        healthBar.SetHealth(health);
        healthBar.gameObject.SetActive(false);
        AIDestinationSetter.target = PlayerController.Instance.gameObject.transform;
    }

    public void Update()////////////////////////UPDATE HERE
    {
        CheckIfReached();
        UpdateHealthBar();

        if (health <= 0)
        {
            AIPath.enabled = false; // Hareketi durdur
            AIDestinationSetter.target = null;

            Die();
        }
    }

    private void OnDestroy()// Destroy zamaný çaðrýlýr
    {
        GiveExp();
    }

    private void UpdateHealthBar()
    {

        if (healthBar == null)
        {
            return;
        }

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        healthBar.SetHealth(health);
    }

    public IEnumerator DamageAnim()
    {
        isDamaged = true;
        animator.SetBool("isDamaged", isDamaged);

        if (AIPath != null)
        {
            AIPath.enabled = false; // Hareketi durdur
        }

        yield return new WaitForSeconds(0.25f); // Animasyon süresi
        isDamaged = false;
        animator.SetBool("isDamaged", isDamaged);

        if (AIPath != null)
        {
            AIPath.enabled = true; // Hareketi tekrar aç
        }
        yield break;
    }

    public void TakeDamage(int damage)
    {
        StartCoroutine(DamageAnim()); // Hasar yeme animasyonunu çalýþtýr

        healthBar.gameObject.SetActive(true);
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth); // 0 - max limit

    }

    private IEnumerator EvilSpiritHitEffect()
    {
        PlayerController.Instance.moveSpeed = 1.3f;
        yield return new WaitForSeconds(2f);
        PlayerController.Instance.moveSpeed = 2.5f;
        yield break;
    }
    private IEnumerator EvilSpiritDeadBuff()
    {
        float duration = 3f; // 3 saniye sürecek
        float startSpeed = 3.5f;
        float targetSpeed = 2.5f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            PlayerController.Instance.moveSpeed = Mathf.Lerp(startSpeed, targetSpeed, elapsedTime / duration);// azalarak yavaþlama
            yield return null;//her frame
        }

        PlayerController.Instance.moveSpeed = targetSpeed; // Son deðeri kesinleþtir
    }
    public void Attack(int damage, Collider2D collider)
    {
        if (collider != null)
        {
            if(gameObject.tag == "EvilSpirit")
            {
                StartCoroutine(EvilSpiritHitEffect()); 
            }
            // Eðer düþman varsa düþmanýn hasar görme fonksiyonunu aktif et
            collider.GetComponent<Warrior>()?.TakeDamage(damage);
        }

    }
    private IEnumerator Attack(float attackCD)
    {
        if (!canAttack) yield break; // Eðer saldýrý yapýlamýyorsa çýk

        canAttack = false; // Saldýrý yapýldý, tekrar saldýrýyý engelle

        isAttacking = true;
        // Enemy baktýðý yönü hesapla
        var attackPos = transform.position + moveDirection * 0.5f;
        var collider = Physics2D.OverlapCircle(attackPos, 0.5f, playerLayer);//oyuncu colliderýný al

        yield return new WaitForSeconds(0.22f); // Animasyon süresi

        isAttacking = false;

        yield return new WaitForSeconds(attackCD); // Dinlenme süresi (cooldown)

        Attack(damage, collider);

        canAttack = true; // Yeniden saldýrý yapýlabilir
    }

    public IEnumerator DyingAnim()
    {
        if(gameObject.tag == "EvilSpirit")
        {
            StartCoroutine(EvilSpiritDeadBuff());
        }

        yield return new WaitForSeconds(3.0f); // Animasyon süresi
        Destroy(gameObject);
        yield break;
    }

    private void GiveExp()
    {
        PlayerController.Instance.experience += expValue;
    }

    public void Die()
    {
        animator.SetTrigger("isDead");

        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            Destroy(circleCollider);//ölü düþmana vurmayý inaktif ettik
        }

        StartCoroutine(DyingAnim());
    }

}
