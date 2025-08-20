
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Tilemaps;

using TMPro;

using UnityEngine.UI;
using UnityEngine.TextCore.Text;

// Oyuncunun kontrol�n� sa�layan s�n�f
public class PlayerController : MonoBehaviour, Warrior
{
    [SerializeField] public AudioClip step1;
    [SerializeField] public AudioClip hit;
    [SerializeField] public AudioClip playerDamaged;
    public TextMeshProUGUI levelIndicator; 
    public float moveSpeed; // Oyuncunun hareket h�z�
    public float stepSize;   // Oyuncunun her ad�mda hareket edece�i mesafe
    private bool isMoving;           // Oyuncunun hareket halinde olup olmad���n� kontrol eder
    private bool isAttacking;        // Sald�r� yap�lp yap�lmad���n� kontrol eder  
    private Vector2 input;           // Kullan�c�n�n girdi y�n�
    private Animator animator;       // Oyuncu animasyonlar�n� y�netmek i�in
    private int damage;               // Oyuncunun sald�r� g�c�
    public int experience;             //tecr�be puan�
    private bool dashDamage;
    public int spiritNum;            // Oyuncunun can de�eri,
    public int maxHealth;      // Max can de�eri
    public bool isGatheredAnySouls; //Soul ile etkile�ime girildi mi kontrol�

    public GameObject spiritPrefab;  // Ruh�uk prefab
    public GameObject heartEffectPrefab;//Can gitme efekti
    public GameObject zzzEffectPrefab; // CoolDown Efekti
    public int spiritNumOnTheScene;  // Sahnede olu�acak ruh say�s�n� limitlendirmemiz gerekir
    private List<Vector3> spiritSpawnedPositions = new List<Vector3>();

    public HealthBar healthBar;      //UI can bar� referans�
    public ExpBar expBar;           //UI expBar referans�
    public int level;           //oyun zorlu�una etkisi olacak, oyuncunun seviye durumu

    public Tilemap spiritZoneTilemap;  // Spirit'lerin ��kabilece�i Tilemap
    private List<Vector3> spiritZonePositions = new List<Vector3>();

    public LayerMask solidObjectsLayer;   // Engel katman� (duvarlar, a�a�lar vb.)
    public LayerMask doorsLayer;   // Engel katman� kap�
    public LayerMask interactableLayer;   // Etkile�imli nesneler katman� (NPC'ler vb.)
    public LayerMask spiritLayer;         // Rastgele spirit olu�turucak alanlar
    public LayerMask enemyLayer;         // Rastgele spirit olu�turucak alanlar
    private bool walkSoundCheck;

    private bool canAttack; // Sald�r� yap�l�p yap�lamayaca��n� kontrol eden de�i�ken
    private bool attackAnim;
    private bool dashAnim;
    private bool canDash;
    private float attackCooldown; // Sald�r� bekleme s�resi
    private float attackCooldownDash; // Sald�r� bekleme s�resi
    public bool isDead;
    public int instantScore;
    public event Action LevelUp; // Seviye al�nd���nda tetiklenecek olay


    public int continousLevelBorder;

    [SerializeField] public AudioClip demonSlayedSound;

    [SerializeField] public Text DeadPanelScore;
    [SerializeField] public GameObject DeadPanel;

    public static PlayerController Instance { get; private set; } // Singleton eri�imi

    private Dictionary<int, (int damage, int maxHealth, int expBorder, float attackCoolDown)> levelData = new()// levela g�re oyuncu �zelliklerini saklar�z
    {
        {2, (5, 110, 30, 0.4f)},    // MediumZombie level
        {3, (10, 120, 70, 0.2f)},    // HighZombie level
        {4, (20, 150, 100, 0.2f)},    // EvilSpirit level
        {5, (25, 200, 766, 0.1f)},  // Demon level
        {6, (30, 200, 1532, 0.0f)},  // Sonsuz seviyeye ge�i� s�n�r�
    };

    private void Start()
    {
        walkSoundCheck = true;
        GetSpiritZonePositions();//can ��kacak alanlar�n kaydedilmesi
        spiritNumOnTheScene = 1;
        attackCooldown = 0.5f;
        attackCooldownDash = 10f;
        canAttack = true;
        attackAnim = false;
        canDash = true;
        dashAnim = false;
        moveSpeed = 4f; // Oyuncunun hareket h�z�
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
        dashDamage = false;
        instantScore = 3;

        if (levelIndicator != null)
        {
            levelIndicator.text = level.ToString(); 
        }
    }

    private void Awake()
    {
        Instance = this; // Tek bir PlayerController olmas�n� sa�l�yoruz
        animator = GetComponent<Animator>(); // Animator bile�enini al
    }

