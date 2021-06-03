using Godot;
using System;
using System.Linq;
using System.Collections.Generic;

public class AnimationLoader : AnimationPlayer
{
    public enum AnimationState {
        Idle_unarmed,
        Pistol_equip,
        Pistol_fire,
        Pistol_idle,
        Pistol_reload,
        Pistol_unequip,
        Rifle_equip,
        Rifle_fire,
        Rifle_idle,
        Rifle_reload,
        Rifle_unequip,
        Knife_equip,
        Knife_fire,
        Knife_idle,
        Knife_unequip
    }

    Dictionary<AnimationState, string[]> states = new Dictionary<AnimationState, string[]>() {
        { AnimationState.Idle_unarmed,   new string[] { "Knife_equip", "Pistol_equip", "Rifle_equip", "Idle_unarmed" } },
        { AnimationState.Pistol_equip,   new string[] { "Pistol_idle" } },
        { AnimationState.Pistol_fire,    new string[] { "Pistol_idle" } },
        { AnimationState.Pistol_idle,    new string[] { "Pistol_fire", "Pistol_reload", "Pistol_unequip", "Pistol_idle" } },
        { AnimationState.Pistol_reload,  new string[] { "Pistol_idle" } },
        { AnimationState.Pistol_unequip, new string[] { "Idle_unarmed" } },
 
        { AnimationState.Rifle_equip,   new string[] { "Rifle_idle" } },
        { AnimationState.Rifle_fire,    new string[] { "Rifle_idle" } },
        { AnimationState.Rifle_idle,    new string[] { "Rifle_fire", "Rifle_reload", "Rifle_unequip", "Rifle_idle" } },
        { AnimationState.Rifle_reload,  new string[] { "Rifle_idle" } },
        { AnimationState.Rifle_unequip, new string[] { "Idle_unarmed" } },
 
        { AnimationState.Knife_equip,   new string[] { "Knife_idle" } },
        { AnimationState.Knife_fire,    new string[] { "Knife_idle" } },
        { AnimationState.Knife_idle,    new string[] { "Knife_fire", "Knife_unequip", "Knife_idle" } },
        { AnimationState.Knife_unequip, new string[] { "Idle_unarmed" } }
    };

    Dictionary<AnimationState, float> animationSpeeds = new Dictionary<AnimationState, float>() {
        { AnimationState.Idle_unarmed, 8f },

        { AnimationState.Pistol_equip, 3f },
        { AnimationState.Pistol_fire, 2.2f },
        { AnimationState.Pistol_idle, 1f },
        { AnimationState.Pistol_reload, 3f },
        { AnimationState.Pistol_unequip, 4f },

        { AnimationState.Rifle_equip, 3f },
        { AnimationState.Rifle_fire, 10f },
        { AnimationState.Rifle_idle, 1f },
        { AnimationState.Rifle_reload, 2f },
        { AnimationState.Rifle_unequip, 3f },

        { AnimationState.Knife_equip, 3f },
        { AnimationState.Knife_fire, 1.35f },
        { AnimationState.Knife_idle, 1f },
        { AnimationState.Knife_unequip, 3f }
    };

    public AnimationState currentState = AnimationState.Idle_unarmed;
    public Action CallbackFunction = null;
    

    public override void _Ready()
    {
        Connect("animation_finished", this, "AnimationEnded");
    }

    public bool SetAnimation(AnimationState animationName)
    {
        if (animationName == currentState) {
            GD.Print($"AnimationLoader.cs -- WARNING: animation is already {animationName}");
            return true;
        }

        if (HasAnimation(animationName.ToString()))
        {
            var possibleAnimations = states[currentState];

            if (Array.IndexOf(possibleAnimations, animationName.ToString()) != -1)
            {
                currentState = animationName;
                Play(animationName.ToString(), -1, animationSpeeds[animationName]);
                return true;
            }
            else 
            {
                GD.Print($"AnimationLoader.cs -- WARNING: Cannot change to {animationName} from {currentState}");
                return false;
            }
        }
        return false;
    }

    public void AnimationEnded(AnimationState animationName)
    {
        switch(currentState) {
            case AnimationState.Idle_unarmed:   break;

            case AnimationState.Knife_equip:    SetAnimation(AnimationState.Knife_idle); break;
            case AnimationState.Knife_idle:     break;
            case AnimationState.Knife_fire:     SetAnimation(AnimationState.Knife_idle); break;
            case AnimationState.Knife_unequip:  SetAnimation(AnimationState.Idle_unarmed); break;

            case AnimationState.Pistol_equip:   SetAnimation(AnimationState.Pistol_idle); break;
            case AnimationState.Pistol_idle:    break;
            case AnimationState.Pistol_fire:    SetAnimation(AnimationState.Pistol_idle); break;
            case AnimationState.Pistol_unequip: SetAnimation(AnimationState.Idle_unarmed); break;
            case AnimationState.Pistol_reload:  SetAnimation(AnimationState.Pistol_idle); break;

            case AnimationState.Rifle_equip:    SetAnimation(AnimationState.Rifle_idle); break;
            case AnimationState.Rifle_idle:     break;
            case AnimationState.Rifle_fire:     SetAnimation(AnimationState.Rifle_idle); break;
            case AnimationState.Rifle_unequip:  SetAnimation(AnimationState.Idle_unarmed); break;
            case AnimationState.Rifle_reload:   SetAnimation(AnimationState.Rifle_idle); break;
        }
    }
    
    void AnimationCallback() {
        CallbackFunction?.Invoke();
    }
}
