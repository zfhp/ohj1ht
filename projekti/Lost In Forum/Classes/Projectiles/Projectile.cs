using Jypeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lost_In_Forum;
class Projectile : PhysicsObject
{
    public Projectile(double size, double speed, Vector dir, PhysicsObject o) : base(size, size, Shape.Circle)
    {
        this.speed = speed;
        this.dir = dir;
        this.CanRotate = false;
        this.Collided += coll;
        Timer.SingleShot(LifeTime, delegate { this.Destroy(); });
        owner = o;
    }
    PhysicsObject owner;
    public bool Friendly { get; set; }
    public int Damage { get; set; } = 1;
    public double LifeTime = 3;
    double speed;
    Vector dir;
    public virtual void AI()
    {
        this.Position += dir * speed;
    }
    public override void Update(Time time)
    {
        AI();
    }
    void coll(IPhysicsObject o, IPhysicsObject t)
    {
        if (t is Player p && !this.Friendly)
        {
            p.TakeDamage(Damage);
        }
        if (t is Enemy e && this.Friendly)
        {
            e.TakeDamage(Damage);
        }
        if (t != this.owner && t.Tag.ToString() != "Punch")
            this.Destroy();
    }
}
