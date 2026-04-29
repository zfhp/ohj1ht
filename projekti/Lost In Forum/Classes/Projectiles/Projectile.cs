using Jypeli;

namespace Lost_In_Forum;
class Projectile : PhysicsObject
{
    public Projectile(double size, double speed, Vector dir, PhysicsObject o) : base(size, size, Shape.Circle)
    {
        this._speed = speed;
        this._dir = dir;
        this.CanRotate = false;
        this.Collided += Coll;
        Timer.SingleShot(LifeTime, delegate { this.Destroy(); });
        _owner = o;
    }
    PhysicsObject _owner;
    public bool Friendly { get; set; }
    public int Damage { get; set; } = 1;
    public double LifeTime = 3;
    double _speed;
    Vector _dir;
    public virtual void Ai()
    {
        this.Position += _dir * _speed;
    }
    public override void Update(Time time)
    {
        Ai();
    }
    void Coll(IPhysicsObject o, IPhysicsObject t)
    {
        if (t is Player p && !this.Friendly)
        {
            p.TakeDamage(Damage);
        }
        if (t is Enemy e && this.Friendly)
        {
            e.TakeDamage(Damage);
        }
        if (t != this._owner && t.Tag.ToString() != "Punch")
            this.Destroy();
    }
}
