using Godot;
using System;
using System.Collections.Generic;

//
// Summary:
//     The Player. Nuff said.
public class Player : KinematicBody, HittableByBullets
{
    const float MOUSE_SCROLL_WHEEL_SENSITIVITY = 0.08f;
    const int MAX_HEALTH = 150;

    [Export] public float Gravity = -24.8f;
    [Export] public float MaxSpeed = 20;
    [Export] public float JumpSpeed = 18;
    [Export] public float Acceleration = 4.5f;
    [Export] public float Deacceleration = 16;
    [Export] public float MaxSlopeAngle = 40;
    [Export] public float MouseSensitivity = 0.05f;
    [Export] public float MaxSprintSpeed = 30;
    [Export] public float SprintAccel = 30;

    bool _isSprinting = false;
    bool reloadingWeapon = false;
    bool changingWeapon = false;    
    string currentWeaponName = "UNARMED";
    string changingWeaponName = "UNARMED";

    float mouseScrollValue = 0;

    [Export] float health = 100;

    Vector3 _velocity = new Vector3();
    Vector3 _direction = new Vector3();

    Camera _camera;
    Spatial _rotationHelper;
    SpotLight _flashlight;
    PackedScene simpleAudioPlayer = GD.Load("res://Simple_Audio_Player.tscn") as PackedScene;
    
    public AnimationLoader AnimationPlayer;
    
    Dictionary<string, Weapon> weapons = new Dictionary<string, Weapon>() {
        { "UNARMED", null },
        { "KNIFE", null },
        { "PISTOL", null },
        { "RIFLE", null }
    }; 
    Dictionary<int, string> weaponNumberToName = new Dictionary<int, string>() {
        { 0, "UNARMED" },
        { 1, "KNIFE" },
        { 2, "PISTOL" },
        { 3, "RIFLE" }
    };
    Dictionary<string, int> weaponNameToNumber = new Dictionary<string, int>() {
        { "UNARMED", 0 },
        { "KNIFE", 1 },
        { "PISTOL", 2 },
        { "RIFLE", 3 }
    };


    Label UIStatusLabel;
    
    /// <summary>Called when the player enters the game for the first time.</summary>
    public override void _Ready()
    {
        _flashlight = GetNode<SpotLight>("Rotation_Helper/Flashlight");
        _camera = GetNode<Camera>("Rotation_Helper/Camera");
        AnimationPlayer = GetNode<AnimationLoader>("Rotation_Helper/Model/Animation_Player");
        AnimationPlayer.CallbackFunction = FireBullet;
        _rotationHelper = GetNode<Spatial>("Rotation_Helper");

        weapons["KNIFE"] = GetNode<Weapon>("Rotation_Helper/Gun_Fire_Points/Knife_Point");
        weapons["PISTOL"] = GetNode<Weapon>("Rotation_Helper/Gun_Fire_Points/Pistol_Point");
        weapons["RIFLE"] = GetNode<Weapon>("Rotation_Helper/Gun_Fire_Points/Rifle_Point");

        Vector3 gunAimPointPos = GetNode<Spatial>("Rotation_Helper/Gun_Aim_Point").GlobalTransform.origin;

        foreach (KeyValuePair<string, Weapon> weapon in weapons)
        {
            Weapon weaponNode = weapon.Value;
            if (weaponNode != null)
            {
                weaponNode.PlayerNode = this;
                weaponNode.LookAt(gunAimPointPos, new Vector3(0, 1, 0));
                weaponNode.RotateObjectLocal(new Vector3(0, 1, 0), Mathf.Deg2Rad(180));
            }
        }

        currentWeaponName = "UNARMED";
        changingWeaponName = "UNARMED";

        UIStatusLabel = GetNode<Label>("HUD/Panel/Gun_label");

        Input.SetMouseMode(Input.MouseMode.Captured);
    }

    public override void _PhysicsProcess(float delta)
    {
        ProcessInput();
        ProcessMovement(delta);
        ProcessChangingWeapons();
        ProcessReloading();
        ProcessUI();
        HealthDecay(delta);
    }

    void ProcessInput()
    {
        GetWalkInput();
        GetJumpInput();
        GetFlashlightToggleInput();
        GetCaptureCursorToggleInput();
        GetChangeWeaponInput();
        GetFireWeaponInput();
        GetReloadInput();
    }

    void GetWalkInput()
    {
        _direction = new Vector3();
        Transform camXform = _camera.GlobalTransform;

        Vector2 inputMovementVector = new Vector2();

        inputMovementVector.y += Input.IsActionPressed("movement_forward") ? 1 : 0;
        inputMovementVector.y -= Input.IsActionPressed("movement_backward") ? 1 : 0;
        inputMovementVector.x -= Input.IsActionPressed("movement_left") ? 1 : 0;
        inputMovementVector.x += Input.IsActionPressed("movement_right") ? 1 : 0;

        inputMovementVector = inputMovementVector.Normalized();

        _direction += -camXform.basis.z * inputMovementVector.y;
        _direction += camXform.basis.x * inputMovementVector.x;

        _isSprinting = Input.IsActionPressed("movement_sprint") ? true : false;
    }

