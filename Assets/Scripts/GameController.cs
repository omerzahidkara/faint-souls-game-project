
using System.Collections;

using UnityEngine;

// Oyunun genel durumlarýný belirten enum
public enum GameState
{
    FreeRoam,  // Serbest dolaþým modu
    Dialog,    // Diyalog modu
}

public class GameController : MonoBehaviour
{
    [SerializeField] AudioSource musicPrapareBattle;
    [SerializeField] GameObject pauseMenu;//esc basýýlýnca aktif olacak pause menu
    [SerializeField] GameObject optionsPanel;
    [SerializeField] GameObject deadPanel;
    [SerializeField] public AudioClip enemyWaveLaunched;
    [SerializeField] GameObject infoPanel; 
    [SerializeField] GameObject TipsPanel;
    public static bool IsInputBlocked = false;
    // düþman ai referanslarý
    public GameObject lowZombie;  // LowZombie prefab referansý
    public GameObject mediumZombie;  // mediumZombie prefab referansý
    public GameObject highZombie;  // highZombie prefab referansý
    public GameObject evilSpirit;  // evilSpirit prefab referansý
    public GameObject demon;  // demon prefab referansý
    public int demonCount; //sahnedeki aktif demon sayýsý

    [SerializeField] NPCController npcController; // Diyalog bitince deðiþime uðrayacak sisterNPC referansý
    [SerializeField] public Sprite deadSprite; // Ölü NPC için kullanýlacak sprite
    private SpriteRenderer spriteRenderer; // Mevcut sprite renderer
    GameState state; // Mevcut oyun durumu
    private bool levelState;

    public GameObject pos1, pos2, pos3, pos4; // pozisyon referanslarý
    Vector3 spawnPosition1, spawnPosition2, spawnPosition3, spawnPosition4; // kapý pozisyonlarýndan düþmanlar gelecek
    public static GameController Instance { get; private set; } // Singleton eriþimi
    private Coroutine infiniteSpawnCoroutine = null;
    private void Awake()
    {
        Instance = this; // Tek bir Controller olmasýný saðlýyoruz
    }

    public void Start()
    {
        demonCount = 0;

        QualitySettings.vSyncCount = 0; // VSync'i tamamen kapatýr
        Application.targetFrameRate = 120;

        Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked;// Fare gizlenir
        levelState = true;
        state = GameState.FreeRoam;

        spawnPosition1 = pos1.transform.position;
        spawnPosition2 = pos2.transform.position;
        spawnPosition3 = pos3.transform.position;
        spawnPosition4 = pos4.transform.position;// kapý posizyonlarý empty obj vasýtasýyla belirlenir

        spriteRenderer = npcController.GetComponent<SpriteRenderer>();

        // DialogManager olaylarýna abone olarak oyunun durumunu deðiþtiriyoruz
        DialogManager.Instance.OnShowDialog += () =>
        {
            state = GameState.Dialog; // Diyalog baþladýðýnda oyun durumu deðiþiyor
        };

        DialogManager.Instance.OnHideDialog += () =>
        {
            if (state == GameState.Dialog)
            {
                state = GameState.FreeRoam; // Diyalog bittiðinde serbest dolaþýma geç
            }
        };

        PlayerController.Instance.LevelUp += () =>
        {
            levelState = true;
        };
        Time.timeScale = 0f;
        TipsPanel.SetActive(true);
    }
    public void Update()
    {
        if (PlayerController.Instance.AnySoulsGathered())
        {
            if(!musicPrapareBattle.isPlaying)
            {
                musicPrapareBattle.Play();
            }
        }/*
        //menüler açýksa fare devrede
        if (pauseMenu.activeSelf || optionsPanel.activeSelf || infoPanel.activeSelf || deadPanel.activeSelf ||TipsPanel.activeSelf)
        {
            Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked;// Fare kapanýr
        }*/
        //options ve info kapalýysa 
        if(!IsInputBlocked)
        {
            if (Input.GetKeyDown(KeyCode.Escape))//pause menu açma kapama koþullarý
            {
                if (pauseMenu.activeSelf)
                {
                    Cursor.visible = false; Cursor.lockState = CursorLockMode.Locked;// Fare kapanýr
                    pauseMenu.SetActive(false);
                    Time.timeScale = 1f;
                }
                else
                {
                    Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
                    pauseMenu.SetActive(true);
                    Time.timeScale = 0f;
                }
            }
        }
        if (deadPanel.activeSelf)
        {
            Cursor.visible = true; Cursor.lockState = CursorLockMode.None;
        }

        // Mevcut duruma göre ilgili kontrolcüyü çalýþtýr
        if (state == GameState.FreeRoam)
        {
            PlayerController.Instance.HandleUpdate(); // Oyuncu hareketlerini yönet
            SisterHasGone(PlayerController.Instance.AnySoulsGathered());

        }
        else if (state == GameState.Dialog)
        {
            DialogManager.Instance.HandleUpdate(); // Diyalog giriþlerini yönet
        }
    }

