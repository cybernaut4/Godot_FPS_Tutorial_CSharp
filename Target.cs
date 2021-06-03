using Godot;
using System;

public class Target : StaticBody, HittableByBullets
{
    const int TARGET_HEALTH = 40;
    const int TARGET_RESPAWN_TIME = 14;

    [Export] PackedScene DestroyedTarget;

    [Export] float ShatterImpulse = 12;

    float currentHealth = TARGET_HEALTH;
    float targetRespawnTimer = 0;

    Spatial BrokenTargetHolder;
    CollisionShape TargetCollisionShape;

    public override void _Ready()
    {
        BrokenTargetHolder = GetParent().GetNode<Spatial>("Broken_Target_Holder");
        TargetCollisionShape = GetNode<CollisionShape>("Collision_Shape");
    }

    public override void _PhysicsProcess(float delta)
    {
        // ProcessRespawn()
        if (targetRespawnTimer > 0)
        {
            targetRespawnTimer -= delta;

            if (targetRespawnTimer <= 0)
            {
                // CleanDebris()
                foreach (Spatial child in BrokenTargetHolder.GetChildren())
                {
                    child.QueueFree();
                }

                // Spawn()
                TargetCollisionShape.Disabled = false;
                Visible = true;
                currentHealth = TARGET_HEALTH;
            }
        }
    }

    public void BulletHit(float damage, Transform GlobalTransform)
    {
        GD.Print($"it IS being hit. Current health is {currentHealth}");
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            // SpawnDebris()
            Node clone = DestroyedTarget.Instance();
            BrokenTargetHolder.AddChild(clone);

            // CalculateDebrisPhysics()
            foreach (RigidBody rigid in clone.GetChildren())
            {
                if (rigid is RigidBody)
                {
                    Vector3 centerInRigidSpace = BrokenTargetHolder.GlobalTransform.origin - rigid.GlobalTransform.origin;
                    Vector3 direction = (rigid.Transform.origin - centerInRigidSpace).Normalized();
                    // Apply the impulse with some additional force (I find 12 works nicely).
                    rigid.ApplyImpulse(centerInRigidSpace, direction * ShatterImpulse * damage);
                }
            }

            // Despawn()
            targetRespawnTimer = TARGET_RESPAWN_TIME;

            TargetCollisionShape.Disabled = true;
            Visible = false;
        }
    }
}
