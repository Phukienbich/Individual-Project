using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerCamera))]
public class PlayerController : MonoBehaviour, IDataPersistance
{
    //Singleton Reference
    public static PlayerController Instance;

    public delegate void PlayerActions();
    public static event PlayerActions
        OnInventoryToggle,
        OnInteraction,
        OnStartPrimaryAttack,
        OnStopPrimaryAttack,
        OnStartSecondaryAttack,
        OnStopSecondaryAttack,
        OnReload,
        OnUIRightClick,
        OnUIClickStarted,
        OnUIClickCancelled,
        OnQuickDrop,
        OnSpeedUpStart,
        OnSpeedUpCancel,
        OnPause,
        OnSkip,
        OnExitUI;

    public delegate void HotbarActions(int _num);
    public static event HotbarActions OnScroll, OnSwitchTo;

    //References
    PlayerInputs playerInputs;
    PlayerMotor playerMotor;
    PlayerCamera playerCamera;

    bool canMove = true, canRotate = true, canAttack = true, canSwitch = true;

    void Awake()
    {
        //Singleton init
        if (Instance != null && Instance != this) {
            Destroy(Instance);
        }
        Instance = this;

        //Reference init
        playerInputs = new PlayerInputs();
    }

    void Start()
    {
        //Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //Reference init
        playerMotor = GetComponent<PlayerMotor>();
        playerCamera = GetComponent<PlayerCamera>();
    }

    void OnEnable()
    {
        playerInputs.Player.Enable();
        #region Subscribe Player Inputs
        playerInputs.Player.SpeedUp.started += SpeedUp;
        playerInputs.Player.SpeedUp.canceled += SpeedUp;
        playerInputs.Player.ToggleInventory.performed += ToggleInventory;
        playerInputs.Player.Interaction.performed += Interact;
        playerInputs.Player.Fire.started += PrimaryAttack;
        playerInputs.Player.Fire.canceled += PrimaryAttack;
        playerInputs.Player.SecondaryFire.started += SecondaryAttack;
        playerInputs.Player.SecondaryFire.canceled += SecondaryAttack;
        playerInputs.Player.Reload.performed += Reload;
        playerInputs.Player.SwitchTo1.performed += SwitchTo1;
        playerInputs.Player.SwitchTo2.performed += SwitchTo2;
        playerInputs.Player.SwitchTo3.performed += SwitchTo3;
        playerInputs.Player.SwitchTo4.performed += SwitchTo4;
        playerInputs.Player.SwitchTo5.performed += SwitchTo5;
        playerInputs.Player.SwitchTo6.performed += SwitchTo6;
        playerInputs.Player.Skip.performed += Skip;
        playerInputs.Player.Pause.performed += Pause;
        #endregion

        #region Subscribe UI Inputs
        playerInputs.UI.RightClick.started += RightClickUI;
        playerInputs.UI.Click.started += ClickUI;
        playerInputs.UI.Click.canceled += ClickUI;
        playerInputs.UI.FastDrop.performed += FastDropSlot;
        playerInputs.UI.ExitUI.performed += ExitUI;
        #endregion
    }
    
    void OnDisable()
    {
        playerInputs.Player.Disable();
        #region Unsubscribe Player Inputs
        playerInputs.Player.SpeedUp.started -= SpeedUp;
        playerInputs.Player.SpeedUp.canceled -= SpeedUp;
        playerInputs.Player.ToggleInventory.performed -= ToggleInventory;
        playerInputs.Player.Interaction.performed -= Interact;
        playerInputs.Player.Fire.started -= PrimaryAttack;
        playerInputs.Player.Fire.canceled -= PrimaryAttack;
        playerInputs.Player.SecondaryFire.started -= SecondaryAttack;
        playerInputs.Player.SecondaryFire.canceled -= SecondaryAttack;
        playerInputs.Player.Reload.performed -= Reload;
        playerInputs.Player.SwitchTo1.performed -= SwitchTo1;
        playerInputs.Player.SwitchTo2.performed -= SwitchTo2;
        playerInputs.Player.SwitchTo3.performed -= SwitchTo3;
        playerInputs.Player.SwitchTo4.performed -= SwitchTo4;
        playerInputs.Player.SwitchTo5.performed -= SwitchTo5;
        playerInputs.Player.SwitchTo6.performed -= SwitchTo6;
        playerInputs.Player.Skip.performed -= Skip;
        playerInputs.Player.Pause.performed -= Pause;
        #endregion

        playerInputs.UI.Disable();
        #region Unsubscribe UI Inputs
        playerInputs.UI.RightClick.started -= RightClickUI;
        playerInputs.UI.Click.started -= ClickUI;
        playerInputs.UI.Click.canceled -= ClickUI;
        playerInputs.UI.FastDrop.performed -= FastDropSlot;
        playerInputs.UI.ExitUI.performed -= ExitUI;
        #endregion
    }

