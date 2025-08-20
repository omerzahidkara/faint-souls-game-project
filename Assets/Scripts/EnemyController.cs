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
    public GameController controller;

    private EnemyType enemyType;

    public LayerMask playerLayer;  // player layer i�in referans

    public AIPath AIPath;             // A* i�in referans
    public AIDestinationSetter AIDestinationSetter;

    public bool isReached;           // enemy playera vurmak i�in vard� m�
    private bool isAttacking;        // Sald�r� yap�l�p yap�lmad���n� kontrol eder  
    private Animator animator;       // enemy animasyonlar�n� y�netmek i�in

    public int maxHealth;      // Max can de�eri

    public HealthBarEnemies healthBar;      //UI can bar� referans�


    public float moveSpeed;
    public int health;
    private bool canAttack; // Sald�r� yap�l�p yap�lamayaca��n� kontrol eden de�i�ken
    private float attackCooldown; // Sald�r� bekleme s�resi
    public int damage;
    public bool isDamaged;// Hasar yedi�i an true olacak ve animasyon �al��acak
    private int expValue; //d��man�n oyuncuya verece�i tecr�bepuan�

    private Rigidbody2D rb;
    private Vector3 moveDirection;

    private void CheckIfReached()// enemy bakt��� y�n ve oyuncuya var�� kontrol�
    {
        moveDirection = rb.velocity.normalized; // Hareket y�n�n� al

        if (AIPath.desiredVelocity.x >= 0.01f)// A* velocity y�n�ne g�re sprite y�n� ayarlamas�
        {
            transform.localScale = new Vector3(1f, 1f, 1f);
        }
        else if (AIPath.desiredVelocity.x <= -0.01f)
        {
            transform.localScale = new Vector3(-1f, 1f, 1f);
        }

        // Playera eri�ildi mi kontrol et 
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
    private void WhichEnemy()// d��man tipine g�re �zelliklerini belirleriz
    {
        string objectTag = gameObject.tag;

        switch (objectTag)
        {
            case "LowZombie":
                enemyType = EnemyType.LowZombie;          
                maxHealth = 15;
                attackCooldown = 2.5f;
                damage = 5;
                expValue = 1;
                break;
            case "MediumZombie":
                enemyType = EnemyType.MediumZombie;
                maxHealth = 25;
                attackCooldown = 2.5f;
                damage = 5;
                expValue = 3;
                break;
            case "HighZombie":
                enemyType = EnemyType.HighZombie;
                maxHealth = 50;
                attackCooldown = 2f;
                damage = 10;
                expValue = 5;
                break;
            case "EvilSpirit":
                enemyType = EnemyType.EvilSpirit;
                maxHealth = 90;
                attackCooldown = 1.5f;
                damage = 10;
                expValue = 10;
                break;
            case "Demon":
                enemyType = EnemyType.Demon;
                maxHealth = 300;
                attackCooldown = 3f;
                damage = 100;
                expValue = 666;
                controller.demonCount++;
                break;
            default:
                break;
        }

    }

    private void Awake()
    {
        moveSpeed = AIPath.maxSpeed;
        animator = GetComponent<Animator>(); // Animator bile�enini al
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        controller = GameController.Instance;
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

    private void OnDestroy()// Destroy zaman� �a�r�l�r
    {
        if(gameObject.tag == "Demon")
        {
            controller.demonCount--;
        }
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

        yield return new WaitForSeconds(0.25f); // Animasyon s�resi
        isDamaged = false;
        animator.SetBool("isDamaged", isDamaged);

        if (AIPath != null)
        {
            AIPath.enabled = true; // Hareketi tekrar a�
        }
        yield break;
    }

    public void TakeDamage(int damage)
    {
        StartCoroutine(DamageAnim()); // Hasar yeme animasyonunu �al��t�r

        healthBar.gameObject.SetActive(true);
        health -= damage;
        health = Mathf.Clamp(health, 0, maxHealth); // 0 - max limit

    }

    private IEnumerator EvilSpiritHitEffect()
    {
        PlayerController.Instance.moveSpeed = 1.0f;
        yield return new WaitForSeconds(5.5f);
        PlayerController.Instance.moveSpeed = 4f;
        yield break;
    }
    private IEnumerator EvilSpiritDeadBuff()
    {
        float duration = 5f; // 4 saniye s�recek
        float startSpeed = 10f;
        float targetSpeed = 4f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            PlayerController.Instance.moveSpeed = Mathf.Lerp(startSpeed, targetSpeed, elapsedTime / duration);// azalarak yava�lama
            yield return null;//her frame
        }

        PlayerController.Instance.moveSpeed = targetSpeed; // Son de�eri kesinle�tir
    }
    public void Attack(int damage, Collider2D collider)
    {
        if (collider != null)
        {
            if(gameObject.tag == "EvilSpirit")
            {
                StartCoroutine(EvilSpiritHitEffect()); 
            }
            // E�er d��man varsa d��man�n hasar g�rme fonksiyonunu aktif et
            collider.GetComponent<Warrior>()?.TakeDamage(damage);
        }

    }
    private IEnumerator Attack(float attackCD)
    {
        if (!canAttack) yield break; // E�er sald�r� yap�lam�yorsa ��k

        canAttack = false; // Sald�r� yap�ld�, tekrar sald�r�y� engelle

        isAttacking = true;
        // Enemy bakt��� y�n� hesapla
        var attackPos = transform.position + moveDirection * 0.5f;
        var collider = Physics2D.OverlapCircle(attackPos, 0.5f, playerLayer);//oyuncu collider�n� al

        yield return new WaitForSeconds(0.22f); // Animasyon s�resi

        isAttacking = false;

        yield return new WaitForSeconds(attackCD); // Dinlenme s�resi (cooldown)

        Attack(damage, collider);

        canAttack = true; // Yeniden sald�r� yap�labilir
    }

    public IEnumerator DyingAnim()
    {
        if(gameObject.tag == "EvilSpirit")
        {
            StartCoroutine(EvilSpiritDeadBuff());
        }

        yield return new WaitForSeconds(3.0f); // Animasyon s�resi
        Destroy(gameObject);
        yield break;
    }

    private void GiveExp()
    {
        if(PlayerController.Instance.level <=6)
        {
            PlayerController.Instance.experience += expValue;
        }
        else
        {
            PlayerController.Instance.experience += HealthExpMultiplier();
        }
    }
    private int HealthExpMultiplier()
    {
        float pieceEarning = (float)PlayerController.Instance.spiritNum / 100f;
        float netEarningDb = (pieceEarning) * (float)expValue;
        int netEarning = (int)netEarningDb;
        Debug.Log("exp: "+netEarning);
        return netEarning;
    }
    public void Die()
    {
        animator.SetTrigger("isDead");

        CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            Destroy(circleCollider);//�l� d��mana vurmay� inaktif ettik
        }

        StartCoroutine(DyingAnim());
    }

}
