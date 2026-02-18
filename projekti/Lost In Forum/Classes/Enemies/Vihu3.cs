using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using System.Threading.Tasks;

namespace Lost_In_Forum;
class TestiVihu3 : Enemy
{
    public TestiVihu3() : base(120, 120)
    {
        this.Danger = 2;
        Timer t = new Timer(1.6, Shoot);
        t.Start();
    }
    void Shoot()
    {
        if (Target == null || !this.IsAddedToGame)
            return;
        Projectile p = new Projectile(50, 15, (Target.Position - this.Position).Normalize(), this);
        p.Friendly = false;
        p.CollisionIgnoreGroup = 67;
        p.Position = this.Position;
        Game.Instance.Add(p);
    }
}