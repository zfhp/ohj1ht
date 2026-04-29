using Jypeli;

namespace Lost_In_Forum;

public class Enemy : PhysicsObject
{
    public Enemy(double width, double height) : base(width, height, Shape.Rectangle)
    {
        Hp.LowerLimit += Die;
        base.CollisionIgnoreGroup = 67;
        CanRotate = false;
        LinearDamping = 6;
        Target = Game.Instance.GetFirstObject(p => p is Player) as Player;
        Sprite.Size = base.Size;
        Sprite.Image = Image.FromColor((int)width, (int)height, Color.Red);
        HealthBarBg = new GameObject(this.Sprite.Width, this.Height / 10, Shape.Rectangle);
        HealthBarBg.Image = Image.FromColor((int)HealthBarBg.Width + 2, (int)HealthBarBg.Height + 1, Color.Black);
        HealthBarBg.Y = this.Bottom - HealthBarBg.Height * 1.5;
        HealthBar = new GameObject(this.Width, this.Height / 10, Shape.Rectangle);
        HealthBar.Image = Image.FromColor((int)HealthBar.Width + 2, (int)HealthBar.Height + 1, Color.Red);
        HealthBar.Y = HealthBarBg.Y;
        this.Add(HealthBarBg);
        this.Add(Sprite);
        HealthBarBg.Add(HealthBar);
    }
    public GameObject HealthBarBg { get; }
    public GameObject HealthBar { get; }
    public GameObject Sprite { get; set; } = new GameObject(1,1);
    public Player Target { get; set; }
    public double Speed { get; set; } = 5;
    public double KbPower { get; set; } = 10000;
    public bool Immune { get; set; }
    public IntMeter Hp { get; set; } = new IntMeter(3, 0, 3);
    public int Damage { get; set; } = 1;
    public int Danger { get; set; } = 1;


    public virtual void Ai()
    {
        if (Target == null)
        {
            Target = Game.Instance.GetFirstObject(p => p is Player) as Player;
        }
    }


    public override void Update(Time time)
    {
        Ai();
        base.Update(time);
    }
    public virtual void Die()
    {
        Rooms.CurrentRoom.Enemies.Remove(this);
        Destroy();
    }
    public virtual void TakeDamage(int dmg)
    {
        Hp.AddValue(-dmg);
        Sprite.IsVisible = false;
        HealthBar.Width = HealthBarBg.Width * ((double)Hp.Value / Hp.MaxValue);
        HealthBar.RelativeLeft = HealthBarBg.RelativeLeft;
        HealthBar.Image = Image.FromColor((int)HealthBar.Width + 2, (int)HealthBar.Height + 2, Color.Red);
        Timer.SingleShot(0.02, delegate { Sprite.IsVisible = true; });
    }
    public void Knockback(Vector dir)
    {
        Hit(dir * KbPower);
    }
    /// <summary>
    /// Tekee pelaajaan vahinkoa tämän vihollisen osuessa pelaajaan. Käytä yhdistämällä vihollisen Collided-tapahtumaan tämä aliohjelma.
    /// </summary>
    public void ContactDamage(IPhysicsObject collider, IPhysicsObject target)
    {
        if (target.Tag.ToString() == "Player")
        {
            Target.TakeDamage(Damage);
        }
    }
}