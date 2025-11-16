using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class CharacterPanel : MonoBehaviour
{
    public GameObject character;
    public Transform weaponsPanel;
    public Transform actionsPanel;
    public Transform camerasPanel;
    public Button buttonPrefab;
    public Slider motionSpeed;

    private Actions actions;
    private PlayerController controller;
    private Camera[] cameras;

    void Start()
    {
        Initialize();
    }

    void Initialize()
    {
        if (character == null)
        {
            Debug.LogError("Character reference is missing.");
            return;
        }

        actions = character.GetComponent<Actions>();
        if (actions == null)
        {
            Debug.LogWarning("Actions component not found on character.");
        }

        controller = character.GetComponent<PlayerController>();
        if (controller == null)
        {
            Debug.LogWarning("PlayerController component not found on character.");
        }

        // Clear previous buttons if any (optional safety)
        foreach (Transform child in weaponsPanel) Destroy(child.gameObject);
        foreach (Transform child in actionsPanel) Destroy(child.gameObject);
        foreach (Transform child in camerasPanel) Destroy(child.gameObject);

        if (controller != null && controller.arsenal != null)
        {
            foreach (PlayerController.Arsenal a in controller.arsenal)
                CreateWeaponButton(a.name);
        }

        CreateActionButtonSimple("Stay");
        CreateActionButtonSimple("Walk");
        CreateActionButtonSimple("Run");
        CreateActionButtonSimple("Sitting");
        CreateActionButtonSimple("Jump");
        CreateActionButtonSimple("Aiming");
        CreateActionButtonSimple("Attack");
        CreateActionButtonSimple("Damage");
        CreateActionButtonWithMessage("Death Reset", "Death");

        // Use FindObjectsByType with FindObjectsSortMode.None for better performance as recommended
        cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
        var sortedCameras = cameras.OrderBy(c => c.name);

        foreach (Camera c in sortedCameras)
            CreateCameraButton(c);

        // Auto-select first camera button if any
        if (camerasPanel.childCount > 0)
        {
            Button firstCamButton = camerasPanel.GetChild(0).GetComponent<Button>();
            if (firstCamButton != null)
                firstCamButton.onClick.Invoke();
        }
    }

    void CreateWeaponButton(string name)
    {
        Button button = CreateButton(name, weaponsPanel);
        if (controller != null)
            button.onClick.AddListener(() => controller.SetArsenal(name));
    }

    void CreateActionButtonSimple(string name)
    {
        CreateActionButtonWithMessage(name, name);
    }

    void CreateActionButtonWithMessage(string name, string message)
    {
        Button button = CreateButton(name, actionsPanel);
        if (actions != null)
            button.onClick.AddListener(() => actions.SendMessage(message, SendMessageOptions.DontRequireReceiver));
        else
            Debug.LogWarning($"Actions component missing, cannot send message '{message}'");
    }

    void CreateCameraButton(Camera c)
    {
        Button button = CreateButton(c.name, camerasPanel);
        button.onClick.AddListener(() => ShowCamera(c));
    }

    Button CreateButton(string name, Transform group)
    {
        GameObject obj = Instantiate(buttonPrefab.gameObject, group);
        obj.name = name;
        obj.transform.localScale = Vector3.one;
        Text text = obj.transform.GetChild(0).GetComponent<Text>();
        if (text != null)
            text.text = name;
        else
            Debug.LogWarning("Button prefab missing Text component in first child.");
        return obj.GetComponent<Button>();
    }

    void ShowCamera(Camera cam)
    {
        if (cameras == null) return;

        foreach (Camera c in cameras)
            c.gameObject.SetActive(c == cam);
    }

    void Update()
    {
        Time.timeScale = motionSpeed.value;
    }

    public void OpenPublisherPage()
    {
        Application.OpenURL("https://www.assetstore.unity3d.com/en/#!/publisher/11008");
    }
}
