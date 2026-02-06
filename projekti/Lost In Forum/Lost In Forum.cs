using FarseerPhysics.Common;
using FarseerPhysics.Dynamics.Contacts;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using Microsoft.Extensions.DependencyModel.Resolution;
using Silk.NET.Core;
using Silk.NET.OpenGL;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Transactions;
using System.Xml.Serialization;

/// @author gr313108
/// @version 21.11.2025
/// <summary>
/// 
/// </summary>

/// <summary>
/// Sis‰lt‰‰ arvoja pelaajan ohjaamalle hahmolle
/// </summary>


public class Player : PhysicsObject
{
    public Player() : base(50,50, Shape.Octagon)
    {
        this.MaxVelocity = 0;
    }
    public GameObject Sprite { get; set; }
    bool immune;
    public double punchInterval { get; set; } = 0.5;
    public Vector punchdir { get; set; }
    public bool Punching { get; set; }
    public int Dmg { get; set; } = 1;
    public IntMeter Hp { get; set; } = new IntMeter(3, 0, 3);
    public double Speed { get; set; } = 7;
    public Animation Walk_S { get; set; }
    public Animation Walk_N { get; set; }
    public Animation Walk_E { get; set; }
    public Animation Walk_W { get; set; }
    public Animation Punch_R { get; set; }
    public Animation Punch_L { get; set; }
    public Animation Punch_U { get; set; }
    public Animation Punch_D { get; set; }
    public Animation Idle { get; set; }
    public void TakeDamage(int dmg)
    {
        if (immune)
            return;
        Hp.AddValue(-dmg);
        immune = true;
        UI.HealthBar.UpdateHealthBar(this);
        Timer flashTimer = new Timer(0.1, delegate { this.Sprite.IsVisible = !this.Sprite.IsVisible; });
        flashTimer.Start();
        Timer.SingleShot(0.75, delegate { immune = false; flashTimer.Stop(); this.Sprite.IsVisible = true; });
    }
    public void Walk(Vector dir)
    {
        if (Punching) return;
        this.Position += (dir * Speed);
    }
    public void Die()
    {
        this.Destroy();
        Speed = 0;
    }
}
class Projectile : PhysicsObject
{
    public Projectile(double size, double speed, Vector dir, PhysicsObject o) : base(size, size, Shape.Circle)
    {
        this.speed = speed;
        this.dir = dir;
        this.CanRotate = false;
        this.Collided += coll;
        Timer.SingleShot(LifeTime, delegate { this.Destroy(); });
        owner = o;
    }
    PhysicsObject owner;
    public bool Friendly {  get; set; }
    public int Damage { get; set; } = 1;
    public double LifeTime = 3;
    double speed;
    Vector dir;
    public virtual void AI()
    {
        this.Position += dir * speed;
    }
    public override void Update(Time time)
    {
        AI();
    }
    void coll(IPhysicsObject o, IPhysicsObject t)
    {
        if(t is Player p && !this.Friendly)
        {
            p.TakeDamage(Damage);
        }
        if(t is Enemy e && this.Friendly)
        {
            e.TakeDamage(Damage);
        }
        if(t != this.owner && t.Tag.ToString() != "Punch")
            this.Destroy();
    }
}
class Enemy : PhysicsObject
{
    public Enemy(double width, double height) : base(width, height, Shape.Rectangle)
    {
        hp.LowerLimit += Die;
        this.CollisionIgnoreGroup = 67;
        this.CanRotate = false;
        this.LinearDamping = 6;
    }


    public Player Target { get; set; }
    public double speed { get; set; } = 5;
    public double KbPower { get; set; } = 10000;
    public bool immune { get; set; }
    public  IntMeter hp { get; set; } = new IntMeter(3,0,3);
    public string type { get; set; }
    public int damage { get; set; } = 1;


    public virtual void AI()
    {
        
    }


