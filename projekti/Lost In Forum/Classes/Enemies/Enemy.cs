using Jypeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lost_In_Forum;

public class Enemy : PhysicsObject
{
    public Enemy(double width, double height) : base(width, height, Shape.Rectangle)
    {
        hp.LowerLimit += Die;
        CollisionIgnoreGroup = 67;
        CanRotate = false;
        LinearDamping = 6;
        Target = Game.Instance.GetFirstObject(p => p is Player) as Player;
        Color = Color.Red;
        healthBarBg = new GameObject(this.Width, this.Height / 10, Shape.Rectangle);
        healthBarBg.Image = Image.FromColor((int)healthBarBg.Width + 2, (int)healthBarBg.Height + 1, Color.Black);
        healthBarBg.Y = this.Bottom - healthBarBg.Height * 1.5;
        healthBar = new GameObject(this.Width, this.Height / 10, Shape.Rectangle);
        healthBar.Image = Image.FromColor((int)healthBar.Width + 2, (int)healthBar.Height + 1, Color.Red);
        healthBar.Y = healthBarBg.Y;
        this.Add(healthBarBg);
        healthBarBg.Add(healthBar);
    }
    public GameObject healthBarBg { get; }
    public GameObject healthBar { get; }
    public Player Target { get; set; }
    public double speed { get; set; } = 5;
    public double KbPower { get; set; } = 10000;
    public bool immune { get; set; }
    public IntMeter hp { get; set; } = new IntMeter(3, 0, 3);
    public int damage { get; set; } = 1;
    public int Danger { get; set; } = 1;


    public virtual void AI()
    {
        if (Target == null)
        {
            Target = Game.Instance.GetFirstObject(p => p is Player) as Player;
            return;
        }
    }


    public override void Update(Time time)
    {
        AI();
        base.Update(time);
    }
    public virtual void Die()
    {
        Rooms.CurrentRoom.Enemies.Remove(this);
        Destroy();
    }
    public virtual void TakeDamage(int dmg)
    {
        hp.AddValue(-dmg);
        IsVisible = false;
        healthBar.Width = healthBarBg.Width * ((double)hp.Value / hp.MaxValue);
        healthBar.RelativeLeft = healthBarBg.RelativeLeft;
        healthBar.Image = Image.FromColor((int)healthBar.Width + 2, (int)healthBar.Height + 2, Color.Red);
        Timer.SingleShot(0.02, delegate { IsVisible = true; });
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
            Target.TakeDamage(damage);
        }
    }
}