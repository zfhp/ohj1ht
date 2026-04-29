using Jypeli;
using System.Collections.Generic;

namespace Lost_In_Forum;
class Room
{
    public List<PhysicsObject> Layout { get; set; } = new List<PhysicsObject>();
    public List<Enemy> Enemies { get; set; } = new List<Enemy>();
    public int[] Exits { get; set; } = new int[4];
}