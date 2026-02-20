using Jypeli;
using System.Collections.Generic;

/// @author gr313108
/// @version 21.11.2025
/// <summary>
/// 
/// </summary>

/// <summary>
/// Sis‰lt‰‰ arvoja pelaajan ohjaamalle hahmolle
/// </summary>


namespace Lost_In_Forum;
public class Lost_In_Forum : PhysicsGame
{
    Player player;
    PhysicsObject[] borders;
    bool GamePlaying = false;
    Image playerCurrentImg;
    Image concreteImg;
    List<GameObject> punches = new List<GameObject>();
    /// <summary>
    /// Asettaa kent‰n koon ja kameran/ikkunan asetuksia
    /// </summary>
    void SetupWindow()
    {
        SetWindowSize(1920, 1200);
        Level.Size = new Jypeli.Vector(1920, 1200);
        IsFullScreen = true;
        Camera.ZoomToLevel();
        Level.BackgroundColor = Color.DarkGray;
        Game.UpdatesPerSecod = 59;
        borders = new PhysicsObject[] {
            new PhysicsObject(Level.Width, Level.Height/10,Shape.Rectangle, 0, Level.Bottom),
            new PhysicsObject(Level.Width, Level.Height/10,Shape.Rectangle, 0, Level.Top),
            new PhysicsObject(Level.Width / 16, Level.Height,Shape.Rectangle,Level.Left, 0),
            new PhysicsObject(Level.Width / 16, Level.Height,Shape.Rectangle, Level.Right, 0),
        };
        foreach (PhysicsObject g in borders)
        {
            g.MakeStatic();
            Add(g);
            g.Color = Color.Transparent;
        }
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
        Keyboard.Listen(Key.Tab, ButtonState.Pressed, UI.MiniMap.UpdateMap, "");
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
        else if (v.Y > 0)
        {
            i = Game.LoadImages("KoditonPunch_UHand1", "KoditonPunch_UHand2");
            p.Y += p.Height / 3;
        }
        else if (v.Y < 0)
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
        p.Animation.Played += delegate { p.Destroy(); punches.Remove(p); };
        p.Tag = "Punch";
        p.MakeStatic();
        p.CollisionIgnoreGroup = 1;
        p.IgnoresCollisionResponse = true;
        p.IsVisible = false;
        Add(p, 4);
        punches.Add(p);
        Timer.SingleShot(player.punchInterval, delegate { EndPunch(); });
    }
    bool isOverlapping(GameObject a, GameObject b)
    {
        return a.Top > b.Bottom && a.Bottom < b.Top && a.Left < b.Right && a.Right > b.Left;
    }
    void PunchCollision(IPhysicsObject o, IPhysicsObject t)
    {
        if (t is Enemy e)
        {
            //RaySegment r = new RaySegment(player.Position, (e.Position - player.Position).Normalize(), Vector.Distance(player.Position,e.Position));
            GameObject g = new GameObject(Vector.Distance(player.Position, e.Position), 1, Shape.Rectangle);
            g.Position = player.Position + (e.Position - player.Position) / 2;
            g.Angle = (e.Position - player.Position).Normalize().Angle;
            bool hit = true;
            foreach (GameObject x in Rooms.CurrentRoom.Layout)
            {
                if (isOverlapping(x, g))
                    hit = false;
            }
            if (!hit)
                return;
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
            Rooms.CurrentPos[0]++;
            Rooms.LoadRoom(Game.Instance, Rooms.map[Rooms.CurrentPos[1], Rooms.CurrentPos[0]]);
            player.X = Level.Left + 100;
            player.IFrames(1.5);
        }
        if (player.Position.X < Level.Left)
        {
            Rooms.CurrentPos[0]--;
            Rooms.LoadRoom(Game.Instance, Rooms.map[Rooms.CurrentPos[1], Rooms.CurrentPos[0]]);
            player.X = Level.Right - 100;
            player.IFrames(1.5);
        }
        if (player.Position.Y > Level.Top)
        {
            Rooms.CurrentPos[1]--;
            Rooms.LoadRoom(Game.Instance, Rooms.map[Rooms.CurrentPos[1], Rooms.CurrentPos[0]]);
            player.Y = Level.Bottom + 100;
            player.IFrames(1.5);
        }
        if (player.Position.Y < Level.Bottom)
        {
            Rooms.CurrentPos[1]++;
            Rooms.LoadRoom(Game.Instance, Rooms.map[Rooms.CurrentPos[1], Rooms.CurrentPos[0]]);
            player.Y = Level.Top - 100;
            player.IFrames(1.5);
        }
    }
    public void MainMenu()
    {
        ClearAll();
        IsFullScreen = false;
        GamePlaying = false;
        SetWindowSize(900, 900);
        Level.Size = new Vector(900, 900);
        Level.BackgroundColor = Color.DarkGray;
        GameObject Title = new GameObject(599, 392, Shape.Rectangle);
        Title.Image = Game.LoadImage("FORUMTITLE");
        Title.Y = Level.Top - Title.Height / 2;
        Title.X = 100;
        GameObject MenuSignPole = new GameObject(11 * 2.5, 200 * 2.5, Shape.Rectangle);
        MenuSignPole.Y = Level.Bottom + MenuSignPole.Height / 2 + 200;
        MenuSignPole.X = Level.Left + 150;
        GameObject playButton = new GameObject(85 * 3, 85 * 3, Shape.Rectangle);
        GameObject optionsButton = new GameObject(85 * 2, 22 * 2);
        playButton.Y = MenuSignPole.Top;
        playButton.X = MenuSignPole.X;
        optionsButton.Y = playButton.Bottom - optionsButton.Height;
        optionsButton.X = MenuSignPole.X;
        playButton.Image = Game.LoadImage("PlaySign");
        optionsButton.Image = Game.LoadImage("OptionsSign");
        playButton.Image.Scaling = ImageScaling.Nearest;
        optionsButton.Image.Scaling = ImageScaling.Nearest;
        MenuSignPole.Image = Game.LoadImage("MenuSignPole");
        GameObject bgImage = new GameObject(Level.Width, Level.Height, Shape.Rectangle);
        bgImage.Image = Game.LoadImage("MenuBG");
        bgImage.Image.Scaling = ImageScaling.Nearest;
        Mouse.ListenOn(playButton, MouseButton.Left, ButtonState.Pressed, StartGame, "");
        Add(playButton, 1);
        Add(optionsButton, 1);
        Add(MenuSignPole);
        Add(Title, 2);
        Add(bgImage, -1);
    }
    void StartGame()
    {
        GameObject loadbg = new GameObject(Level.Width, Level.Height, Shape.Rectangle);
        loadbg.Color = Color.Black;
        GameObject loadimg = new GameObject(500, 500);
        Image[] loadImgs = Game.LoadImages("Loading1", "Loading2", "Loading3");
        foreach (Image i in loadImgs)
            i.Scaling = ImageScaling.Nearest;
        loadimg.Animation = new Animation(loadImgs);
        loadimg.Animation.FPS = 1;
        loadimg.Animation.IsPlaying = true;
        Add(loadimg, 4);
        Add(loadbg, 4);
        // tykk‰‰n milt‰ n‰ytt‰‰ ku siin‰ on sekunnin joku latauskuva nii tein sitten n‰in
        Timer.SingleShot(3, delegate
        {
            ClearAll();
            SetupWindow();
            Rooms.GetAllRooms();
            Rooms.CreateMap();
            Rooms.SetRooms();
            Rooms.CurrentPos = new int[] { Rooms.StartRoom, Rooms.StartRoom };
            Rooms.LoadRoom(Game.Instance, Rooms.map[Rooms.StartRoom, Rooms.StartRoom]);
            loadimg.Destroy();
            loadbg.Destroy();
            CreatePlayer();
            CreateAnimations();
            CreateControls();
            UI.HealthBar.CreateHealthBar(Game.Instance, player, Level);
            GamePlaying = true;
        });

    }


