using Jypeli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lost_In_Forum;
public static class UI
{
    public static class HealthBar
    {
        public static double HeartSize { get; set; } = 150;
        public static List<Heart> Hearts = new List<Heart>();
        public class Heart : GameObject
        {
            public Heart() : base(HeartSize, HeartSize)
            {
                this.Image = Game.LoadImage("sydän");
                this.Image.Scaling = ImageScaling.Nearest;
            }
        }
        public static void CreateHealthBar(Game game, Player player, Level level)
        {
            for (int i = 0; i < Hearts.Count;)
            {
                Hearts[i].Destroy();
                Hearts.RemoveAt(i);
            }
            for (int i = 0; i < player.Hp.MaxValue; i++)
            {
                Heart h = new Heart();
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
}