    /// <summary>
    /// Toggle the player's ability to move
    /// </summary>
    /// <param name="_state">The state to toggle to</param>
    public void ToggleMovement(bool _state)
    {
        canMove = _state;
    }

    /// <summary>
    /// Toggle the player's ability to rotate
    /// </summary>
    /// <param name="_state">The state to toggle to</param>
    public void ToggleRotation(bool _state)
    {
        canRotate = _state;
    }

    /// <summary>
    /// Toggle the player's ability to attack
    /// </summary>
    /// <param name="_state">The state to toggle to</param>
    public void ToggleAttack(bool _state)
    {
        canAttack = _state;
    }

    /// <summary>
    /// Toggle the player's ability to switch their hot bar
    /// </summary>
    /// <param name="_state"></param>
    public void ToggleSwitchHotbar(bool _state)
    {
        canSwitch = _state;
    }

    /// <summary>
    /// Add a value to the multiplier applied to floating movements
    /// </summary>
    /// <param name="_value"></param>
    public void AddFloatingSpeedMultipier(float _value)
    {
        playerMotor.FloatingSpeedMultiplier += _value;
    }

    /// <summary>
    /// Returns the player's current position
    /// </summary>
    /// <returns>The Vector3 position of the player</returns>
    public Vector3 GetPlayerPosition()
    {
        return transform.position;
    }

    /// <summary>
    /// Sets the player's position to a given position
    /// </summary>
    /// <param name="_pos"></param>
    public void SetPlayerPosition(Vector3 _pos)
    {
        transform.position = _pos;
    }

    /// <summary>
    /// Sets the player's rotation to a given rotation
    /// </summary>
    /// <param name="_rot"></param>
    public void SetPlayerRotation(Quaternion _rot)
    {
        playerCamera.SetRotation(_rot);
    }

    /// <summary>
    /// Returns the current mouse inputs of this player
    /// </summary>
    /// <returns></returns>
    public Vector2 GetMouseInputs()
    {
        if (canRotate)
            return playerInputs.Player.Look.ReadValue<Vector2>();
        else
            return Vector2.zero;
    }

    /// <summary>
    /// Returns the transform of the player's orientation
    /// </summary>
    /// <returns>The orientation transform of the player</returns>
    public Transform GetOrientation()
    {
        return playerMotor.GetOrientation();
    }

    /// <summary>
    /// Toggles the player's inputs for the UI
    /// </summary>
    /// <param name="_state">The state to toggle the inputs to</param>
    public void ToggleInterfaceInputs(bool _state)
    {
        if (_state == true) 
            playerInputs.UI.Enable();
        else 
            playerInputs.UI.Disable();
    }

    void Update()
    {
        Movement();
        VerticalMovement();
        Rotation();
        SwitchScroll();
    }

    /// <summary>
    /// Calculate movement inputs
    /// </summary>
    void Movement()
    {
        if (!canMove) {
            playerMotor.SetDirection(Vector3.zero);
        }
        else {
            Vector2 _inputVector = playerInputs.Player.Move.ReadValue<Vector2>();
            //Calculate direction based of current orientation
            Vector3 _moveDirection = (playerMotor.GetOrientation().forward * _inputVector.y) + (playerMotor.GetOrientation().right * _inputVector.x);
            playerMotor.SetDirection(_moveDirection);
        }
    }

    /// <summary>
    /// Calculate vertical movement inputs
    /// </summary>
    void VerticalMovement()
    {
        if (!canMove) {
            playerMotor.SetVerticalDirection(0f);
        }
        else {
            float _inputVector = playerInputs.Player.VerticalMove.ReadValue<float>();
            playerMotor.SetVerticalDirection(_inputVector);
        }
    }

