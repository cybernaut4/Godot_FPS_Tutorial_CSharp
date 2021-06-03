using Godot;
using System;

public class Rifle : Weapon
{
    public const int AMMO_IN_MAG = 50;
    
    public override void _Ready()
    {
        Damage = 4;
        SpareAmmo = 125;
        AmmoInWeapon = 25;
        
        Reloadable = true; // CAN_RELOAD
        Refillable = true; // CAN_REFILL

        IdleAnimationState = AnimationLoader.AnimationState.Rifle_idle;
        FireAnimationState = AnimationLoader.AnimationState.Rifle_fire;
        ReloadAnimationState = AnimationLoader.AnimationState.Rifle_reload;

        WeaponEnabled = false;
        PlayerNode = null;
    }

    public override void FireWeapon()
    {
        RayCast ray = GetNode<RayCast>("Ray_Cast");
        ray.ForceRaycastUpdate();
        AmmoInWeapon--;

        if (ray.IsColliding()) {
            var body = ray.GetCollider();

            if (body == PlayerNode) {

            } 
            else if (body is HittableByBullets hittableByBullets) 
            {
                hittableByBullets.BulletHit(Damage, ray.GlobalTransform);
            }
        }
        
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
            PlayerNode.AnimationPlayer.SetAnimation(AnimationLoader.AnimationState.Rifle_reload);

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
            PlayerNode.AnimationPlayer.SetAnimation(AnimationLoader.AnimationState.Rifle_equip);
        }
        return false;
    }

    public override bool UnequipWeapon()
    {
        if (PlayerNode.AnimationPlayer.currentState.ToString() == IdleAnimationState.ToString())
            if (PlayerNode.AnimationPlayer.currentState.ToString() != "Rifle_unequip")
                PlayerNode.AnimationPlayer.SetAnimation(AnimationLoader.AnimationState.Rifle_unequip);

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
