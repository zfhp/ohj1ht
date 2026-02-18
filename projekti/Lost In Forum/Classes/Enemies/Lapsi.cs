using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lost_In_Forum;
class Lapsi : Enemy
{
    public Lapsi() : base(100, 100)
    {
        this.Collided += ContactDamage;
        hp.MaxValue = 3;

        hp.DefaultValue = 3;
    }
    public override void AI()
    {
        base.AI();
        if (Target != null && Target.Position != this.Position)
            this.Position += (Target.Position - this.Position).Normalize() * speed;
    }
}