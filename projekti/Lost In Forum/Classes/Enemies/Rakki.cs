using Jypeli;

namespace Lost_In_Forum;
class Rakki : Enemy
{
    Timer _dashTimer = new Timer(2.7);
    bool _liikkuu;
    Vector _dir;
    public Rakki() : base(100, 100)
    {
        this.Danger = 2;
        this.IsVisible = false;
        this.KbPower = 9000;
        _dashTimer.Timeout += Dash;
        Speed = 26;
        Hp.MaxValue = 5;
        Hp.DefaultValue = 5;
        _dashTimer.Start();
        this.Collided += ContactDamage;
        this.Sprite.Size = new Vector(150, 150);
        this.Sprite.Y = this.Bottom + this.Sprite.Height/2;
        Image[] i = Game.LoadImages("RakkiIdle", "RakkiWindup", "RakkiCharge");
        foreach (Image img in i)
            img.Scaling = ImageScaling.Nearest;
        this.Sprite.Animation = new Animation(i);
        this.HealthBarBg.Size = new Vector(this.Sprite.Width, this.Sprite.Height/10);
        this.HealthBar.Size = this.HealthBarBg.Size;
    }
    void Dash()
    {
        if (Target == null)
            return;
        this.Sprite.Animation.Step();
        Timer.SingleShot(0.3, delegate
        {
            _liikkuu = true;
            this.Color = Color.White;
            this.Sprite.Animation.Step();
        });
        Timer.SingleShot(0.9, delegate
        {
            _liikkuu = false;
            this.Sprite.Animation.Step();
        });
        _dir = (Target.Position - this.Position).Normalize();
    }
    public override void Ai()
    {
        base.Ai();
        if (_liikkuu && Target != null && Target.Position != this.Position)
        {
            this.Position += _dir * Speed;
        }
    }
}