    public override void Begin()
    {
        MainMenu();
        concreteImg = Game.LoadImage("Concrete");
    }
    protected override void Paint(Canvas canvas)
    {
        if (Rooms.CurrentRoom == null || player == null || !GamePlaying)
            return;
        Image img = concreteImg;
        foreach (var obj in Rooms.CurrentRoom.Layout)
        {
            img.Scaling = ImageScaling.Nearest;
            img.Rescale((int)obj.Size.X + 1, (int)obj.Size.Y + 1);
            Vector pos = new Vector(obj.X, obj.Y);
            canvas.DrawImage(pos, img);
        }
        if (playerCurrentImg != null && player.Sprite.IsVisible)
            canvas.DrawImage(player.Sprite.Position, playerCurrentImg);
        foreach (GameObject g in punches)
        {
            img = g.Animation.CurrentFrame;
            img.Rescale((int)g.Width + 1, (int)g.Height + 1);
            canvas.DrawImage(g.Position, img);
        }
        foreach (Enemy e in Rooms.CurrentRoom.Enemies)
        {
            canvas.DrawImage(e.healthBarBg.Position, e.healthBarBg.Image);
            canvas.DrawImage(e.healthBar.Position, e.healthBar.Image);
        }
        foreach (var h in UI.HealthBar.Hearts)
        {
            img = h.Image;
            img.Rescale((int)UI.HealthBar.HeartSize + 1, (int)UI.HealthBar.HeartSize + 1);
            canvas.DrawImage(h.Position, img);
        }
        if(Keyboard.IsKeyDown(Key.Tab))
        {
            if (UI.MiniMap.mapBg != null)
            {
                var b = UI.MiniMap.mapBg;
                canvas.DrawImage(b.Position, b.Image);
            }
            foreach (var b in UI.MiniMap.map)
            {
                canvas.DrawImage(b.Position, b.Image);
            }
        }
    }
    protected override void Update(Time time)
    {
        if (GamePlaying)
        {
            AnimatePlayer();
            CheckRoomExit();
            if (Rooms.CurrentRoom.Enemies.Count == 0)
                Rooms.map[Rooms.CurrentPos[1], Rooms.CurrentPos[0]].Cleared = true;
            if (Rooms.map[Rooms.CurrentPos[1], Rooms.CurrentPos[0]].Cleared && !borders[1].IgnoresCollisionResponse)
            {
                foreach (PhysicsObject g in borders)
                    g.IgnoresCollisionResponse = true;
            }
            else if (!Rooms.map[Rooms.CurrentPos[1], Rooms.CurrentPos[0]].Cleared && borders[1].IgnoresCollisionResponse)
            {
                foreach (PhysicsObject g in borders)
                    g.IgnoresCollisionResponse = false;
            }
            if (player != null && player.Sprite.Animation.CurrentFrame != null)
            {
                playerCurrentImg = player.Sprite.Animation.CurrentFrame;
                playerCurrentImg.Rescale((int)player.Sprite.Width + 1, (int)player.Sprite.Height + 1);
            }
        }
        base.Update(time);
    }
}

