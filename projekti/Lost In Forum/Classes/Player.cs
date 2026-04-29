using Jypeli;
using Jypeli.Effects;

namespace Lost_In_Forum;

public class Player : PhysicsObject
{
    public Player() : base(50, 50, Shape.Octagon)
    {
        this.MaxVelocity = 0;
    }
    public GameObject Sprite { get; set; }
    bool _immune;
    public double PunchInterval { get; set; } = 0.5;
    public Vector Punchdir { get; set; }
    public bool Punching { get; set; }
    public int Dmg { get; set; } = 1;
    public IntMeter Hp { get; set; } = new IntMeter(3, 0, 3);
    public double Speed { get; set; } = 7;
    public Animation WalkS { get; set; }
    public Animation WalkN { get; set; }
    public Animation WalkE { get; set; }
    public Animation WalkW { get; set; }
    public Animation PunchR { get; set; }
    public Animation PunchL { get; set; }
    public Animation PunchU { get; set; }
    public Animation PunchD { get; set; }
    public Animation Idle { get; set; }
    public void TakeDamage(int dmg)
    {
        if (_immune)
            return;
        Hp.AddValue(-dmg);
        Ui.HealthBar.UpdateHealthBar(this);
        ImmunityFrames(0.75);
    }
    public void ImmunityFrames(double duration)
    {
        _immune = true;
        Timer flashTimer = new Timer(0.1, delegate { this.Sprite.IsVisible = !this.Sprite.IsVisible; });
        flashTimer.Start();
        Timer.SingleShot(duration, delegate { _immune = false; flashTimer.Stop(); this.Sprite.IsVisible = true; });
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
        var peli = (LostInForum)Game.Instance;
        ExplosionSystem efekti = new ExplosionSystem(this.Sprite.Animation.CurrentFrame, 100);
        efekti.MinVelocity = 50;
        efekti.MaxVelocity = 200;
        efekti.MinLifetime = 1;
        efekti.MaxLifetime = 2;
        Game.Instance.Add(efekti);
        efekti.AddEffect(this.X, this.Y, 50);
        Timer.SingleShot(1, peli.MainMenu);
    }
}