    public void HandleUpdate()
    {

        if (spiritNum <= 0)
        {
            Die();
            return;
        }

        if (!isMoving)
        {
            // Oyuncunun hareket giri�ini al
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");


            // Hareket de�erlerini ad�m b�y�kl���ne g�re �l�ekle
            input.x = stepSize * input.x;
            input.y = stepSize * input.y;

            // �apraz hareketi �nlemek i�in bir ekseni s�f�rla
            if (input.x != 0) input.y = 0;

            if (input != Vector2.zero)
            {

                if(isAttacking || dashAnim)
                {
                    //E�er ba�ar�l� sald�r� yap�l�yorsa d��mana d�n�k kalmaya zorlan�l�r
                    // Animat�re y�n bilgisini g�nder - sald�r� halinde d��mana d�n�k y�r�nerek sinematik bir his verilmesi ama�land�
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

                // E�er hedef pozisyon y�r�nebilir bir alansa, hareket ettir
                if (IsWalkable(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }

            }
        }

        // E�er oyuncu "F" tu�una bast�ysa, etkile�ime gir
        if (Input.GetKeyDown(KeyCode.F))
        {
            Interact();
        }

        // E�er oyuncu "E" tu�una bast�ysa ve sald�r� zaten yap�lm�yorsa sald�r� hamlesi yap
        if (Input.GetKeyDown(KeyCode.E) && !attackAnim && !dashAnim)
        {           
            StartCoroutine(Attack(attackCooldown));
        }

        // E�er oyuncu "Space" tu�una bast�ysa ve sald�r� zaten yap�lm�yorsa s��arama ve sald�rma hamlesi yap
        if (Input.GetKeyDown(KeyCode.Space) && !attackAnim && !dashAnim && !isMoving)
        {           
            StartCoroutine(DashAttack(attackCooldownDash));
        }

        if (experience >= continousLevelBorder)
        {
            StartCoroutine(LevelPropertiesAdjustment());// level atlan�rsa i�lemler yap�l�r
        }

        animator.SetBool("isMoving", isMoving);
        animator.SetBool("attackAnim", attackAnim);
        animator.SetBool("isDash", dashAnim);
        UpdateHealthBar();
        UpdateExpBar();
    }
    public void Attack(int damage, Collider2D collider)
    {
        if (collider.tag == "Demon")
        {
            SoundFXManager.instance.PlaySoundFXClip(demonSlayedSound, transform, 1f);
        }
        else
        {
            // E�er d��man varsa d��man�n hasar g�rme fonksiyonunu aktif et
            SoundFXManager.instance.PlaySoundFXClip(hit, transform, 1f);
        }

        collider.GetComponent<Warrior>()?.TakeDamage(damage);

    }
    private IEnumerator Attack(float attackCD)
    {
        if (!canAttack) yield break; // E�er sald�r� yap�lam�yorsa ��k

        canAttack = false; // Sald�r� yap�ld�, tekrar sald�r�y� engelle
        attackAnim = true;
        // Oyuncunun bakt��� y�n� hesapla
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + (facingDir * 0.03f);
        var collider = Physics2D.OverlapCircle(interactPos, 0.15f, enemyLayer);

        yield return new WaitForSeconds(0.20f); // Animasyon s�resi

        // E�er bir collider bulunmu�sa
        if (collider != null)
        {
            isAttacking = true;
            // E�er trigger bir collider ise, oyuncu y�n�n� ona d�nd�rs�n
            StartCoroutine(LookAtTheEnemyYouHit(collider));
            if (dashDamage)
            {
                Attack(300, collider);
            }
            else
            {
                Attack(damage, collider);
            }

        }
        dashDamage = false;
        attackAnim = false;// cooldowndan �nce animasyon bitirilir, vuur� animasyonu s�resi kadar animasyon oynat�l�r
        yield return new WaitForSeconds(attackCD); // Cooldown s�resi
        canAttack = true; // Yeniden sald�r� yap�labilir
    }
    private IEnumerator DashAttack(float dashCD)
    {
        if (!canAttack) yield break; // E�er sald�r� yap�lam�yorsa ��k

        if (!canDash) 
        {
            Instantiate(zzzEffectPrefab, transform.position + new Vector3(0.2f, 1f, 0f), Quaternion.identity);
            yield break; 
        } // E�er sald�r� yap�lam�yorsa ��k

        canDash = false;
        canAttack = false; // Sald�r� yap�ld�, tekrar sald�r�y� engelle
        dashAnim = true;
        animator.SetBool("isDash", dashAnim);
        dashDamage = true;
        // Oyuncunun bakt��� y�n� hesapla
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));

        float dashDistance = 3f;
        Vector2 direction = facingDir.normalized; // Karakterin bakt��� y�n
        Vector2 origin = transform.position;

        // AN�MAT�R G�NCELLEMES�
        animator.SetFloat("moveX", direction.x);
        animator.SetFloat("moveY", direction.y);
        isMoving = true;
        animator.SetBool("isMoving", isMoving);


        RaycastHit2D hit = Physics2D.Raycast(origin, direction, dashDistance, solidObjectsLayer | interactableLayer | doorsLayer);

        Vector2 targetPos;

        if (hit.collider != null)
        {
            // Engel varsa, biraz �n�nde dur
            targetPos = hit.point - direction * 0.4f; // Duvara yap��mas�n
        }
        else
        {
            // Engel yoksa maksimum mesafeye kadar dash at
            targetPos = origin + direction * dashDistance;
        }

        // Oyuncu hedef konuma ula�ana kadar hareket eder
        while ((targetPos - (Vector2)transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, 10f * Time.deltaTime);
            yield return null;
        }

        // Nihai konumu d�zelt ve hareketi durdur
        transform.position = targetPos;
        dashAnim = false;
        yield return new WaitForSeconds(0.01f); // izin s�resi
        isMoving = false;
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isDash", dashAnim);
        canAttack = true;
        StartCoroutine(Attack(attackCooldown));

        yield return new WaitForSeconds(dashCD); // Cooldown s�resi
        canDash = true;
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
                    // Tilemap h�cresini d�nya koordinat�na �evir
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
        Instantiate(heartEffectPrefab,transform.position + new Vector3(-0.2f, 1f, 0f), Quaternion.identity);
        SoundFXManager.instance.PlaySoundFXClip(playerDamaged, transform, 1.5f);
        spiritNum -= damage;
        spiritNum = Mathf.Clamp(spiritNum, 0, maxHealth);// 0-100 limit
        healthBar.SetHealth(spiritNum);
    }