    public void SisterHasGone(bool hasGone)// Soul toplandýðý anda kýz npc ölmüþ olacak
    {
        if (hasGone) 
        {
            // NPC'nin sprite'ýný deðiþtir
            spriteRenderer.sprite = deadSprite;  // mezar sprite ile deðiþtir

            // Layer'ý deðiþtir
            npcController.gameObject.layer = LayerMask.NameToLayer("SolidObjects");//katý obje katmanýna dönüþtür          
            if(levelState)
            {
                EnemySpawner();// saldýrý baþlar
            }
        }
    }

    private void EnemySpawner()
    {
        levelState = false;// level atlayana kadar kendini tekrar çaðýrmamalý

        int level = PlayerController.Instance.level;
        if (level < 0)
        {
            Debug.LogError("Seviye negatif olamaz!");
        }
        else if (level == 1)//10 lowzombie = 10xp+
        {
            StartCoroutine(Level1Enemies());
            SoundFXManager.instance.PlaySoundFXClip(enemyWaveLaunched, transform, 1f);
        }
        else if (level == 2)//5 low, 5 medium  = 20xp+
        {
            StartCoroutine(Level2Enemies());
            SoundFXManager.instance.PlaySoundFXClip(enemyWaveLaunched, transform, 1f);
        }
        else if (level == 3)//4 high, 4 medium, 8 low = 40xp+
        {
            StartCoroutine(Level3Enemies());
            SoundFXManager.instance.PlaySoundFXClip(enemyWaveLaunched, transform, 1f);
        }
        else if (level == 4)//3 evil = 30xp+
        {
            StartCoroutine(Level4Enemies());
            SoundFXManager.instance.PlaySoundFXClip(enemyWaveLaunched, transform, 1f);
        }
        else if (level == 5)//1 demon = 666xp+
        {
            Instantiate(demon, spawnPosition4, Quaternion.identity);
            SoundFXManager.instance.PlaySoundFXClip(enemyWaveLaunched, transform, 1f);
        }
        else
        {
            if (infiniteSpawnCoroutine == null) // Eðer daha önce baþlatýlmadýysa
            {
                infiniteSpawnCoroutine = StartCoroutine(InfiniteEnemySpawn());
            }
        }
    }

    private IEnumerator InfiniteEnemySpawn()
    {
        int demonMultiplier = (PlayerController.Instance.continousLevelBorder / 8000) + 2;
        //continousLevelBorder = (continousLevelBorder / 100) * 10 + 666 + experience; 
        while (PlayerController.Instance.experience < PlayerController.Instance.continousLevelBorder)
        {
            SoundFXManager.instance.PlaySoundFXClip(enemyWaveLaunched, transform, 1.2f);
            for (int i = 0; i < (PlayerController.Instance.continousLevelBorder/250)+5; i++) 
            {
                SpawnPosRandomizer();
                int decidingNum = UnityEngine.Random.Range(1, 31); // 1 ile 30 arasýnda rastgele bir sayý seçilir.

                while (demonMultiplier > 0)
                {
                    if (demonCount >= 8)
                    {
                        demonMultiplier = 0;
                    }
                    if (demonMultiplier > 8)
                    {
                        demonMultiplier = 8;
                    }

                    SpawnPosRandomizer();
                    yield return new WaitForSeconds(1.75f);
                    Instantiate(demon, spawnPosition4, Quaternion.identity);
                    if (demonMultiplier > 0)
                    {
                        demonMultiplier--;
                    }
                }

                switch (decidingNum)
                {
                    case 1:
                    case 2:
                    case 3:
                    case 4:
                    case 5:
                    case 6:
                    case 7:
                    case 8:
                    case 9:
                    case 10:
                        Instantiate(lowZombie, spawnPosition1, Quaternion.identity);
                        break;
                    case 11:
                    case 12:
                    case 13:
                    case 14:
                    case 15:
                    case 16:
                        Instantiate(mediumZombie, spawnPosition2, Quaternion.identity);
                        break;
                    case 17:
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                    case 23:
                    case 24:
                    case 25:
                        Instantiate(highZombie, spawnPosition3, Quaternion.identity);
                        break;
                    case 26:
                    case 27:
                    case 28:
                    case 29:
                    case 30:
                        Instantiate(evilSpirit, spawnPosition4, Quaternion.identity);
                        break;          
                }
                yield return new WaitForSeconds(1.2222f);
                if(PlayerController.Instance.experience > PlayerController.Instance.continousLevelBorder)
                {
                    yield break;
                }
                if (i >= 50) // 40 düþmandan fazlasý olduðunda beklemeye girilecek
                {
                    yield return new WaitForSeconds(16f);
                    break;
                }
                if(demonMultiplier < 2 && demonCount < 2)
                {
                    demonMultiplier++;
                }
            }//FOR END

            if (PlayerController.Instance.level < 10)
            {
                yield return new WaitForSeconds(5f);
            }
            else
            {
                yield return new WaitForSeconds(10f);
            }

        }
        yield return new WaitForSeconds(10f);
        yield break;
    }

