
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

using TMPro;

using UnityEngine.UI;

// Oyuncunun kontrolünü saðlayan sýnýf
public class PlayerController : MonoBehaviour, Warrior
{
    [SerializeField] public AudioClip step1;
    [SerializeField] public AudioClip hit;
    public TextMeshProUGUI levelIndicator; 
    public float moveSpeed; // Oyuncunun hareket hýzý
    public float stepSize;   // Oyuncunun her adýmda hareket edeceði mesafe
    private bool isMoving;           // Oyuncunun hareket halinde olup olmadýðýný kontrol eder
    private bool isAttacking;        // Saldýrý yapýlp yapýlmadýðýný kontrol eder  
    private Vector2 input;           // Kullanýcýnýn girdi yönü
    private Animator animator;       // Oyuncu animasyonlarýný yönetmek için
    private int damage;               // Oyuncunun saldýrý gücü
    public int experience;             //tecrübe puaný

    public int spiritNum;            // Oyuncunun can deðeri,
    public int maxHealth;      // Max can deðeri
    public bool isGatheredAnySouls; //Soul ile etkileþime girildi mi kontrolü

    public GameObject spiritPrefab;  // Ruhçuk prefab
    public int spiritNumOnTheScene;  // Sahnede oluþacak ruh sayýsýný limitlendirmemiz gerekir
    private List<Vector3> spiritSpawnedPositions = new List<Vector3>();

    public HealthBar healthBar;      //UI can barý referansý
    public ExpBar expBar;           //UI expBar referansý
    public int level;           //oyun zorluðuna etkisi olacak, oyuncunun seviye durumu

    public Tilemap spiritZoneTilemap;  // Spirit'lerin çýkabileceði Tilemap
    private List<Vector3> spiritZonePositions = new List<Vector3>();

    public LayerMask solidObjectsLayer;   // Engel katmaný (duvarlar, aðaçlar vb.)
    public LayerMask doorsLayer;   // Engel katmaný kapý
    public LayerMask interactableLayer;   // Etkileþimli nesneler katmaný (NPC'ler vb.)
    public LayerMask spiritLayer;         // Rastgele spirit oluþturucak alanlar
    public LayerMask enemyLayer;         // Rastgele spirit oluþturucak alanlar
    private bool walkSoundCheck;

    private bool canAttack; // Saldýrý yapýlýp yapýlamayacaðýný kontrol eden deðiþken
    private bool attackAnim;
    private float attackCooldown; // Saldýrý bekleme süresi

    public bool isDead;

    public event Action LevelUp; // Seviye alýndýðýnda tetiklenecek olay


    public int continousLevelBorder;

    [SerializeField] public AudioClip demonSlayedSound;

    [SerializeField] public Text DeadPanelScore;
    [SerializeField] public GameObject DeadPanel;

    public static PlayerController Instance { get; private set; } // Singleton eriþimi

    private Dictionary<int, (int damage, int maxHealth, int expBorder, float attackCoolDown)> levelData = new()// levela göre oyun özelliklerini saklarýz
    {
        {2, (7, 110, 30, 0.4f)},    // MediumZombie
        {3, (8, 120, 70, 0.2f)},    // HighZombie 
        {4, (10, 150, 100, 0.2f)},    // EvilSpirit 
        {5, (24, 200, 766, 0.1f)},  // Demon 
        {6, (26, 200, 1532, 0.0f)},  // Sonsuz seviyeye geçiþ sýnýrý
    };

    private void Start()
    {
        walkSoundCheck = true;
        GetSpiritZonePositions();//can çýkacak alanlarýn kaydedilmesi
        spiritNumOnTheScene = 1;
        attackCooldown = 0.5f;
        canAttack = true;
        attackAnim = false;
        moveSpeed = 2.5f; // Oyuncunun hareket hýzý
        stepSize = 0.05f;
        isMoving = false;
        experience = 0;
        expBar.SetMaxExp(10);
        expBar.SetExp(experience);
        level = 1;
        damage = 6;
        maxHealth = 100;
        spiritNum = 15;
        healthBar.SetMaxHealth(maxHealth);
        healthBar.SetHealth(15);
        continousLevelBorder = 10;

        if (levelIndicator != null)
        {
            levelIndicator.text = level.ToString(); 
        }
    }

    private void Awake()
    {
        Instance = this; // Tek bir PlayerController olmasýný saðlýyoruz
        animator = GetComponent<Animator>(); // Animator bileþenini al
    }

