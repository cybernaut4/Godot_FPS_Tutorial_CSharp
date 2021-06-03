using Godot;
using System;

public abstract class Weapon : Spatial
{
    [Export] public AudioStreamSample FireSound = ResourceLoader.Load("res://assets/Audio/RifleFire.wav") as AudioStreamSample;
    [Export] public AudioStreamSample ReloadSound = ResourceLoader.Load("res://assets/Audio/PistolReload.wav") as AudioStreamSample;
    public float Damage;
    public AnimationLoader.AnimationState IdleAnimationState;
    public AnimationLoader.AnimationState FireAnimationState;
    public AnimationLoader.AnimationState ReloadAnimationState;
    public bool WeaponEnabled;
    public Player PlayerNode = null;
    public int SpareAmmo;
    public int AmmoInWeapon;
    public bool Reloadable = false; // CAN_RELOAD
    public bool Refillable = false; // CAN_REFILL

    //
    // Summary:
    //     Tells the weapon to open fire.
    public abstract void FireWeapon();
    public abstract bool ReloadWeapon();
    public abstract bool EquipWeapon();
    public abstract bool UnequipWeapon();

}