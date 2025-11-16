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

    [SerializeField] private TMP_Text gameGoalCountText;
    [SerializeField] private Image playerHPBar;
    [SerializeField] private GameObject playerDamagePanel;

    [SerializeField] private GameObject player;
    [SerializeField] private playerController controller;
    public GameObject cow;

    public bool isPaused { get; private set; }

    private float timeScaleOrig;
    private int gameGoalCount;

    // Provide public read-only properties to expose private fields
    public GameObject Player => player;
    public Image PlayerHPBar => playerHPBar;
    public GameObject PlayerDamagePanel => playerDamagePanel;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        timeScaleOrig = Time.timeScale;

        player = GameObject.FindWithTag("Player");
        cow = GameObject.FindWithTag("Cow");
        controller = player != null ? player.GetComponent<playerController>() : null;

        // Hide all menus on start
        menuPause?.SetActive(false);
        menuWin?.SetActive(false);
        menuLose?.SetActive(false);
        menuActive = null;

        isPaused = false;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
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
        if (gameGoalCountText != null)
        {
            gameGoalCountText.text = gameGoalCount.ToString("F0");
        }

        if (gameGoalCount <= 0)
        {
            // You Win!
            statePause();
            menuActive = menuWin;
            menuActive?.SetActive(true);
        }
    }

    public void youLose()
    {
        statePause();
        menuActive = menuLose;
        menuActive?.SetActive(true);
    }
}