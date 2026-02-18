using Jypeli;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Lost_In_Forum;
class Rooms
{
    public static int DangerLevel = 0;
    public static int MapSize = 13;
    public static int StartRoom = 7;
    public static int[] CurrentPos { get; set; } = new int[2] { StartRoom, StartRoom };
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
                    if (x > 0 && map[i, x - 1] != null)
                        map[i, x].Exits[3] = 1;
                    if (x < MapSize - 1 && map[i, x + 1] != null)
                        map[i, x].Exits[1] = 1;
                    List<RoomData> ViableRooms = new List<RoomData>();
                    foreach (RoomData room in AllRooms)
                    {
                        if (room.Exits.SequenceEqual(map[i, x].Exits))
                            ViableRooms.Add(room);
                    }
                    if (ViableRooms.Count > 0)
                    {
                        RoomData r = ViableRooms[RandomGen.NextInt(ViableRooms.Count)];
                        map[i, x] = new RoomData
                        {
                            Enemies = new List<ObjectData>(r.Enemies),
                            Layout = new List<ObjectData>(r.Layout),
                            Exits = new List<int>(r.Exits).ToArray()
                        };
                    }

                }
            }
        }
    }
    public static void CreateMap()
    {
        map = new RoomData[MapSize, MapSize];
        for (int x = -1; x < 2; x += 2)
        {
            map[StartRoom + x, StartRoom] = new RoomData();
            map[StartRoom, StartRoom + x] = new RoomData();
        }
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
            int[] pos = validRooms[RandomGen.NextInt(validRooms.Count)];
            selection = new int[] { pos[0], pos[1] };
            return pos;
        }
        void MakeWorm(int[] startpos, int min, int max)
        {
            selection[0] = startpos[0];
            selection[1] = startpos[1];
            int[] dir = RandomGen.SelectOne<int[]>(new int[] { 0, 1 }, new int[] { 0, -1 }, new int[] { 1, 0 }, new int[] { -1, 0 });
            for (int i = 0; i < RandomGen.NextInt(min, max + 1); i++)
            {
                selection[0] += dir[0];
                selection[1] += dir[1];
                if (RandomGen.NextInt(2) == 0)
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
            Rooms.DangerLevel = Math.Abs(Rooms.StartRoom - CurrentPos[0]) + Math.Abs(Rooms.StartRoom - CurrentPos[1]);
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
                PhysicsObject block = new PhysicsObject(game.Level.Width / 32, game.Level.Height / 20, obj.X, obj.Y);
                block.MakeStatic();
                block.Color = Color.AshGray;
                game.Add(block);
                CurrentRoom.Layout.Add(block);
            }
            List<Vector> enemySpawns = new List<Vector>();
            foreach (ObjectData obj in room.Enemies)
            {
                enemySpawns.Add(new Vector(obj.X, obj.Y));
            }
            int dangerPoints = DangerLevel;
            while (dangerPoints > 0 && enemySpawns.Count > 0 && !room.Cleared)
            {
                Enemy e = new Enemy(1, 1);
                for (bool b = false; b == false;)
                {
                    e = GetEnemyType(RandomGen.NextInt(1, 4));
                    if (e.Danger <= dangerPoints)
                        b = true;
                }
                int rand = RandomGen.NextInt(enemySpawns.Count);
                e.Position = enemySpawns[rand];
                enemySpawns.Remove(enemySpawns[rand]);
                dangerPoints -= e.Danger;
                game.Add(e);
                CurrentRoom.Enemies.Add(e);
            }
            CurrentRoom.Exits = room.Exits;
        }
        catch (Exception e)
        {
            System.Diagnostics.Debug.WriteLine("Error loading room: " + e.Message);
        }
    }
    public static Enemy GetEnemyType(int i)
    {
        switch (i)
        {
            case 1:
                return new Lapsi();
            case 2:
                return new Rakki();
            case 3:
                return new TestiVihu3();
            default:
                return new Lapsi();
        }
    }
}