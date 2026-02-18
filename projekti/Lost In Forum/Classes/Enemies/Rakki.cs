using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jypeli;
using System.Threading.Tasks;

namespace Lost_In_Forum;
class Rakki : Enemy
{
    Timer dashTimer = new Timer(2.7);
    bool liikkuu;
    Vector dir;
    public Rakki() : base(150, 150)
    {
        this.Danger = 3;
        dashTimer.Timeout += Dash;
        speed = 26;
        hp.MaxValue = 6;
        hp.DefaultValue = 6;
        dashTimer.Start();
        this.Collided += ContactDamage;
        this.KbPower = 40000;
    }
    void Dash()
    {
        if (Target == null)
            return;
        this.Color = Color.Red;
        Timer.SingleShot(0.3, delegate
        {
            liikkuu = true;
            this.Color = Color.White;
        });
        Timer.SingleShot(0.9, delegate
        {
            liikkuu = false;
        });
        dir = (Target.Position - this.Position).Normalize();
    }
    public override void AI()
    {
        base.AI();
        if (liikkuu && Target != null && Target.Position != this.Position)
        {
            this.Position += dir * speed;
        }
    }
}