    public override void Update(Time time)
    {
        AI();
        base.Update(time);
    }
    public virtual void Die()
    {
        this.Destroy();
    }
    public virtual void TakeDamage(int dmg)
    {
        hp.AddValue(-dmg);
        this.IsVisible = false;
        Timer.SingleShot(0.02, delegate{ this.IsVisible = true; });
    }
    public void Knockback(Vector dir)
    {
        this.Hit(dir * KbPower);
    }
    /// <summary>
    /// Tekee pelaajaan vahinkoa t‰m‰n vihollisen osuessa pelaajaan. K‰yt‰ yhdist‰m‰ll‰ vihollisen Collided-tapahtumaan t‰m‰ aliohjelma.
    /// </summary>
    public void ContactDamage(IPhysicsObject collider, IPhysicsObject target)
    {
         if (target.Tag.ToString() == "Player")
        {
            Target.TakeDamage(damage);
        }
    }
}
class TestiVihu3 : Enemy
{
    public TestiVihu3() : base(120,120)
    {
        Timer t = new Timer(1, Shoot);
        t.Start();
    }
    void Shoot()
    {
        if (Target == null || !this.IsAddedToGame)
            return;
        Projectile p = new Projectile(50, 5, (Target.Position - this.Position).Normalize(), this);
        p.Friendly = false;
        p.CollisionIgnoreGroup = 67;
        p.Position = this.Position;
        Game.Instance.Add(p);
    }
}
class TestiVihu1 : Enemy
{
    public TestiVihu1() : base(100,100)
    {
        this.Collided += ContactDamage;
        hp.MaxValue = 3;
        
        hp.DefaultValue = 3;    
    }
    public override void AI()
    {
        if (Target != null && Target.Position != this.Position)
            this.Position += (Target.Position - this.Position ).Normalize() * speed;
    }
}
class TestiVihu2 : Enemy
{
    Timer pissa = new Timer(2.7);
    bool liikkuu;
    Vector dir;
    public TestiVihu2() : base (150, 150)
    {
        pissa.Timeout += delegate { this.Color = Color.Red; Timer.SingleShot(0.3, delegate { liikkuu = true; this.Color = Color.White; }); Timer.SingleShot(0.9, delegate { liikkuu = false; }); dir = (Target.Position - this.Position).Normalize(); };
        speed = 26;
        hp.MaxValue = 6;
        hp.DefaultValue = 6;
        pissa.Start();
        this.Collided += ContactDamage;
        this.KbPower = 40000;
    }
    public override void AI()
    {
        if(liikkuu && Target != null && Target.Position != this.Position)
        {
            this.Position += dir * speed;
        }
    }
}
class Rooms 
{
    public static int MapSize = 13;
    public static int StartRoom = 7;
    public static int[] CurrentPos { get; set; } = new int[2] {StartRoom, StartRoom};
    public static RoomData[,] map;
    public static Room CurrentRoom { get; set; }
    public static List<RoomData> AllRooms { get; set; } = new List<RoomData>();
    public static void GetAllRooms()
    {
        if (!Directory.Exists("Rooms"))
        {
            Game.Instance.MessageDisplay.Add("DIRECTORY NOT FOUDN");
            return;
        }
        string[] paths = Directory.GetFiles("Rooms");
        foreach (string path in paths)
        {
            string jsonString = File.ReadAllText(path);
            RoomData room = JsonSerializer.Deserialize<RoomData>(jsonString);
            AllRooms.Add(room);
        }
    }
    public static void SetRooms()
    {
        for (int i = 0; i < MapSize; i++)
        {
            for (int x = 0; x < MapSize; x++)
            {
                if (map[i, x] != null)
                {

                    if (i > 0 && map[i - 1, x] != null)
                        map[i, x].Exits[0] = 1;
                    if (i < MapSize - 1 && map[i + 1, x] != null)
                        map[i, x].Exits[2] = 1;
                    if (x > 0&& map[i, x - 1] != null)
                        map[i, x].Exits[3] = 1;
                    if (x < MapSize - 1 &&map[i, x + 1] != null)
                        map[i, x].Exits[1] = 1;
                    List<RoomData> ViableRooms = new List<RoomData>();
                    foreach (RoomData room in AllRooms)
                    {
                        if (room.Exits.SequenceEqual(map[i, x].Exits))
                            ViableRooms.Add(room);
                    }
                    if (ViableRooms.Count > 0)
                        map[i, x] = ViableRooms[RandomGen.NextInt(ViableRooms.Count)];
                }
            }
        }
    }
    public static void CreateMap()
    {
        map = new RoomData[MapSize, MapSize];
        map[StartRoom, StartRoom] = new RoomData();
        int[] selection = new int[2] { 0, 0 };
        double i = MapSize;
        int safetynet = 0;
        while (CountRooms() < 2 * MapSize && safetynet < 250)
        {
            MakeWorm(GetRandomRoom(), 3, 7);
            safetynet++;
        }
        
        static int CountRooms()
        {
            int count = 0;
            foreach (var r in map) if (r != null) count++;
            return count;
        }
        int[] GetRandomRoom()
        {
            List<int[]> validRooms = new List<int[]>();
            for (int row = 0; row < map.GetLength(0); row++)
            {
                for (int col = 0; col < map.GetLength(1); col++)
                {
                    if (map[row, col] != null)
                    {
                        validRooms.Add(new int[] { row, col });
                    }
                }
            }
            if (validRooms.Count == 0) return null;
            int[]pos = validRooms[RandomGen.NextInt(validRooms.Count)];
            selection = new int[] { pos[0], pos[1] }    ;
            return pos;
        }
        void MakeWorm(int[] startpos, int min, int max)
        {
            selection[0] = startpos[0];
            selection[1] = startpos[1];
            int[] dir = RandomGen.SelectOne<int[]>(new int[] {0,1}, new int[] { 0, -1 }, new int[] { 1,0 }, new int[] { -1,0});
            for (int i = 0; i < RandomGen.NextInt(min, max + 1); i++)
            {
                selection[0] += dir[0];
                selection[1] += dir[1];
                if(RandomGen.NextInt(2) == 0)
                {
                    dir = RandomGen.SelectOne<int[]>(new int[] { 0, 1 }, new int[] { 0, -1 }, new int[] { 1, 0 }, new int[] { -1, 0 });
                    i--;
                }
                if (selection[0] >= 0 && selection[0] < map.GetLength(0) && selection[1] >= 0 && selection[1] < map.GetLength(1))
                {
                    map[selection[0], selection[1]] = new RoomData();
                }
                else
                    break;
            }
        }
}
    public static void LoadRoom(Game game, RoomData room)
    {
        try
        {
            if (CurrentRoom != null)
            {
                for (int i = CurrentRoom.Layout.Count - 1; i >= 0; i--)
                {
                    CurrentRoom.Layout[i].Destroy();
                }
            }
            CurrentRoom = new Room();
            foreach (ObjectData obj in room.Layout)
            {
                PhysicsObject block = new PhysicsObject(game.Level.Width/32, game.Level.Height/20, obj.X, obj.Y);
                block.MakeStatic();
                game.Add(block);
                CurrentRoom.Layout.Add(block);
            }
            CurrentRoom.Exits = room.Exits;
        }
        catch (Exception e)
        { 
            System.Diagnostics.Debug.WriteLine("Error loading room: " + e.Message);
        }
    }
    static RoomData GetRoomData(string path)
    {
        string jsonString = File.ReadAllText(path);
        RoomData room = JsonSerializer.Deserialize<RoomData>(jsonString);
        return room;
    }
}
public static class UI
{
    public static class HealthBar
    {
        public static double HeartSize { get; set; } = 150;
        static List<Heart> Hearts = new List<Heart>();
        public class Heart : GameObject
        {
            public Heart() : base(HeartSize, HeartSize)
            {
                this.Image = Game.LoadImage("syd‰n");
                this.Image.Scaling = ImageScaling.Nearest;
            }
        }
        public static void CreateHealthBar(Game game, Player player, Level level)
        {
            for (int i = 0; i < player.Hp.MaxValue; i++)
            {
                Heart h = new Heart();
                h.Position = new Vector(level.Left + h.Width * (i+1) * 1.2, level.Top - h.Height);
                Hearts.Add(h);
                game.Add(h);
            }
        }
        public static void UpdateHealthBar(Player player)
        {
            for (int i = 0; i < Hearts.Count; i++)
            {
                if (!player.IsAddedToGame)
                    Hearts[i].Image = Game.LoadImage("tyhj‰syd‰n");
                else if (i < player.Hp.Value)
                {
                    Hearts[i].Image = Game.LoadImage("syd‰n");
                }
                else
                {
                    Hearts[i].Image = Game.LoadImage("tyhj‰syd‰n");
                }
                Hearts[i].Image.Scaling = ImageScaling.Nearest;
            }
        }
    }
}
public class ObjectData
{
    public string Type { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string ImageName { get; set; }
}
class Room
{
    public List<PhysicsObject> Layout { get; set; } = new List<PhysicsObject>();
    public List<Enemy> Enemies { get; set; }
    public int[] Exits { get; set; } = new int[4];
}
public class RoomData()
{
    public List<ObjectData> Layout { get; set; } = new List<ObjectData>();
    public int[] Exits { get; set; } = new int[4];
}
public class Lost_In_Forum : PhysicsGame
{
    Player player;
    bool GamePlaying = false;
    /// <summary>
    /// Asettaa kent‰n koon ja kameran/ikkunan asetuksia
    /// </summary>
    void SetupWindow()
    {
        SetWindowSize(1920, 1200);
        Level.Size = new Jypeli.Vector(1920, 1200);
        IsFullScreen = true;
        Camera.ZoomToLevel();
        Level.BackgroundColor = Color.Black;
        Game.UpdatesPerSecod = 59;
    }
    /// <summary>
    /// Luo pelaajan.
    /// </summary>
    void CreatePlayer()
    {
        player = new Player();
        player.Tag = "Player";
        player.Hp.LowerLimit += player.Die;
        player.IgnoresGravity = true;
        player.CollisionIgnoreGroup = 1;
        player.MaxVelocity = 0;
        player.CanRotate = false;
        player.IsVisible = false;
        Add(player);
        player.Sprite = new GameObject(Level.Width / 9.6, Level.Height / 6, Shape.Rectangle);
        player.Sprite.Y = player.Bottom + player.Sprite.Height / 2;
        player.Add(player.Sprite);
    }
    /// <summary>
    /// Luo pelin ohjaukset.
    /// </summary>
    void CreateControls()
    {
        Keyboard.ListenArrows(ButtonState.Pressed, Punch, "");
        Keyboard.Listen(Key.W, ButtonState.Down, player.Walk, "", new Jypeli.Vector(0, 1));
        Keyboard.Listen(Key.A, ButtonState.Down, player.Walk, "", new Jypeli.Vector(-1, 0));
        Keyboard.Listen(Key.S, ButtonState.Down, player.Walk, "", new Jypeli.Vector(0, -1));
        Keyboard.Listen(Key.D, ButtonState.Down, player.Walk, "", new Jypeli.Vector(1, 0));
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }
    /// <summary>
    /// Siirt‰‰ oliota a m‰‰r‰ll‰ speed suuntaan dir.
    /// </summary>
    /// <param name="a">Olio, jota siirret‰‰n.</param>
    /// <param name="speed">Nopeus, jolla oliota siirret‰‰n.</param>
    /// <param name="dir">Suunta, johon oliota siirret‰‰n.</param>

