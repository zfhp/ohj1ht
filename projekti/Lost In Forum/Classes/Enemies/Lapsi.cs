namespace Lost_In_Forum;
class Lapsi : Enemy
{
    public Lapsi() : base(100, 100)
    {
        this.Collided += ContactDamage;
        Hp.MaxValue = 3;

        Hp.DefaultValue = 3;
    }
    public override void Ai()
    {
        base.Ai();
        if (Target != null && Target.Position != this.Position)
            this.Position += (Target.Position - this.Position).Normalize() * Speed;
    }
}