    /// <summary>
    /// Calculate player rotations with mouse inputs
    /// </summary>
    void Rotation()
    {
        if (!canRotate) {
            playerCamera.SetRotationDir(Quaternion.Euler(Vector3.zero));
        }
        else {
            Vector2 _inputVector = playerInputs.Player.Look.ReadValue<Vector2>();
            playerCamera.SetRotationDir(Quaternion.Euler(_inputVector));
        }
    }

    void SpeedUp(InputAction.CallbackContext context)
    {
        if (context.started) {
            playerMotor.SpeedUp(true);
            OnSpeedUpStart?.Invoke();
        }
        if (context.canceled) {
            playerMotor.SpeedUp(false);
            OnSpeedUpCancel?.Invoke();
        }
    }

    void ToggleInventory(InputAction.CallbackContext context)
    {
        if (context.performed) {
            OnInventoryToggle?.Invoke();
        }
    }

    void FastDropSlot(InputAction.CallbackContext context)
    {
        if (context.performed) {
            OnQuickDrop?.Invoke();
        }
    }

    void Interact(InputAction.CallbackContext context)
    {
        if (context.performed) {
            OnInteraction?.Invoke();
        }
    }

    void ClickUI(InputAction.CallbackContext context)
    {
        if (context.started) {
            OnUIClickStarted?.Invoke();
        }
        else if (context.canceled) {
            OnUIClickCancelled?.Invoke();
        }
    }

    void RightClickUI(InputAction.CallbackContext context)
    {
        if (context.started) {
            OnUIRightClick?.Invoke();
        }
    }

    void PrimaryAttack(InputAction.CallbackContext context)
    {
        if (!canAttack)
            return;

        if (context.started)
            OnStartPrimaryAttack?.Invoke();
        if (context.canceled)
            OnStopPrimaryAttack?.Invoke();
    }

    void SecondaryAttack(InputAction.CallbackContext context)
    {
        if (!canAttack)
            return;

        if (context.started)
            OnStartSecondaryAttack?.Invoke();
        if (context.canceled)
            OnStopSecondaryAttack?.Invoke();
    }

    void Reload(InputAction.CallbackContext context)
    {
        if (context.performed)
            OnReload?.Invoke();
    }

    void SwitchScroll()
    {
        if (!canSwitch)
            return;
        //Returns 1D vector depending on scroll dir
        float _scrollAxis = playerInputs.Player.SwitchScroll.ReadValue<float>();
        if (_scrollAxis != 0f)
            OnScroll?.Invoke((int)_scrollAxis);
    }
    void SwitchTo1(InputAction.CallbackContext context)
    {
        if (!canSwitch)
            return;

        if (context.performed) {
            OnSwitchTo?.Invoke(0);
        }
    }
    void SwitchTo2(InputAction.CallbackContext context)
    {
        if (!canSwitch)
            return;

        if (context.performed) {
            OnSwitchTo?.Invoke(1);
        }
    }
    void SwitchTo3(InputAction.CallbackContext context)
    {
        if (!canSwitch)

            return;
        if (context.performed) {
            OnSwitchTo?.Invoke(2);
        }
    }
    void SwitchTo4(InputAction.CallbackContext context)
    {
        if (!canSwitch)
            return;

        if (context.performed) {
            OnSwitchTo?.Invoke(3);
        }
    }
    void SwitchTo5(InputAction.CallbackContext context)
    {
        if (!canSwitch)
            return;

        if (context.performed) {
            OnSwitchTo?.Invoke(4);
        }
    }
    void SwitchTo6(InputAction.CallbackContext context)
    {
        if (!canSwitch)
            return;

        if (context.performed) {
            OnSwitchTo?.Invoke(5);
        }
    }

    void Pause(InputAction.CallbackContext context)
    {
        if (context.performed) {
            OnPause?.Invoke();
        }
    }

    void Skip(InputAction.CallbackContext context)
    {
        if (context.performed) {
            OnSkip?.Invoke();
        }
    }
    
    void ExitUI(InputAction.CallbackContext context)
    {
        if (context.performed) {
            OnExitUI?.Invoke();
        }
    }

    public void SaveData(ref GameData _data)
    {
        _data.playerPosition = transform.position;
        _data.playerRotation = playerCamera.GetRotation();
    }

    public void LoadData(GameData _data)
    {
        SetPlayerPosition(_data.playerPosition);
        SetPlayerRotation(_data.playerRotation);
    }
}