    public void HandleUpdate()
    {

        if (spiritNum <= 0)
        {
            Die();
            Time.timeScale = 0f;
            DeadPanel.SetActive(true);

            DeadPanelScore.text = "Score: " + (level * experience).ToString();

            return;
        }

        if (!isMoving)
        {
            // Oyuncunun hareket giriþini al
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");


            // Hareket deðerlerini adým büyüklüðüne göre ölçekle
            input.x = stepSize * input.x;
            input.y = stepSize * input.y;

            // Çapraz hareketi önlemek için bir ekseni sýfýrla
            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {
                if (Input.GetKey(KeyCode.Space))//(space öncelikli!)
                {
                    // Eðer Space tuþu basýlýysa yön bilgisi güncellenmeyecek
                }
                else if(isAttacking)
                {
                    //Eðer baþarýlý saldýrý yapýlýyorsa düþmana dönük kalmaya zorlanýlýr
                    // Animatöre yön bilgisini gönder - saldýrý halinde düþmana dönük yürünerek sinematik bir his verilmesi amaçlandý
                }
                else
                {
                    animator.SetFloat("moveX", input.x);
                    animator.SetFloat("moveY", input.y);
                }
                // Hedef pozisyonu hesapla
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                // Eðer hedef pozisyon yürünebilir bir alansa, hareket ettir
                if (IsWalkable(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }

            }
        }

        // Eðer oyuncu "F" tuþuna bastýysa, etkileþime gir
        if (Input.GetKeyDown(KeyCode.F))
        {
            Interact();
        }

        // Eðer oyuncu "E" tuþuna bastýysa ve saldýrý zaten yapýlmýyorsa saldýrý hamlesi yap
        if (Input.GetKeyDown(KeyCode.E) && !attackAnim)
        {
            StartCoroutine(Attack(attackCooldown));
        }

        if (experience >= continousLevelBorder)
        {
            StartCoroutine(LevelPropertiesAdjustment());// level atlanýrsa iþlemler yapýlýr
        }

        animator.SetBool("isMoving", isMoving);
        animator.SetBool("attackAnim", attackAnim);
        UpdateHealthBar();
        UpdateExpBar();
    }
    private IEnumerator WalkSound()
    {
        walkSoundCheck = false;
        SoundFXManager.instance.PlaySoundFXClip(step1, transform, 1f);
        yield return new WaitForSeconds(0.2f);
        walkSoundCheck = true;
        yield break;
    }
    public IEnumerator LevelPropertiesAdjustment()
    {
        level++; 

        if (levelData.TryGetValue(level, out var data))
        {
            continousLevelBorder = data.expBorder;
            damage = data.damage;
            attackCooldown = data.attackCoolDown;
            maxHealth = data.maxHealth;
            healthBar.SetMaxHealth(maxHealth);
            expBar.SetMaxExp(continousLevelBorder);
        }
        else
        {
            if (level >= 6)//sonsuz seviye sistemi 6 dan sonra
            {
                if (experience >= continousLevelBorder)
                {
                    GenerateInfiniteLevels();
                    expBar.SetMaxExp(continousLevelBorder);
                }
            }
        }
        LevelUp?.Invoke();         

        yield break;
    }

    private void GenerateInfiniteLevels()
    {
        continousLevelBorder = (continousLevelBorder / 100) * 10 + 666 + experience; 
    }

    private void GetSpiritZonePositions()
    {
        BoundsInt bounds = spiritZoneTilemap.cellBounds;
        TileBase[] allTiles = spiritZoneTilemap.GetTilesBlock(bounds);

        for (int x = bounds.xMin; x < bounds.xMax; x++)
        {
            for (int y = bounds.yMin; y < bounds.yMax; y++)
            {
                Vector3Int cellPosition = new Vector3Int(x, y, 0);
                if (spiritZoneTilemap.HasTile(cellPosition))
                {
                    // Tilemap hücresini dünya koordinatýna çevir
                    Vector3 worldPos = spiritZoneTilemap.GetCellCenterWorld(cellPosition);
                    spiritZonePositions.Add(worldPos);
                }
            }
        }
    }
    private void UpdateHealthBar()
    {
        if(spiritNum > maxHealth)
        {
            spiritNum = maxHealth;
        }

        healthBar.SetHealth(spiritNum);
    }

    private void UpdateExpBar()
    {
        expBar.SetExp(experience);
        levelIndicator.text = level.ToString();
    }
    public void TakeDamage(int damage)
    {
        spiritNum -= damage;
        spiritNum = Mathf.Clamp(spiritNum, 0, maxHealth);// 0-100 limit
        healthBar.SetHealth(spiritNum);
    }

    public void Die()
    {
        /*
        if (isDead) return; // Eðer zaten öldüyse fonksiyondan çýk

        isMoving = true; // hareket engellendi
        isDead = true;
        animator.SetBool("isDead", isDead);*/
    }
    private IEnumerator LookAtTheEnemyYouHit(Collider2D collider)
    {       
        //vurduðumuz düþmana doðru otomatik bakma
        var enemyPos = collider.gameObject.transform.position;
        var enemyDir = transform.position - enemyPos;
        // Animatöre yön bilgisini gönder
        animator.SetFloat("moveX", -enemyDir.x);
        animator.SetFloat("moveY", -enemyDir.y);
        yield return new WaitForSeconds(2.0f);
        isAttacking = false;
        yield break;
    }
    public void Attack(int damage, Collider2D collider)
    {
        if (collider.tag == "Demon")
        {
            SoundFXManager.instance.PlaySoundFXClip(demonSlayedSound, transform, 1f);
        }
        else 
        {
            // Eðer düþman varsa düþmanýn hasar görme fonksiyonunu aktif et
            SoundFXManager.instance.PlaySoundFXClip(hit, transform, 1f);
        }

        collider.GetComponent<Warrior>()?.TakeDamage(damage);

    }
    private IEnumerator Attack(float attackCD)
    {
        if (!canAttack) yield break; // Eðer saldýrý yapýlamýyorsa çýk

        canAttack = false; // Saldýrý yapýldý, tekrar saldýrýyý engelle
        attackAnim = true;
        // Oyuncunun baktýðý yönü hesapla
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + (facingDir * 0.03f);
        var collider = Physics2D.OverlapCircle(interactPos, 0.15f, enemyLayer);

        yield return new WaitForSeconds(0.20f); // Animasyon süresi

        // Eðer bir collider bulunmuþsa
        if (collider != null)
        {
            isAttacking = true;          
            // Eðer trigger bir collider ise, oyuncu yönünü ona döndürsün
            StartCoroutine(LookAtTheEnemyYouHit(collider));
            Attack(damage, collider);           
        }
        attackAnim = false;// cooldowndan önce animasyon bitirilir, vuurþ animasyonu süresi kadar animasyon oynatýlýr
        yield return new WaitForSeconds(attackCD); // Cooldown süresi
        canAttack = true; // Yeniden saldýrý yapýlabilir
    }


    // Oyuncunun yöneldiði yerde bir etkileþim olup olmadýðýný kontrol eder
    void Interact()
    {
        // Oyuncunun baktýðý yönü hesapla
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + facingDir;

        // Etkileþimli nesne olup olmadýðýný kontrol et
        var collider = Physics2D.OverlapCircle(interactPos, 0.5f, interactableLayer);
        if (collider != null)
        {
            // Eðer etkileþimli bir nesne varsa, Interact() fonksiyonunu çaðýr
            collider.GetComponent<Interactable>()?.Interact();
        }
    }

    // Oyuncuyu belirli bir hedef konuma taþýr
    private IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        if (walkSoundCheck)
        {
            StartCoroutine(WalkSound());
        }

        // Oyuncu hedef konuma ulaþana kadar hareket eder
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Nihai konumu düzelt ve hareketi durdur
        transform.position = targetPos;
        isMoving = false;

        if (spiritNum < maxHealth)
        {
            // Rastgele ruh kontrolünü çalýþtýr
            CheckForSpirits();
        }
    }

