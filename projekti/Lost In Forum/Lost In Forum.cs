using Jypeli;
using System.Collections.Generic;



namespace Lost_In_Forum;
public class LostInForum : PhysicsGame
{
    Player _player;
    PhysicsObject[] _borders;
    bool _gamePlaying;
    Image _playerCurrentImg;
    Image _concreteImg;
    List<GameObject> _punches = new List<GameObject>();
    /// <summary>
    /// Asettaa kentan koon ja kameran/ikkunan asetuksia
    /// </summary>
    void SetupWindow()
    {
        SetWindowSize(1920, 1200);
        Level.Size = new Vector(1920, 1200);
        IsFullScreen = true;
        Camera.ZoomToLevel();
        Level.BackgroundColor = Color.DarkGray;
        Game.UpdatesPerSecod = 59;
        _borders = [
            new PhysicsObject(Level.Width, Level.Height/10,Shape.Rectangle, 0, Level.Bottom),
            new PhysicsObject(Level.Width, Level.Height/10,Shape.Rectangle, 0, Level.Top),
            new PhysicsObject(Level.Width / 16, Level.Height,Shape.Rectangle,Level.Left),
            new PhysicsObject(Level.Width / 16, Level.Height,Shape.Rectangle, Level.Right),
        ];
        foreach (PhysicsObject g in _borders)
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
        _player = new Player();
        _player.Tag = "Player";
        _player.Hp.LowerLimit += _player.Die;
        _player.IgnoresGravity = true;
        _player.CollisionIgnoreGroup = 1;
        _player.MaxVelocity = 0;
        _player.CanRotate = false;
        _player.IsVisible = false;
        Add(_player);
        _player.Sprite = new GameObject(Level.Width / 9.6, Level.Height / 6, Shape.Rectangle);
        _player.Sprite.Y = _player.Bottom + _player.Sprite.Height / 2;
        _player.Add(_player.Sprite);
    }
    /// <summary>
    /// Luo pelin ohjaukset.
    /// </summary>
    void CreateControls()
    {
        Keyboard.ListenArrows(ButtonState.Pressed, Punch, "");
        Keyboard.Listen(Key.W, ButtonState.Down, _player.Walk, "", new Vector(0, 1));
        Keyboard.Listen(Key.A, ButtonState.Down, _player.Walk, "", new Vector(-1, 0));
        Keyboard.Listen(Key.S, ButtonState.Down, _player.Walk, "", new Vector(0, -1));
        Keyboard.Listen(Key.D, ButtonState.Down, _player.Walk, "", new Vector(1, 0));
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Tab, ButtonState.Pressed, Ui.MiniMap.UpdateMap, "");
    }

    void Punch(Vector v)
    {
        if (_player.Punching) return;
        PhysicsObject p = new PhysicsObject(200, 200, Shape.Rectangle);
        p.Position = _player.Sprite.Position;
        p.Collided += PunchCollision;
        _player.Punchdir = v;
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
        else i = [Game.LoadImage("norsu")];
        _player.Punching = true;
        foreach (Image x in i)
            x.Scaling = ImageScaling.Nearest;
        p.Animation = new Animation(i);
        p.Animation.IsPlaying = true;
        p.Animation.FPS = 10;
        p.Animation.StopOnLastFrame = true;
        p.Animation.Played += delegate { p.Destroy(); _punches.Remove(p); };
        p.Tag = "Punch";
        p.MakeStatic();
        p.CollisionIgnoreGroup = 1;
        p.IgnoresCollisionResponse = true;
        p.IsVisible = false;
        Add(p, 4);
        _punches.Add(p);
        Timer.SingleShot(_player.PunchInterval, delegate { EndPunch(); });
    }
    bool IsOverlapping(GameObject a, GameObject b)
    {
        return a.Top > b.Bottom && a.Bottom < b.Top && a.Left < b.Right && a.Right > b.Left;
    }
    void PunchCollision(IPhysicsObject o, IPhysicsObject t)
    {
        if (t is Enemy e)
        {
            //RaySegment r = new RaySegment(player.Position, (e.Position - player.Position).Normalize(), Vector.Distance(player.Position,e.Position));
            GameObject g = new GameObject(Vector.Distance(_player.Position, e.Position), 1, Shape.Rectangle);
            g.Position = _player.Position + (e.Position - _player.Position) / 2;
            g.Angle = (e.Position - _player.Position).Normalize().Angle;
            bool hit = true;
            foreach (GameObject x in Rooms.CurrentRoom.Layout)
            {
                if (IsOverlapping(x, g))
                    hit = false;
            }
            if (!hit)
                return;
            e.TakeDamage(_player.Dmg);
            e.Knockback(_player.Punchdir);
        }
    }
    void EndPunch()
    {
        _player.Punching = false;
    }

    /// <summary>
    /// Luo animaatiot.
    /// </summary>
    void CreateAnimations()
    {
        Image idle = Game.LoadImage("KoditonIdle");
        idle.Scaling = ImageScaling.Nearest;
        _player.Idle = new Animation(idle);
        Image[] walkE = Game.LoadImages("KoditonWalk_R1", "KoditonWalk_R2", "KoditonWalk_R3", "KoditonWalk_R2");
        foreach (Image i in walkE)
            i.Scaling = ImageScaling.Nearest;
        _player.WalkE = new Animation(walkE);
        _player.WalkE.FPS = 4;
        Image[] walkW = Game.LoadImages("KoditonWalk_L1", "KoditonWalk_L2", "KoditonWalk_L3", "KoditonWalk_L2");
        foreach (Image i in walkW)
            i.Scaling = ImageScaling.Nearest;
        _player.WalkW = new Animation(walkW);
        _player.WalkW.FPS = 4;
        Image[] walkN = Game.LoadImages("KoditonWalk_U1", "KoditonWalk_U2", "KoditonWalk_U3", "KoditonWalk_U2");
        foreach (Image i in walkN)
            i.Scaling = ImageScaling.Nearest;
        _player.WalkN = new Animation(walkN);
        _player.WalkN.FPS = 4;
        Image[] walkS = Game.LoadImages("KoditonWalk_D1", "KoditonIdle", "KoditonWalk_D3", "KoditonIdle");
        foreach (Image i in walkS)
            i.Scaling = ImageScaling.Nearest;
        _player.WalkS = new Animation(walkS);
        _player.WalkS.FPS = 4;
        Image[] punchR = Game.LoadImages("KoditonPunch_Rbody");
        foreach (Image i in punchR)
            i.Scaling = ImageScaling.Nearest;
        _player.PunchR = new Animation(punchR);
        _player.PunchR.FPS = 4;
        Image[] punchL = Game.LoadImages("KoditonPunch_Lbody");
        foreach (Image i in punchL)
            i.Scaling = ImageScaling.Nearest;
        _player.PunchL = new Animation(punchL);
        _player.PunchL.FPS = 4;
        Image[] punchU = Game.LoadImages("KoditonPunch_Ubody");
        foreach (Image i in punchU)
            i.Scaling = ImageScaling.Nearest;
        _player.PunchU = new Animation(punchU);
        _player.PunchU.FPS = 4;
        Image[] punchD = Game.LoadImages("KoditonPunch_Dbody");
        foreach (Image i in punchD)
            i.Scaling = ImageScaling.Nearest;
        _player.PunchD = new Animation(punchD);
        _player.PunchD.FPS = 4;
    }
    /// <summary>
    /// Asettaa pelaajan hahmolle tilanteeseen sopivan animaation.
    /// </summary>
    void AnimatePlayer()
    {
        if (_player.Punching)
        {
            if (_player.Punchdir.X > 0)
                _player.Sprite.Animation = _player.PunchR;
            else if (_player.Punchdir.X < 0)
                _player.Sprite.Animation = _player.PunchL;
            else if (_player.Punchdir.Y > 0)
                _player.Sprite.Animation = _player.PunchU;
            else if (_player.Punchdir.Y < 0)
                _player.Sprite.Animation = _player.PunchD;
            return;
        }
        if (Keyboard.IsKeyDown(Key.W))
        {
            _player.Sprite.Animation = _player.WalkN;
            _player.Sprite.Animation.IsPlaying = true;
        }
        else if (Keyboard.IsKeyDown(Key.S))
        {
            _player.Sprite.Animation = _player.WalkS;
            _player.Sprite.Animation.IsPlaying = true;
        }
        else if (Keyboard.IsKeyDown(Key.A))
        {
            _player.Sprite.Animation = _player.WalkW;
            _player.Sprite.Animation.IsPlaying = true;
        }
        else if (Keyboard.IsKeyDown(Key.D))
        {
            _player.Sprite.Animation = _player.WalkE;
            _player.Sprite.Animation.IsPlaying = true;
        }
        else
            _player.Sprite.Animation = _player.Idle;
    }
    /// <summary>
    /// Tarkistaa, ett� onko pelaaja poistumassa huoneesta ja siirt�� pelaajan oikeaan huoneeseen.
    /// </summary>
    void CheckRoomExit()
    {
        if (_player.Position.X > Level.Right)
        {
            Rooms.CurrentPos[0]++;
            Rooms.LoadRoom(Game.Instance, Rooms.Map[Rooms.CurrentPos[1], Rooms.CurrentPos[0]]);
            _player.X = Level.Left + 100;
            _player.ImmunityFrames(1.5);
        }
        if (_player.Position.X < Level.Left)
        {
            Rooms.CurrentPos[0]--;
            Rooms.LoadRoom(Game.Instance, Rooms.Map[Rooms.CurrentPos[1], Rooms.CurrentPos[0]]);
            _player.X = Level.Right - 100;
            _player.ImmunityFrames(1.5);
        }
        if (_player.Position.Y > Level.Top)
        {
            Rooms.CurrentPos[1]--;
            Rooms.LoadRoom(Game.Instance, Rooms.Map[Rooms.CurrentPos[1], Rooms.CurrentPos[0]]);
            _player.Y = Level.Bottom + 100;
            _player.ImmunityFrames(1.5);
        }
        if (_player.Position.Y < Level.Bottom)
        {
            Rooms.CurrentPos[1]++;
            Rooms.LoadRoom(Game.Instance, Rooms.Map[Rooms.CurrentPos[1], Rooms.CurrentPos[0]]);
            _player.Y = Level.Top - 100;
            _player.ImmunityFrames(1.5);
        }
    }
    public void MainMenu()
    {
        ClearAll();
        IsFullScreen = false;
        _gamePlaying = false;
        SetWindowSize(900, 900);
        Level.Size = new Vector(900, 900);
        Level.BackgroundColor = Color.DarkGray;
        GameObject title = new GameObject(599, 392, Shape.Rectangle);
        title.Image = Game.LoadImage("FORUMTITLE");
        title.Y = Level.Top - title.Height / 2;
        title.X = 100;
        GameObject menuSignPole = new GameObject(11 * 2.5, 200 * 2.5, Shape.Rectangle);
        menuSignPole.Y = Level.Bottom + menuSignPole.Height / 2 + 200;
        menuSignPole.X = Level.Left + 150;
        GameObject playButton = new GameObject(85 * 3, 85 * 3, Shape.Rectangle);
        GameObject optionsButton = new GameObject(85 * 2, 22 * 2);
        playButton.Y = menuSignPole.Top;
        playButton.X = menuSignPole.X;
        optionsButton.Y = playButton.Bottom - optionsButton.Height;
        optionsButton.X = menuSignPole.X;
        playButton.Image = Game.LoadImage("PlaySign");
        optionsButton.Image = Game.LoadImage("OptionsSign");
        playButton.Image.Scaling = ImageScaling.Nearest;
        optionsButton.Image.Scaling = ImageScaling.Nearest;
        menuSignPole.Image = Game.LoadImage("MenuSignPole");
        GameObject bgImage = new GameObject(Level.Width, Level.Height, Shape.Rectangle);
        bgImage.Image = Game.LoadImage("MenuBG");
        bgImage.Image.Scaling = ImageScaling.Nearest;
        Mouse.ListenOn(playButton, MouseButton.Left, ButtonState.Pressed, StartGame, "");
        Add(playButton, 1);
        Add(optionsButton, 1);
        Add(menuSignPole);
        Add(title, 2);
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
        Timer.SingleShot(3, delegate
        {
            ClearAll();
            SetupWindow();
            Rooms.GetAllRooms();
            Rooms.CreateMap();
            Rooms.SetRooms();
            Rooms.CurrentPos = [ Rooms.StartRoom, Rooms.StartRoom ];
            Rooms.LoadRoom(Game.Instance, Rooms.Map[Rooms.StartRoom, Rooms.StartRoom]);
            loadimg.Destroy();
            loadbg.Destroy();
            CreatePlayer();
            CreateAnimations();
            CreateControls();
            Ui.HealthBar.CreateHealthBar(Game.Instance, _player, Level);
            _gamePlaying = true;
        });

    }


    public override void Begin()
    {
        MainMenu();
        _concreteImg = Game.LoadImage("Concrete");
    }
    protected override void Paint(Canvas canvas)
    {
        if (Rooms.CurrentRoom == null || _player == null || !_gamePlaying)
            return;
        Image img = _concreteImg;
        foreach (var obj in Rooms.CurrentRoom.Layout)
        {
            img.Scaling = ImageScaling.Nearest;
            img.Rescale((int)obj.Size.X + 1, (int)obj.Size.Y + 1);
            Vector pos = new Vector(obj.X, obj.Y);
            canvas.DrawImage(pos, img);
        }
        if (_playerCurrentImg != null && _player.Sprite.IsVisible)
            canvas.DrawImage(_player.Sprite.Position, _playerCurrentImg);
        foreach (GameObject g in _punches)
        {
            img = g.Animation.CurrentFrame;
            img.Rescale((int)g.Width + 1, (int)g.Height + 1);
            canvas.DrawImage(g.Position, img);
        }
        foreach (Enemy e in Rooms.CurrentRoom.Enemies)
        {
            canvas.DrawImage(e.HealthBarBg.Position, e.HealthBarBg.Image);
            canvas.DrawImage(e.HealthBar.Position, e.HealthBar.Image);
        }
        foreach (var h in Ui.HealthBar.Hearts)
        {
            img = h.Image;
            img.Rescale((int)Ui.HealthBar.HeartSize + 1, (int)Ui.HealthBar.HeartSize + 1);
            canvas.DrawImage(h.Position, img);
        }
        if(Keyboard.IsKeyDown(Key.Tab))
        {
            if (Ui.MiniMap.MapBg != null)
            {
                var b = Ui.MiniMap.MapBg;
                canvas.DrawImage(b.Position, b.Image);
            }
            foreach (var b in Ui.MiniMap.Map)
            {
                canvas.DrawImage(b.Position, b.Image);
            }
        }
    }
    protected override void Update(Time time)
    {
        if (_gamePlaying)
        {
            AnimatePlayer();
            CheckRoomExit();
            if (Rooms.CurrentRoom.Enemies.Count == 0)
                Rooms.Map[Rooms.CurrentPos[1], Rooms.CurrentPos[0]].Cleared = true;
            if (Rooms.Map[Rooms.CurrentPos[1], Rooms.CurrentPos[0]].Cleared && !_borders[1].IgnoresCollisionResponse)
            {
                foreach (PhysicsObject g in _borders)
                    g.IgnoresCollisionResponse = true;
            }
            else if (!Rooms.Map[Rooms.CurrentPos[1], Rooms.CurrentPos[0]].Cleared && _borders[1].IgnoresCollisionResponse)
            {
                foreach (PhysicsObject g in _borders)
                    g.IgnoresCollisionResponse = false;
            }
            if (_player != null && _player.Sprite.Animation.CurrentFrame != null)
            {
                _playerCurrentImg = _player.Sprite.Animation.CurrentFrame;
                _playerCurrentImg.Rescale((int)_player.Sprite.Width + 1, (int)_player.Sprite.Height + 1);
            }
        }
        base.Update(time);
    }
}

