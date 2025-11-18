using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class gamemanager : MonoBehaviour
{
    public static gamemanager instance;

    [SerializeField] private GameObject menuActive;
    [SerializeField] private GameObject menuPause;
    [SerializeField] private GameObject menuWin;
    [SerializeField] private GameObject menuLose;

    public TMP_Text gameGoalCountText;
    public Image playerHPBar;
    public GameObject playerDamagePanel;

    public GameObject player;
    public PlayerController controller;

    public List<GameObject> cows = new List<GameObject>();

    public bool isPaused;

    private float timeScaleOrig;
    private int gameGoalCount;

    
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
        controller = player?.GetComponent<PlayerController>();

        GameObject[] cowArray = GameObject.FindGameObjectsWithTag("Cow");
        cows = cowArray.ToList();


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

    public Transform[] GetCowTransforms()
    {
        return cows.Where(c => c != null).Select(c => c.transform).ToArray();
    }
}