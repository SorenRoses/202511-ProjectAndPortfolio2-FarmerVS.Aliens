using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class gamemanager : MonoBehaviour
{
    public static gamemanager instance;

    
    [SerializeField] private GameObject menuActive;
    [SerializeField] private GameObject menuPause;
    [SerializeField] private GameObject menuWin;
    [SerializeField] private GameObject menuLose;

    
    [SerializeField] private TMP_Text gameGoalCountText; // 
    [SerializeField] private TMP_Text cowsAliveText;    
    [SerializeField] private Image playerHPBar;
    [SerializeField] private GameObject playerDamagePanel;

    
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerController controller;

    public bool isPaused { get; private set; }

    private float timeScaleOrig;
    private int gameGoalCount;
    private int cowsAlive; 

    public GameObject Player => player;
    public Image PlayerHPBar => playerHPBar;
    public GameObject PlayerDamagePanel => playerDamagePanel;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        controller = player != null ? player.GetComponent<PlayerController>() : null;

        menuPause?.SetActive(false);
        menuWin?.SetActive(false);
        menuLose?.SetActive(false);
        menuActive = null;

        isPaused = false;
    }

    void Start()
    {
        
        UpdateGameGoalUI();

        
        GameObject[] cows = GameObject.FindGameObjectsWithTag("Cow");
        cowsAlive = cows.Length;
        UpdateCowUI();
    }

    void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            if (menuActive == null)
            {
                statePause();
                menuActive = menuPause;
                menuActive?.SetActive(true);
            }
            else if (menuActive == menuPause)
            {
                stateUnpause();
            }
        }
    }

    
    public void statePause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void stateUnpause()
    {
        isPaused = false;
        Time.timeScale = timeScaleOrig;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        if (menuActive != null)
        {
            menuActive.SetActive(false);
            menuActive = null;
        }
    }

    
    public void updateGameGoal(int amount)
    {
        gameGoalCount += amount;
        UpdateGameGoalUI();

        if (gameGoalCount <= 0)
        {
            statePause();
            menuActive = menuWin;
            menuActive?.SetActive(true);
        }
    }

    private void UpdateGameGoalUI()
    {
        if (gameGoalCountText != null)
            gameGoalCountText.text = gameGoalCount.ToString("F0");
    }

    
    public void UpdateCowCount(int amount)
    {
        cowsAlive += amount;
        UpdateCowUI();

        if (cowsAlive <= 0)
        {
            youLose();
        }
    }

    private void UpdateCowUI()
    {
        if (cowsAliveText != null)
            cowsAliveText.text = cowsAlive.ToString("F0");
    }

   
    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive?.SetActive(true);
    }
}