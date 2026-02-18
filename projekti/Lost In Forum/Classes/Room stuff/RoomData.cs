using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lost_In_Forum;
public class RoomData()
{
    public bool Cleared { set; get; }
    public List<ObjectData> Layout { get; set; } = new List<ObjectData>();
    public List<ObjectData> Enemies { get; set; } = new List<ObjectData>();
    public int[] Exits { get; set; } = new int[4];
}