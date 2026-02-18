using FarseerPhysics.Collision;
using Jypeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace Lost_In_Forum;
public static class UI
{
    public static class HealthBar
    {
        public static double HeartSize { get; set; } = 150;
        public static List<GameObject> Hearts = new List<GameObject>();
        public static void CreateHealthBar(Game game, Player player, Level level)
        {
            for (int i = 0; i < Hearts.Count;)
            {
                Hearts[i].Destroy();
                Hearts.RemoveAt(i);
            }
            for (int i = 0; i < player.Hp.MaxValue; i++)
            {
                GameObject h = new GameObject(HeartSize, HeartSize);
                h.Image = Game.LoadImage("Sydän");
                h.Image.Scaling = ImageScaling.Nearest;
                h.Position = new Vector(level.Left + h.Width * (i + 1) * 1.2, level.Top - h.Height);
                Hearts.Add(h);
                game.Add(h, 4);
            }
        }
        public static void UpdateHealthBar(Player player)
        {
            for (int i = 0; i < Hearts.Count; i++)
            {
                if (!player.IsAddedToGame)
                    Hearts[i].Image = Game.LoadImage("tyhjäsydän");
                else if (i < player.Hp.Value)
                {
                    Hearts[i].Image = Game.LoadImage("sydän");
                }
                else
                {
                    Hearts[i].Image = Game.LoadImage("tyhjäsydän");
                }
                Hearts[i].Image.Scaling = ImageScaling.Nearest;
            }
        }
    }
    public static class MiniMap
    {
        public static List<GameObject> map = new List<GameObject>();
        public static GameObject mapBg;
        public static void UpdateMap()
        {
            map.Clear();
            Level k = Game.Instance.Level;
            if(mapBg == null)
            {
                mapBg = new GameObject(k.Width / 64 * Rooms.MapSize, k.Height / 40 * Rooms.MapSize, Shape.Rectangle);
                mapBg.Color = Color.LightGray;
                mapBg.Position = new Vector(k.Right - k.Width / 5 , k.Top - k.Height / 5);
                mapBg.Image = Image.FromColor((int)mapBg.Width + 1, (int)mapBg.Height + 1, mapBg.Color);
            }
            int[] pos = Rooms.CurrentPos;
            List<RoomData> l = new List<RoomData>();
            for (int y = 0; y < Rooms.map.GetLength(0); y++) 
            {
                for (int x = 0; x < Rooms.map.GetLength(1); x++)
                {
                    bool neighboringCleared = false;
                    RoomData room = Rooms.map[y, x];
                    if (room != null)
                    {
                        for (int i = -1; i< 2; i += 2)
                        {
                            if (Rooms.map[Math.Clamp(y + i, 0, Rooms.map.GetLength(0) - 1), x] != null)
                                if (Rooms.map[Math.Clamp(y + i, 0, Rooms.map.GetLength(0) - 1), x].Cleared || (x == pos[0] && (y + i) == pos[1]))
                                    neighboringCleared = true;
                            if (Rooms.map[y, Math.Clamp(x + i, 0, Rooms.map.GetLength(1) - 1)] != null)
                                if (Rooms.map[y, Math.Clamp(x + i, 0, Rooms.map.GetLength(1) - 1)].Cleared || ((x + i) == pos[0] && y == pos[1]))
                                    neighboringCleared = true;
                        }
                        if (room.Cleared || (pos[1] == y && pos[0] == x) || neighboringCleared)
                        {
                            GameObject g = new GameObject(mapBg.Width / Rooms.MapSize * 0.8, mapBg.Height / Rooms.MapSize * 0.8, Shape.Rectangle);
                            if (pos[1] == y && pos[0] == x)
                                g.Color = Color.White;
                            else if(room.Cleared)
                                g.Color = Color.Gray;
                            else
                                g.Color = Color.DarkGray;
                            double posX = mapBg.Left + (x * (mapBg.Width / Rooms.MapSize)) + (g.Width / 2);
                            double posY = mapBg.Top - (y * (mapBg.Height / Rooms.MapSize)) - (g.Height / 2);
                            g.Image = Image.FromColor((int)g.Width + 1, (int)g.Height + 1, g.Color);
                            g.Position = new Vector(posX, posY);
                            map.Add(g);
                        }
                    }

                }
            }
        }
    }
}