    void Punch(Vector v)
    {
        if (player.Punching) return;
        PhysicsObject p = new PhysicsObject(200, 200, Shape.Rectangle);
        p.Position = player.Sprite.Position;
        p.Collided += PunchCollision;
        player.punchdir = v;
        Image[] i;
        if (v.X > 0)
        {
            i = Game.LoadImages("KoditonPunch_RHand1", "KoditonPunch_RHand2");
            p.X += p.Width / 2;
        }
        else if (v.X < 0)
        {
            i = Game.LoadImages("KoditonPunch_LHand1", "KoditonPunch_LHand2");
            p.X -= p.Width / 2;
        }
        else if(v.Y > 0)
        {
            i = Game.LoadImages("KoditonPunch_UHand1", "KoditonPunch_UHand2");
            p.Y += p.Height / 3;
        }
        else if(v.Y < 0)
        {
            i = Game.LoadImages("KoditonPunch_DHand1", "KoditonPunch_DHand2");
            p.Y -= p.Height / 2;
        }

        else i = new Image[1] { Game.LoadImage("norsu") };
            player.Punching = true;
        foreach (Image x in i)
            x.Scaling = ImageScaling.Nearest;
        p.Animation = new Animation(i);
        p.Animation.IsPlaying = true;
        p.Animation.FPS = 10;
        p.Animation.StopOnLastFrame = true;
        p.Animation.Played += p.Destroy;
        p.Tag = "Punch";
        p.MakeStatic();
        p.CollisionIgnoreGroup = 1;
        p.IgnoresCollisionResponse = true;
        Add(p, 4);
        Timer.SingleShot(player.punchInterval, EndPunch);
    }
    void PunchCollision(IPhysicsObject o, IPhysicsObject t)
    {
        if (t is Enemy e)
        {
            e.TakeDamage(player.Dmg);
            e.Knockback(player.punchdir);
        }
    }
    void EndPunch()
    {
        player.Punching = false;
    }
    
