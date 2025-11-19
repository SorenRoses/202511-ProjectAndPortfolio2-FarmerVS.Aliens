using UnityEngine;

public class cameraController : MonoBehaviour
{
    [SerializeField] float sens = 200f;
    [SerializeField] float lockVertMin = -70f, lockVertMax = 70f;
    [SerializeField] bool invertY = false;

    float rotX = 0f;
    Transform player;

    void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (transform.parent != null)
            player = transform.parent;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * sens * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * sens * Time.deltaTime;

        
        rotX += invertY ? mouseY : -mouseY;
        rotX = Mathf.Clamp(rotX, lockVertMin, lockVertMax);
        transform.localRotation = Quaternion.Euler(rotX, 0, 0);

        
        if (player != null)
        {
            Vector3 euler = player.rotation.eulerAngles;
            euler.y += mouseX;
            player.rotation = Quaternion.Euler(euler);
        }
    }
}
