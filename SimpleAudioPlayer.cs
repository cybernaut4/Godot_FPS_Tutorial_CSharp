using Godot;
using System;

public class SimpleAudioPlayer : Spatial
{
    AudioStreamPlayer audioNode = null;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        audioNode = GetNode<AudioStreamPlayer>("Audio_Stream_Player");
        audioNode.Connect("finished", this, "DestroySelf");
        audioNode.Stop();
    }

    public bool PlaySound(AudioStreamSample soundName, Vector3 position)
    {
        if (soundName == null){
            GD.Print("UNKNOWN STREAM");
            QueueFree();
            return false;
        }

        audioNode.Stream = soundName;
        // 
        //  If you are using an AudioStreamPlayer3D, then uncomment these lines to set the position.
        //  (This C# code is UNTESTED for the time.)
        // 
        // if (audioNode is AudioStreamPlayer3D)
        // {
        //     if (position != null)
        //         audioNode.GlobalTransform = position;
        // }

        audioNode.Play();

        return true;
    }

    void DestroySelf()
    {
        audioNode.Stop();
        QueueFree();
    }
}
