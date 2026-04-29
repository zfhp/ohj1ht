using Jypeli;
using System;
using System.Collections.Generic;

namespace Lost_In_Forum;
public static class Ui
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
        public static List<GameObject> Map = new List<GameObject>();
        public static GameObject MapBg;
        public static void UpdateMap()
        {
            Map.Clear();
            Level k = Game.Instance.Level;
            if(MapBg == null)
            {
                MapBg = new GameObject(k.Width / 64 * Rooms.MapSize, k.Height / 40 * Rooms.MapSize, Shape.Rectangle);
                MapBg.Color = Color.LightGray;
                MapBg.Position = new Vector(k.Right - k.Width / 5 , k.Top - k.Height / 5);
                MapBg.Image = Image.FromColor((int)MapBg.Width + 1, (int)MapBg.Height + 1, MapBg.Color);
            }
            int[] pos = Rooms.CurrentPos;
            for (int y = 0; y < Rooms.Map.GetLength(0); y++) 
            {
                for (int x = 0; x < Rooms.Map.GetLength(1); x++)
                {
                    bool neighboringCleared = false;
                    RoomData room = Rooms.Map[y, x];
                    if (room != null)
                    {
                        for (int i = -1; i< 2; i += 2)
                        {
                            if (Rooms.Map[Math.Clamp(y + i, 0, Rooms.Map.GetLength(0) - 1), x] != null)
                                if (Rooms.Map[Math.Clamp(y + i, 0, Rooms.Map.GetLength(0) - 1), x].Cleared || (x == pos[0] && (y + i) == pos[1]))
                                    neighboringCleared = true;
                            if (Rooms.Map[y, Math.Clamp(x + i, 0, Rooms.Map.GetLength(1) - 1)] != null)
                                if (Rooms.Map[y, Math.Clamp(x + i, 0, Rooms.Map.GetLength(1) - 1)].Cleared || ((x + i) == pos[0] && y == pos[1]))
                                    neighboringCleared = true;
                        }
                        if (room.Cleared || (pos[1] == y && pos[0] == x) || neighboringCleared)
                        {
                            GameObject g = new GameObject(MapBg.Width / Rooms.MapSize * 0.8, MapBg.Height / Rooms.MapSize * 0.8, Shape.Rectangle);
                            if (pos[1] == y && pos[0] == x)
                                g.Color = Color.White;
                            else if(room.Cleared)
                                g.Color = Color.Gray;
                            else
                                g.Color = Color.DarkGray;
                            double posX = MapBg.Left + (x * (MapBg.Width / Rooms.MapSize)) + (g.Width / 2);
                            double posY = MapBg.Top - (y * (MapBg.Height / Rooms.MapSize)) - (g.Height / 2);
                            g.Image = Image.FromColor((int)g.Width + 1, (int)g.Height + 1, g.Color);
                            g.Position = new Vector(posX, posY);
                            Map.Add(g);
                        }
                    }

                }
            }
        }
    }
}