using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance;

    public PlayerKeybinds keybinds = new PlayerKeybinds();
    private PlayerControls controls;

    private Vector2 movementInput;
    private bool jumpPressed;
    private bool jumpHeld;
    private bool dashPressed;
    private bool attackPressed;
    private bool pausePressed;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            controls = new PlayerControls();
            SetupInputActions();
            LoadKeybinds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable() => controls?.Enable();
    private void OnDisable() => controls?.Disable();

    private void SetupInputActions()
    {
        controls.Gameplay.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += _ => movementInput = Vector2.zero;

        controls.Gameplay.Jump.started += _ => { jumpPressed = true; jumpHeld = true; };
        controls.Gameplay.Jump.canceled += _ => jumpHeld = false;

        controls.Gameplay.Dash.performed += _ => dashPressed = true;
        controls.Gameplay.Attack.performed += _ => attackPressed = true;

        controls.Gameplay.Pause.performed += _ => pausePressed = true;

        controls.Gameplay.Enable();
    }

    public bool GetJump()
    {
        bool result = jumpPressed;
        jumpPressed = false;
        return result;
    }

    public bool GetPause()
    {
        bool result = pausePressed;
        pausePressed = false;
        return result;
    }

    public bool GetJumpHeld() => jumpHeld;

    public bool GetDash()
    {
        bool result = dashPressed;
        dashPressed = false;
        return result;
    }

    public bool GetAttack()
    {
        bool result = attackPressed;
        attackPressed = false;
        return result;
    }

    public Vector2 GetMovement() => movementInput;

    public void SaveKeybinds()
    {
        PlayerPrefs.SetString("Jump", keybinds.jumpKey.ToString());
        PlayerPrefs.SetString("Dash", keybinds.dashKey.ToString());
        PlayerPrefs.SetString("Attack", keybinds.attackKey.ToString());
        PlayerPrefs.SetString("Up", keybinds.upKey.ToString());
        PlayerPrefs.SetString("Down", keybinds.downKey.ToString());
        PlayerPrefs.SetString("Left", keybinds.leftKey.ToString());
        PlayerPrefs.SetString("Right", keybinds.rightKey.ToString());
    }

    public void LoadKeybinds()
    {
        if (PlayerPrefs.HasKey("Jump"))
        {
            keybinds.jumpKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Jump"));
            keybinds.dashKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Dash"));
            keybinds.attackKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Attack"));
            keybinds.upKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Up"));
            keybinds.downKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Down"));
            keybinds.leftKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Left"));
            keybinds.rightKey = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Right"));
        }
    }
}