    private bool IsWalkable(Vector3 targetPosition)
    {
        Collider2D collider = Physics2D.OverlapCircle(targetPosition, 0.08f, solidObjectsLayer | interactableLayer | doorsLayer);

        if (collider != null)
        {
            // Eðer çakýþan obje bir Spirit ise yürünebilir olarak kabul et
            if (collider.GetComponent<SpiritController>() != null)
            {
                return true;
            }

            return false; // Eðer baþka bir engel varsa yürünemez
        }

        return true; // Eðer çakýþma yoksa yürünebilir
    }


    // Melek heykeli etrafýnda çalýþacak ruh oluþturmayý kontrol eder
    private void CheckForSpirits()
    {
        if (spiritZonePositions.Count == 0) return;

        if (Physics2D.OverlapCircle(transform.position, 0.1f, spiritLayer) != null)
        { 
            if (UnityEngine.Random.Range(1, 101) <11) // %10 ihtimalle
            {
                
                // Rastgele bir konum seç
                Vector3 spawnPosition = spiritZonePositions[UnityEngine.Random.Range(0, spiritZonePositions.Count)];

                // O pozisyonda daha önce bir ruh spawn olmuþ mu kontrol et
                if (!spiritSpawnedPositions.Contains(spawnPosition) && spiritNumOnTheScene < 10)
                {
                    // Spirit oluþtur
                    Instantiate(spiritPrefab, spawnPosition, Quaternion.identity);
                    spiritNumOnTheScene++; 

                    // Yeni spawn edilen ruhun pozisyonunu kaydet
                    spiritSpawnedPositions.Add(spawnPosition);
                }
            }
        }
    }

    public void RemoveSpiritPosition(Vector3 position)
    {
        if (spiritSpawnedPositions.Contains(position))
        {
            spiritSpawnedPositions.Remove(position);
        }
    }

    public bool AnySoulsGathered()
    {
        
        return isGatheredAnySouls;
    }

}
