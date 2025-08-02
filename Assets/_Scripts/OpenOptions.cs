using UnityEngine;
using UnityEngine.InputSystem;

public class OpenOptions : MonoBehaviour
{
    [Header("Options Menu")]
    [SerializeField] private GameObject optionsPanel;

    private bool isOptionsOpen = false;
    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();
        inputActions.UI.ToggleOptions.performed += ctx => ToggleOptionsMenu();
    }

    private void OnEnable()
    {
        inputActions.Enable();
    }

    private void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(false);
    }

    private void ToggleOptionsMenu()
    {
        isOptionsOpen = !isOptionsOpen;

        if (optionsPanel != null)
            optionsPanel.SetActive(isOptionsOpen);

        Time.timeScale = isOptionsOpen ? 0f : 1f;
        Cursor.visible = isOptionsOpen;
        Cursor.lockState = isOptionsOpen ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void CloseOptionsMenu() // call this on your Back button
    {
        isOptionsOpen = false;

        if (optionsPanel != null)
            optionsPanel.SetActive(false);

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