    public void Die()
    {
        Time.timeScale = 0f;
        DeadPanel.SetActive(true);
        double pieceScoreDouble = level / 7;
        if(pieceScoreDouble <= 0 ) pieceScoreDouble = 1;
        int pieceScore = (int)pieceScoreDouble;
        int theScore =  (pieceScore * experience) + 3;
        instantScore = theScore;
        DeadPanelScore.text = "Score: " + theScore.ToString();
    }
    private IEnumerator LookAtTheEnemyYouHit(Collider2D collider)
    {       
        //vurdu�umuz d��mana do�ru otomatik bakma
        var enemyPos = collider.gameObject.transform.position;
        var enemyDir = transform.position - enemyPos;
        // Animat�re y�n bilgisini g�nder
        animator.SetFloat("moveX", -enemyDir.x);
        animator.SetFloat("moveY", -enemyDir.y);
        yield return new WaitForSeconds(2.0f);
        isAttacking = false;
        yield break;
    }

    // Oyuncunun y�neldi�i yerde bir etkile�im olup olmad���n� kontrol eder
    void Interact()
    {
        // Oyuncunun bakt��� y�n� hesapla
        var facingDir = new Vector3(animator.GetFloat("moveX"), animator.GetFloat("moveY"));
        var interactPos = transform.position + facingDir;

        // Etkile�imli nesne olup olmad���n� kontrol et
        var collider = Physics2D.OverlapCircle(interactPos, 0.5f, interactableLayer);
        if (collider != null)
        {
            // E�er etkile�imli bir nesne varsa, Interact() fonksiyonunu �a��r
            collider.GetComponent<Interactable>()?.Interact();
        }
    }

    // Oyuncuyu belirli bir hedef konuma ta��r
    private IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        if (walkSoundCheck)
        {
            StartCoroutine(WalkSound());
        }

        // Oyuncu hedef konuma ula�ana kadar hareket eder
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // Nihai konumu d�zelt ve hareketi durdur
        transform.position = targetPos;
        isMoving = false;

        if (spiritNum < maxHealth)
        {
            // Rastgele ruh kontrol�n� �al��t�r
            CheckForSpirits();
        }
    }

    private bool IsWalkable(Vector3 targetPosition)
    {
        Collider2D collider = Physics2D.OverlapCircle(targetPosition, 0.08f, solidObjectsLayer | interactableLayer | doorsLayer);

        if (collider != null)
        {
            // E�er �ak��an obje bir Spirit ise y�r�nebilir olarak kabul et
            if (collider.GetComponent<SpiritController>() != null)
            {
                return true;
            }

            return false; // E�er ba�ka bir engel varsa y�r�nemez
        }

        return true; // E�er �ak��ma yoksa y�r�nebilir
    }


    // Melek heykeli etraf�nda �al��acak ruh olu�turmay� kontrol eder
    private void CheckForSpirits()
    {
        if (spiritZonePositions.Count == 0) return;

        if (Physics2D.OverlapCircle(transform.position, 0.1f, spiritLayer) != null)
        { 
            if (UnityEngine.Random.Range(1, 101) <11) // %10 ihtimalle
            {
                
                // Rastgele bir konum se�
                Vector3 spawnPosition = spiritZonePositions[UnityEngine.Random.Range(0, spiritZonePositions.Count)];

                // O pozisyonda daha �nce bir ruh spawn olmu� mu kontrol et
                if (!spiritSpawnedPositions.Contains(spawnPosition) && spiritNumOnTheScene < 10)
                {
                    // Spirit olu�tur
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