    public void SpawnPosRandomizer()
    {
        int decidingNum = UnityEngine.Random.Range(1, 5); // 1 ile 5 arasýnda rastgele bir sayý seçilir.

        switch (decidingNum)
        {
            case 1:
                spawnPosition1 = pos4.transform.position;
                spawnPosition2 = pos3.transform.position;
                spawnPosition3 = pos2.transform.position;
                spawnPosition4 = pos1.transform.position;
                break;

            case 2:
                spawnPosition1 = pos1.transform.position;
                spawnPosition2 = pos3.transform.position;
                spawnPosition3 = pos2.transform.position;
                spawnPosition4 = pos4.transform.position;
                break;

            case 3:
                spawnPosition1 = pos2.transform.position;
                spawnPosition2 = pos3.transform.position;
                spawnPosition3 = pos4.transform.position;
                spawnPosition4 = pos1.transform.position;
                break;

            case 4:
                spawnPosition1 = pos1.transform.position;
                spawnPosition2 = pos4.transform.position;
                spawnPosition3 = pos3.transform.position;
                spawnPosition4 = pos2.transform.position;
                break;

            default:
                spawnPosition1 = pos1.transform.position;
                spawnPosition2 = pos2.transform.position;
                spawnPosition3 = pos3.transform.position;
                spawnPosition4 = pos4.transform.position;
                break;
        }
    }
    private IEnumerator Level1Enemies()
    {
        for(int i = 0; i < 5;  i++)
        {
            GameObject enemy = Instantiate(lowZombie, spawnPosition1, Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(4f);
        for (int i = 0; i < 5; i++)
        {
            GameObject enemy = Instantiate(lowZombie, spawnPosition1, Quaternion.identity);
            yield return new WaitForSeconds(0.5f);
        }
        yield break;
    }
    private IEnumerator Level2Enemies()
    {
        for (int i = 0; i < 5; i++)
        {
            Instantiate(lowZombie, spawnPosition1, Quaternion.identity);
            Instantiate(mediumZombie, spawnPosition2, Quaternion.identity);
            yield return new WaitForSeconds(2f);
        }
        yield break;
    }
    private IEnumerator Level3Enemies()
    {
        for (int i = 0; i < 3; i++)
        {
            Instantiate(lowZombie, spawnPosition1, Quaternion.identity);
            Instantiate(lowZombie, spawnPosition3, Quaternion.identity);
            Instantiate(mediumZombie, spawnPosition1, Quaternion.identity);
            Instantiate(highZombie, spawnPosition2, Quaternion.identity);
            yield return new WaitForSeconds(3f);
        }
        yield return new WaitForSeconds(6f);
        Instantiate(lowZombie, spawnPosition1, Quaternion.identity);
        Instantiate(lowZombie, spawnPosition3, Quaternion.identity);
        Instantiate(mediumZombie, spawnPosition1, Quaternion.identity);
        yield return new WaitForSeconds(10f);
        Instantiate(highZombie, spawnPosition2, Quaternion.identity);
        yield break;
    }
    private IEnumerator Level4Enemies()
    {
        for (int i = 0; i < 3; i++)
        {
            Instantiate(evilSpirit, spawnPosition4, Quaternion.identity);
            yield return new WaitForSeconds(5f);
        }
        yield break;
    }
}