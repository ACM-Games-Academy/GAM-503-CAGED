using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KeybindUI : MonoBehaviour
{
    [Header("Buttons (assign from Hierarchy)")]
    [SerializeField] private Button jumpButton;
    [SerializeField] private Button dashButton;
    [SerializeField] private Button attackButton;

    // These will be auto-fetched from the buttons
    private Text jumpKeyText;
    private Text dashKeyText;
    private Text attackKeyText;

    private void Awake()
    {
        // Auto get the Text components from the buttons
        if (jumpButton) jumpKeyText = jumpButton.GetComponentInChildren<Text>(true);
        if (dashButton) dashKeyText = dashButton.GetComponentInChildren<Text>(true);
        if (attackButton) attackKeyText = attackButton.GetComponentInChildren<Text>(true);

        // Basic validation
        if (!jumpButton || !dashButton || !attackButton)
            Debug.LogError("KeybindUI: Please assign Jump/Dash/Attack Button references in the Inspector (from the Hierarchy).");

        if (!jumpKeyText || !dashKeyText || !attackKeyText)
            Debug.LogError("KeybindUI: Could not find Text component on one of the Buttons. Make sure each Button has a child Text.");
    }

    private void OnEnable()
    {
        if (PlayerInputManager.Instance == null)
        {
            Debug.LogError("KeybindUI: PlayerInputManager not found in scene!");
            return;
        }

        RefreshUI();
    }

    // Hook these 3 methods from the Buttons' OnClick (target the Controls Panel in the HIERARCHY)
    public void StartRebindJump() { StartCoroutine(WaitForKey(Action.Jump)); }
    public void StartRebindDash() { StartCoroutine(WaitForKey(Action.Dash)); }
    public void StartRebindAttack() { StartCoroutine(WaitForKey(Action.Attack)); }

    private enum Action { Jump, Dash, Attack }

    private IEnumerator WaitForKey(Action action)
    {
        if (PlayerInputManager.Instance == null)
        {
            Debug.LogError("KeybindUI: PlayerInputManager not found.");
            yield break;
        }

        Text targetText = GetTextByAction(action);
        if (!targetText)
        {
            Debug.LogError($"KeybindUI: No Text found for {action}. Check button assignments.");
            yield break;
        }

        string original = targetText.text;
        targetText.text = "...";

        bool bound = false;
        while (!bound)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    var keys = PlayerInputManager.Instance.keybinds;

                    switch (action)
                    {
                        case Action.Jump: keys.jumpKey = key; break;
                        case Action.Dash: keys.dashKey = key; break;
                        case Action.Attack: keys.attackKey = key; break;
                    }

                    PlayerInputManager.Instance.SaveKeybinds();
                    RefreshUI();
                    bound = true;
                    break;
                }
            }
            yield return null;
        }
    }

    private void RefreshUI()
    {
        var pim = PlayerInputManager.Instance;
        if (pim == null) return;

        if (jumpKeyText) jumpKeyText.text = pim.keybinds.jumpKey.ToString();
        if (dashKeyText) dashKeyText.text = pim.keybinds.dashKey.ToString();
        if (attackKeyText) attackKeyText.text = pim.keybinds.attackKey.ToString();
    }

    private Text GetTextByAction(Action action)
    {
        switch (action)
        {
            case Action.Jump: return jumpKeyText;
            case Action.Dash: return dashKeyText;
            case Action.Attack: return attackKeyText;
            default: return null;
        }
    }
}
