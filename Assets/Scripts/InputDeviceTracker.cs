using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

[DefaultExecutionOrder(-1000)]
public class InputDeviceTracker : MonoBehaviour
{
    public enum DeviceType
    {
        KeyboardMouse,
        XboxController,
        PlayStationController,
        OtherGamepad
    }

    public static DeviceType CurrentDeviceType { get; private set; } = DeviceType.KeyboardMouse;

    private void OnEnable()
    {
        InputSystem.onEvent += OnInputEvent;
    }

    private void OnDisable()
    {
        InputSystem.onEvent -= OnInputEvent;
    }

    private void OnInputEvent(InputEventPtr eventPtr, InputDevice device)
    {
        // Filter only button press events
        if (eventPtr.IsA<StateEvent>() || eventPtr.IsA<DeltaStateEvent>())
        {
            var gamepad = device as Gamepad;

            if (gamepad != null)
            {
                string displayName = gamepad.displayName.ToLower();

                if (displayName.Contains("xbox"))
                    CurrentDeviceType = DeviceType.XboxController;
                else if (displayName.Contains("dualshock") || displayName.Contains("dualsense") || displayName.Contains("playstation"))
                    CurrentDeviceType = DeviceType.PlayStationController;
                else
                    CurrentDeviceType = DeviceType.OtherGamepad;
            }
            else if (device is Keyboard || device is Mouse)
            {
                CurrentDeviceType = DeviceType.KeyboardMouse;
            }
        }
    }
}