    void GetJumpInput() 
    {
        if (IsOnFloor()) {
            if (Input.IsActionJustPressed("movement_jump"))
                Jump();
        }
    }

    void Jump()
    {
        _velocity.y = JumpSpeed;
    }

    void GetFlashlightToggleInput()
    {
        if ( Input.IsActionJustPressed("flashlight")) {
            if (_flashlight.IsVisibleInTree())
                _flashlight.Hide();
            else
                _flashlight.Show();
        }
        
    }

    void GetCaptureCursorToggleInput()
    {
        if (Input.IsActionJustPressed("ui_cancel")) {
            if (Input.GetMouseMode() == Input.MouseMode.Visible)
                Input.SetMouseMode(Input.MouseMode.Captured);
            else
                Input.SetMouseMode(Input.MouseMode.Visible);
        }
    }

    void GetChangeWeaponInput()
    {
        int weaponChangeNumber = weaponNameToNumber[currentWeaponName];
        const int weaponSlots = 4;

        for (var i = 0; i < weaponSlots; i++)
            if (Input.IsActionPressed($"weapon_{i+1}")) weaponChangeNumber = i;

        if (Input.IsActionJustPressed("shift_weapon_positive")) weaponChangeNumber++;
        if (Input.IsActionJustPressed("shift_weapon_negative")) weaponChangeNumber--;

        // weaponChangeNumber = Mathf.Clamp(weaponChangeNumber, 0, weaponNumberToName.Count - 1);
        weaponChangeNumber = weaponChangeNumber < 0 ? weaponNumberToName.Count - 1 : (weaponChangeNumber > weaponNumberToName.Count - 1 ? 0 : weaponChangeNumber);

        if (!changingWeapon && !reloadingWeapon)
        {
            if (weaponNumberToName[weaponChangeNumber] != currentWeaponName)
            {
                changingWeaponName = weaponNumberToName[weaponChangeNumber];
                changingWeapon = true;
                mouseScrollValue = weaponChangeNumber;
            }
        }
        
    }

    void GetFireWeaponInput()
    {
        if (Input.IsActionPressed("fire"))
        {
            if (reloadingWeapon == false)
            {
                if (changingWeapon == false)
                {
                    Weapon currentWeapon = weapons[currentWeaponName];
                    if (currentWeapon != null && currentWeapon.AmmoInWeapon > 0)
                    {
                        if (AnimationPlayer.currentState == currentWeapon.IdleAnimationState)
                        {
                            AnimationPlayer.SetAnimation(currentWeapon.FireAnimationState);
                        }
                    }
                }
            }
        }
    }

    void GetReloadInput() {
        if (!reloadingWeapon && !changingWeapon)
        {
            if (Input.IsActionJustPressed("reload"))
            {
                Weapon currentWeapon = weapons[currentWeaponName];

                if (currentWeapon != null && currentWeapon.Reloadable)
                {
                    var currentAnimationState = AnimationPlayer.currentState;
                    bool isReloading = false;

                    foreach (KeyValuePair<string, Weapon> weapon in weapons)
                    {
                        var weaponNode = weapons[weapon.Key];
                        if (weaponNode != null)
                        {
                            if (currentAnimationState == weaponNode.ReloadAnimationState) {
                                isReloading = true;
                            }
                        }
                    }
                    if (isReloading == false)
                    {
                        reloadingWeapon = true;
                    }
                }
            }
        }
    }

    void ProcessReloading()
    {
        if (reloadingWeapon == true)
        {
            Weapon currentWeapon = weapons[currentWeaponName];
            if (currentWeapon != null)
                currentWeapon.ReloadWeapon();
            reloadingWeapon = false;
        }
    }

    void ProcessMovement(float delta)
    {
        _direction.y = 0;
        _direction = _direction.Normalized();

        _velocity.y += delta * Gravity;

        Vector3 hvelocity = _velocity;
        Vector3 target = _direction;

        target *= _isSprinting ? MaxSprintSpeed : MaxSpeed;

        float acceleration;

        if (_direction.Dot(hvelocity) > 0)
            acceleration = _isSprinting ? SprintAccel : Acceleration;
        else
            acceleration = Deacceleration;

        
        hvelocity = hvelocity.LinearInterpolate(target, acceleration * delta);
        _velocity.x = hvelocity.x;
        _velocity.z = hvelocity.z;
        _velocity = MoveAndSlide(_velocity, new Vector3(0, 1, 0), false, 4, Mathf.Deg2Rad(MaxSlopeAngle));
    }

