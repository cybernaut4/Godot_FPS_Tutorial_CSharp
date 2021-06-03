using Godot;
using System;

public class Pistol : Weapon
{
    [Export] public PackedScene BulletScene;
    public const int AMMO_IN_MAG = 10;

    public override void _Ready()
    {
        Damage = 15;
        SpareAmmo = 40;
        AmmoInWeapon = AMMO_IN_MAG;

        Reloadable = true; // CAN_RELOAD
        Refillable = true; // CAN_REFILL

        IdleAnimationState = AnimationLoader.AnimationState.Pistol_idle;
        FireAnimationState = AnimationLoader.AnimationState.Pistol_fire;
        ReloadAnimationState = AnimationLoader.AnimationState.Pistol_reload;

        WeaponEnabled = false;
        PlayerNode = null;
    }

    public override void FireWeapon()
    {
        Bullet bullet = BulletScene.Instance() as Bullet;
        Node sceneRoot = GetTree().Root.GetChildren()[0] as Node;
        sceneRoot.AddChild(bullet);

        bullet.GlobalTransform = this.GlobalTransform;
        bullet.Scale = new Vector3(4, 4, 4);
        bullet.Damage = Damage;
        AmmoInWeapon--;
        PlayerNode.CreateSound(FireSound, this.GlobalTransform.origin);
    }

    public override bool ReloadWeapon() {
        bool canReload = false;

        if (PlayerNode.AnimationPlayer.currentState.ToString() == IdleAnimationState.ToString())
            canReload = true;

        if (SpareAmmo <= 0 || AmmoInWeapon == AMMO_IN_MAG)
            canReload = false;

        if (canReload)
        {
            var ammoNeeded = AMMO_IN_MAG - AmmoInWeapon;

            if (SpareAmmo >= ammoNeeded) {
                SpareAmmo -= ammoNeeded;
                AmmoInWeapon = AMMO_IN_MAG;
            } else {
                AmmoInWeapon += SpareAmmo;
                SpareAmmo = 0;
            }

            PlayerNode.CreateSound(ReloadSound, this.GlobalTransform.origin);
            PlayerNode.AnimationPlayer.SetAnimation(AnimationLoader.AnimationState.Pistol_reload);

            return true;
        }
        return false;
    }

    public override bool EquipWeapon()
    {
        if (PlayerNode.AnimationPlayer.currentState.ToString() == IdleAnimationState.ToString()) {
            WeaponEnabled = true;
            return true;
        }
        if (PlayerNode.AnimationPlayer.currentState.ToString() == "Idle_unarmed") {
            PlayerNode.AnimationPlayer.SetAnimation(AnimationLoader.AnimationState.Pistol_equip);
        }
        return false;
    }

    public override bool UnequipWeapon()
    {
        if (PlayerNode.AnimationPlayer.currentState.ToString() == IdleAnimationState.ToString())
            if (PlayerNode.AnimationPlayer.currentState.ToString() != "Pistol_unequip")
                PlayerNode.AnimationPlayer.SetAnimation(AnimationLoader.AnimationState.Pistol_unequip);

        if (PlayerNode.AnimationPlayer.currentState.ToString() == "Idle_unarmed") 
        {
            WeaponEnabled = false;
            return true;
        } 
        else 
        {
            return false;
        }
    }

}