    /// <summary>
    /// Luo animaatiot.
    /// </summary>
    void CreateAnimations()
    {
        Image idle = Game.LoadImage("KoditonIdle");
        idle.Scaling = ImageScaling.Nearest;
        player.Idle = new Animation(idle);
        Image[] walk_E = Game.LoadImages("KoditonWalk_R1", "KoditonWalk_R2", "KoditonWalk_R3", "KoditonWalk_R2");
        foreach (Image i in walk_E)
            i.Scaling = ImageScaling.Nearest;
        player.Walk_E = new Animation(walk_E);
        player.Walk_E.FPS = 4;
        Image[] walk_W = Game.LoadImages("KoditonWalk_L1", "KoditonWalk_L2", "KoditonWalk_L3", "KoditonWalk_L2");
        foreach (Image i in walk_W)
            i.Scaling = ImageScaling.Nearest;
        player.Walk_W = new Animation(walk_W);
        player.Walk_W.FPS = 4;
        Image[] walk_N = Game.LoadImages("KoditonWalk_U1", "KoditonWalk_U2", "KoditonWalk_U3", "KoditonWalk_U2");
        foreach (Image i in walk_N)
            i.Scaling = ImageScaling.Nearest;
        player.Walk_N = new Animation(walk_N);
        player.Walk_N.FPS = 4;
        Image[] walk_S = Game.LoadImages("KoditonWalk_D1", "KoditonIdle", "KoditonWalk_D3", "KoditonIdle");
        foreach (Image i in walk_S)
            i.Scaling = ImageScaling.Nearest;
        player.Walk_S = new Animation(walk_S);
        player.Walk_S.FPS = 4;
        Image[] punch_R = Game.LoadImages("KoditonPunch_Rbody");
        foreach (Image i in punch_R)
            i.Scaling = ImageScaling.Nearest;
        player.Punch_R = new Animation(punch_R);
        player.Punch_R.FPS = 4;
        Image[] punch_L = Game.LoadImages("KoditonPunch_Lbody");
        foreach (Image i in punch_L)
            i.Scaling = ImageScaling.Nearest;
        player.Punch_L = new Animation(punch_L);
        player.Punch_L.FPS = 4;
        Image[] punch_U = Game.LoadImages("KoditonPunch_Ubody");
        foreach (Image i in punch_U)
            i.Scaling = ImageScaling.Nearest;
        player.Punch_U = new Animation(punch_U);
        player.Punch_U.FPS = 4;
        Image[] punch_D = Game.LoadImages("KoditonPunch_Dbody");
        foreach (Image i in punch_D)
            i.Scaling = ImageScaling.Nearest;
        player.Punch_D = new Animation(punch_D);
        player.Punch_D.FPS = 4;
    }
    /// <summary>
    /// Asettaa pelaajan hahmolle tilanteeseen sopivan animaation.
    /// </summary>
    void AnimatePlayer()
    {
        if (player.Punching)
        {
            if (player.punchdir.X > 0)
                player.Sprite.Animation = player.Punch_R;
            else if (player.punchdir.X < 0)
                player.Sprite.Animation = player.Punch_L;
            else if (player.punchdir.Y > 0)
                player.Sprite.Animation = player.Punch_U;
            else if (player.punchdir.Y < 0)
                player.Sprite.Animation = player.Punch_D;
            return;
        }
        if (Keyboard.IsKeyDown(Key.W))
        {
            player.Sprite.Animation = player.Walk_N;
            player.Sprite.Animation.IsPlaying = true;
        }
        else if (Keyboard.IsKeyDown(Key.S))
        {
            player.Sprite.Animation = player.Walk_S;
            player.Sprite.Animation.IsPlaying = true;
        }
        else if (Keyboard.IsKeyDown(Key.A))
        {
            player.Sprite.Animation = player.Walk_W;
            player.Sprite.Animation.IsPlaying = true;
        }
        else if (Keyboard.IsKeyDown(Key.D))
        {
            player.Sprite.Animation = player.Walk_E;
            player.Sprite.Animation.IsPlaying = true;
        }
        else
            player.Sprite.Animation = player.Idle;
    }
    /// <summary>
    /// Tarkistaa, ett‰ onko pelaaja poistumassa huoneesta ja siirt‰‰ pelaajan oikeaan huoneeseen.
    /// </summary>
    void CheckRoomExit()
    {
        if (player.Position.X > Level.Right)
        {
            Rooms.LoadRoom(Game.Instance, Rooms.map[Rooms.CurrentPos[1], Rooms.CurrentPos[0] + 1]);
            player.X = Level.Left + 100;
            Rooms.CurrentPos[0]++;
        }
        if (player.Position.X < Level.Left)
        {
            Rooms.LoadRoom(Game.Instance, Rooms.map[Rooms.CurrentPos[1], Rooms.CurrentPos[0] - 1]);
            player.X = Level.Right - 100;
            Rooms.CurrentPos[0]--;
        }
        if (player.Position.Y > Level.Top)
        {
            Rooms.LoadRoom(Game.Instance, Rooms.map[Rooms.CurrentPos[1] - 1, Rooms.CurrentPos[0]]);
            player.Y = Level.Bottom + 150;
            Rooms.CurrentPos[1]--;
        }
        if (player.Position.Y < Level.Bottom)
        {
            Rooms.LoadRoom(Game.Instance, Rooms.map[Rooms.CurrentPos[1] + 1, Rooms.CurrentPos[0]]);
            player.Y = Level.Top - 100;
            Rooms.CurrentPos[1]++;
        }
    }


