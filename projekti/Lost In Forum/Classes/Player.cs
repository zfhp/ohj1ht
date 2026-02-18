using Jypeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lost_In_Forum;

public class Player : PhysicsObject
{
    public Player() : base(50, 50, Shape.Octagon)
    {
        this.MaxVelocity = 0;
    }
    public GameObject Sprite { get; set; }
    bool immune;
    public double punchInterval { get; set; } = 0.5;
    public Vector punchdir { get; set; }
    public bool Punching { get; set; }
    public int Dmg { get; set; } = 1;
    public IntMeter Hp { get; set; } = new IntMeter(3, 0, 3);
    public double Speed { get; set; } = 7;
    public Animation Walk_S { get; set; }
    public Animation Walk_N { get; set; }
    public Animation Walk_E { get; set; }
    public Animation Walk_W { get; set; }
    public Animation Punch_R { get; set; }
    public Animation Punch_L { get; set; }
    public Animation Punch_U { get; set; }
    public Animation Punch_D { get; set; }
    public Animation Idle { get; set; }
    public void TakeDamage(int dmg)
    {
        if (immune)
            return;
        Hp.AddValue(-dmg);
        UI.HealthBar.UpdateHealthBar(this);
        IFrames(0.75);
    }
    public void IFrames(double duration)
    {
        immune = true;
        Timer flashTimer = new Timer(0.1, delegate { this.Sprite.IsVisible = !this.Sprite.IsVisible; });
        flashTimer.Start();
        Timer.SingleShot(duration, delegate { immune = false; flashTimer.Stop(); this.Sprite.IsVisible = true; });
    }
    public void Walk(Vector dir)
    {
        if (Punching) return;
        this.Position += (dir * Speed);
    }
    public void Die()
    {
        this.Destroy();
        Speed = 0;
    }
}