    void ProcessChangingWeapons()
    {
        if (changingWeapon)
        {
            bool weaponUnequipped = false;
            Weapon currentWeapon = weapons[currentWeaponName];

            if (currentWeapon == null)
            {
                weaponUnequipped = true;
            }
            else
            {
                if (currentWeapon.WeaponEnabled)
                {
                    weaponUnequipped = currentWeapon.UnequipWeapon();
                }
                else
                {
                    weaponUnequipped = true;
                }
            }

            if (weaponUnequipped)
            {
                bool weaponEquipped = false;
                Weapon weaponToEquip = weapons[changingWeaponName];

                if (weaponToEquip == null)
                {
                    weaponEquipped = true;
                }
                else
                {
                    if (weaponToEquip.WeaponEnabled == false)
                    {
                        weaponEquipped = weaponToEquip.EquipWeapon();
                    }
                    else
                    {
                        weaponEquipped = true;
                    }
                }
                if (weaponEquipped)
                {
                    changingWeapon = false;
                    currentWeaponName = changingWeaponName;
                    changingWeaponName = "";
                }
            }
        }
    }

    void ProcessUI()
    {
        if (currentWeaponName == "UNARMED" || currentWeaponName == "KNIFE") {
            UIStatusLabel.Text = $"HEALTH: {Mathf.Ceil(health)}";
        }
        else {
            Weapon currentWeapon = weapons[currentWeaponName];
            UIStatusLabel.Text = $"HEALTH: {Mathf.Ceil(health)} \nAMMO: {currentWeapon.AmmoInWeapon} / {currentWeapon.SpareAmmo}";
        }
    }

    public void FireBullet() {
        if (changingWeapon) {
            
        }
        else
        {
            weapons[currentWeaponName].FireWeapon();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (Input.GetMouseMode() == Input.MouseMode.Captured){
            if (@event is InputEventMouseMotion) 
            {
                InputEventMouseMotion mouseMotionEvent = @event as InputEventMouseMotion;
                

                _rotationHelper.RotateX(Mathf.Deg2Rad(mouseMotionEvent.Relative.y * MouseSensitivity));
                RotateY(Mathf.Deg2Rad(-mouseMotionEvent.Relative.x * MouseSensitivity));

                Vector3 cameraRotation = _rotationHelper.RotationDegrees;
                cameraRotation.x = Mathf.Clamp(cameraRotation.x, -70, 70);
                _rotationHelper.RotationDegrees = cameraRotation;
            }
            if (@event is InputEventMouseButton)
            {
                InputEventMouseButton mouseButtonEvent = @event as InputEventMouseButton;
                if (mouseButtonEvent.ButtonIndex == (int)ButtonList.WheelUp)
                    mouseScrollValue += MOUSE_SCROLL_WHEEL_SENSITIVITY;
                else if (mouseButtonEvent.ButtonIndex == (int)ButtonList.WheelDown)
                    mouseScrollValue -= MOUSE_SCROLL_WHEEL_SENSITIVITY;

                mouseScrollValue = mouseScrollValue < 0 ? weaponNumberToName.Count - 1 : (mouseScrollValue > weaponNumberToName.Count - 1 ? 0 : mouseScrollValue);

                if (!changingWeapon && !reloadingWeapon)
                {
                    int roundMouseScrollValue = (int)Mathf.Round(mouseScrollValue);

                    if (weaponNumberToName[roundMouseScrollValue] != currentWeaponName)
                    {
                        changingWeaponName = weaponNumberToName[roundMouseScrollValue];
                        changingWeapon = true;
                        mouseScrollValue = roundMouseScrollValue;
                    }
                }
            }
        }
    }

    public void BulletHit(float damage, Transform globalTransform)
    {
        throw new NotImplementedException();
    }

    public void CreateSound(AudioStreamSample soundName, Vector3 position)
    {
        SimpleAudioPlayer audioClone = simpleAudioPlayer.Instance() as SimpleAudioPlayer;
        Node sceneRoot = GetTree().Root.GetChildren()[0] as Node;
        sceneRoot.AddChild(audioClone);
        audioClone.PlaySound(soundName, position);
    }

    public void AddHealth(int additionalHealth)
    {
        health += additionalHealth;
        health = Mathf.Clamp(health, 0, MAX_HEALTH);
    }

    public void AddAmmo(float additionalAmmo)
    {
        weapons["PISTOL"].SpareAmmo += (int)(Pistol.AMMO_IN_MAG * additionalAmmo);
        weapons["RIFLE"].SpareAmmo += (int)(Rifle.AMMO_IN_MAG * additionalAmmo);
    }

    public void HealthDecay(float rate) 
    {
        health = health > 100 ? health - rate : health;
    }

}
