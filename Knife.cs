using Godot;
using System;

public class Knife : Weapon
{
    public const int AMMO_IN_MAG = 1;

    public override void _Ready()
    {
        Damage = 40;
        SpareAmmo = 1;
        AmmoInWeapon = AMMO_IN_MAG;

        IdleAnimationState = AnimationLoader.AnimationState.Knife_idle;
        FireAnimationState = AnimationLoader.AnimationState.Knife_fire;
        ReloadAnimationState = AnimationLoader.AnimationState.Knife_idle;

        WeaponEnabled = false;
        PlayerNode = null;
    }

    public override void FireWeapon()
    {
        Area area = GetNode<Area>("Area");
        var bodies = area.GetOverlappingBodies();

        for (int i = 0; i < bodies.Count; i++) {
            if (bodies[i] is PhysicsBody)
            {
                PhysicsBody body = (PhysicsBody)bodies[i];
                if (body == PlayerNode)
                {

                }
                else if (body is HittableByBullets hittableByBullets)
                    hittableByBullets.BulletHit(Damage, area.GlobalTransform);
            }
        }
    }

    public override bool ReloadWeapon()
    {
        return false;
    }

    public override bool EquipWeapon()
    {
        if (PlayerNode.AnimationPlayer.currentState.ToString() == IdleAnimationState.ToString()) {
            WeaponEnabled = true;
            return true;
        }
        if (PlayerNode.AnimationPlayer.currentState.ToString() == "Idle_unarmed") {
            PlayerNode.AnimationPlayer.SetAnimation(AnimationLoader.AnimationState.Knife_equip);
        }
        return false;
    }

    public override bool UnequipWeapon()
    {
        if (PlayerNode.AnimationPlayer.currentState.ToString() == IdleAnimationState.ToString())
            if (PlayerNode.AnimationPlayer.currentState.ToString() != "Knife_unequip")
                PlayerNode.AnimationPlayer.SetAnimation(AnimationLoader.AnimationState.Knife_unequip);

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
