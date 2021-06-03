using Godot;
using System;

public class Bullet : Spatial
{
    [Export] public float Speed = 70;
    [Export] public float Damage = 15;

    const int killTimer = 4;
    float timer = 0;

    bool hitSomething = false;

    public override void _Ready()
    {
        GetNode<Area>("Area").Connect("body_entered", this, "Collided");
    }

    public override void _PhysicsProcess(float delta)
    {
        var forwardDirection = GlobalTransform.basis.z.Normalized();
        GlobalTranslate(forwardDirection * Speed * delta);

        timer += delta;

        if (timer >= killTimer)
            QueueFree();
    }

    public void Collided(CollisionObject2D body) {
        if (!hitSomething)
            if (body is HittableByBullets hittableByBullets)
                hittableByBullets.BulletHit(Damage, GlobalTransform);

        hitSomething = true;
        QueueFree();
    }
}
