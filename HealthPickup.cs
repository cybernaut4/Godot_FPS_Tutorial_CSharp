using Godot;
using System;

public class HealthPickup : Spatial
{
    const int RESPAWN_TIME = 20;

    public enum KitSize
    {
        Large,
        Small
    }

    [Export] public AudioStreamSample LargeKitSound = GD.Load<AudioStreamSample>("res://assets/Audio/LargeHealthPack.wav");
    [Export] public AudioStreamSample SmallKitSound = GD.Load<AudioStreamSample>("res://assets/Audio/SmallHealthPack.wav");
    [Export] public KitSize Size = KitSize.Large;
    public int[] HealthAmounts = {70, 30};

    float respawnTimer = 0;
    AudioStreamSample currentSound;

    public override void _Ready()
    {
        KitSizeChangeValues(KitSize.Large, false);
        KitSizeChangeValues(KitSize.Small, false);
        KitSizeChange(Size);
        GetNode("Holder/Health_Pickup_Trigger").Connect("body_entered", this, "TriggerBodyEntered");
    }

    public override void _PhysicsProcess(float delta)
    {
        respawnTimer = respawnTimer > 0 ? respawnTimer - delta : 0;

        if (respawnTimer <= 0)
            KitSizeChangeValues(Size, true);
    }

    void KitSizeChange(KitSize value)
    {
        KitSizeChangeValues(Size, false);
        Size = value;
        KitSizeChangeValues(Size, true);
    }

    void KitSizeChangeValues(KitSize size, bool enable)
    {
        switch (size)
        {
            case KitSize.Large:
                GetNode<CollisionShape>("Holder/Health_Pickup_Trigger/Shape_Kit").Disabled = !enable; 
                GetNode<Spatial>("Holder/Health_Kit").Visible = enable;
                currentSound = LargeKitSound;
                break;

            case KitSize.Small: 
                GetNode<CollisionShape>("Holder/Health_Pickup_Trigger/Shape_Kit_Small").Disabled = !enable; 
                GetNode<Spatial>("Holder/Health_Kit_Small").Visible = enable;
                currentSound = SmallKitSound;
                break;
        }
    }

    void TriggerBodyEntered(CollisionObject body)
    {
        if (body.HasMethod("AddHealth"))
        {
            Player player = body as Player;
            player.AddHealth(HealthAmounts[(int)Size]);
            respawnTimer = RESPAWN_TIME;
            KitSizeChangeValues(Size, false);
            player.CreateSound(currentSound, player.GlobalTransform.origin);
            GD.Print($"You've got a {Size.ToString()} Health Pack (+{HealthAmounts[(int)Size]})");
        }
    }
}
