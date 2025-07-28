using UnityEngine;

public class PlayerInputManager : MonoBehaviour
{
    public static PlayerInputManager Instance;

    public PlayerKeybinds keybinds = new PlayerKeybinds();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadKeybinds();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public bool GetJump() => Input.GetKeyDown(keybinds.jumpKey);
    public bool GetJumpHeld() => Input.GetKey(keybinds.jumpKey);
    public bool GetDash() => Input.GetKeyDown(keybinds.dashKey);
    public bool GetAttack() => Input.GetKeyDown(keybinds.attackKey);

    public Vector2 GetMovement()
    {
        float x = (Input.GetKey(keybinds.leftKey) ? -1f : 0f) + (Input.GetKey(keybinds.rightKey) ? 1f : 0f);
        float y = (Input.GetKey(keybinds.downKey) ? -1f : 0f) + (Input.GetKey(keybinds.upKey) ? 1f : 0f);
        return new Vector2(x, y);
    }

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