    void MainMenu()
    {
        Level.BackgroundColor = Color.Gray;
        GameObject Title = new GameObject(599, 392, Shape.Rectangle);
        Title.Image = Game.LoadImage("FORUMTITLE");
        Title.Y = Level.Top - Title.Height / 1.5;
        GameObject playButton = new GameObject(200, 50, Shape.Rectangle);
        playButton.Y = Title.Bottom - 2 * playButton.Height;
        playButton.Image = Game.LoadImage("Play");
        Mouse.ListenOn(playButton, MouseButton.Left, ButtonState.Pressed, StartGame, "");
        Add(playButton);
        Add(Title);
    } 
    void StartGame()
    {
        ClearAll();
        SetupWindow();
        Rooms.GetAllRooms();
        Rooms.CreateMap();
        Rooms.SetRooms();
        Rooms.LoadRoom(Game.Instance, Rooms.map[Rooms.StartRoom, Rooms.StartRoom]);
        CreatePlayer();
        CreateAnimations();
        CreateControls();
        UI.HealthBar.CreateHealthBar(Game.Instance, player, Level);
        TestiVihu1 vihu = new TestiVihu1();
        vihu.Image = Game.LoadImage("norsu");
        vihu.Target = player;
        vihu.X = 300;
        Add(vihu);
        TestiVihu2 vihu2 = new TestiVihu2();
        vihu2.Target = player;
        vihu2.X = -300;
        Add(vihu2);
        TestiVihu3 a = new TestiVihu3();
        a.Target = player;
        a.X = 500;
        a.Y = 100;
        Add(a);
        Keyboard.Listen(Key.K, ButtonState.Pressed, delegate {
            TestiVihu2 v = new TestiVihu2();
            v.Target = player;
            Add(v);
        }, "");
        GamePlaying = true;
    }
    public override void Begin()
    {
        MainMenu();
    }
    protected override void Update(Time time)
    {
        if (GamePlaying)
        {
            AnimatePlayer();
            CheckRoomExit();
        }
        base.Update(time);
    }
}

