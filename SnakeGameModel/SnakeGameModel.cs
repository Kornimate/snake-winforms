using Game.SnakeGame.Persistence;
using System.Diagnostics;
using System.Drawing;
using System.Threading;

namespace Game.SnakeGame.Model
{
    public class SnakeGameModel
    {
        private const int SPEED = 400;
        public int TableSize { get; set; }
        private Field[,] Table = null!;
        public Direction Direction { get; set; }
        public int Score { get; set; }
        public bool State { get; set; }

        private Point? Egg = new Point();
        private List<Point>? Snake = new List<Point>();

        private int Speed;
        private bool ChangedDirection;

        private SnakeGamePersistence persistence;

        public event EventHandler<GameEventArgs>? DataToDraw;
        public event EventHandler<GameEventArgs>? OnGameOver;
        public event EventHandler<GameEventArgs>? ScoreWriter;
        public event EventHandler<GameEventArgs>? SpeedChange;
        public SnakeGameModel()
        {
            persistence = new SnakeGamePersistence();
        }
        public void StartNewGame(int size)
        {
            ChangedDirection = false;
            TableSize=size;
            Table = new Field[TableSize, TableSize];
            Score = 0;
            State = false;
            Direction = Direction.Up;

            GenerateTable();
            Speed = SPEED;
            SpeedChange?.Invoke(this, new GameEventArgs(Speed));
        }

        private void GenerateTable()
        {
            NewTable();
            NewSnake();
            NewEgg();
            NewWalls();
        }

        private void NewTable()
        {
            for (int i = 0; i < TableSize; i++)
            {
                for (int j = 0; j < TableSize; j++)
                {
                    SetField(i, j, Field.Empty);
                }
            }
        }

        public void SetField(int x, int y, Field fieldType)
        {
            Table[x, y] = fieldType;
            switch (fieldType)
            {
                case Field.Wall:
                    DataToDraw?.Invoke(this,new GameEventArgs(x, y, Color.Black));
                    break;
                case Field.Body:
                    DataToDraw?.Invoke(this,new GameEventArgs(x, y, Color.LightGreen));
                    break;
                case Field.Head:
                    DataToDraw?.Invoke(this,new GameEventArgs(x, y, Color.DarkGreen));
                    break;
                case Field.Egg:
                    DataToDraw?.Invoke(this,new GameEventArgs(x, y, Color.Red));
                    break;
                default:
                    DataToDraw?.Invoke(this, new GameEventArgs(x, y, Color.LightGray));
                    break;
            }
        }

        public void Move()
        {
            ChangedDirection = false;
            if (!State) return;
            MoveBody();
            switch (Direction)
            {
                case Direction.Up:
                    Snake![0].Y--;
                    break;
                case Direction.Down:
                    Snake![0].Y++;
                    break;
                case Direction.Left:
                    Snake![0].X--;
                    break;
                case Direction.Right:
                    Snake![0].X++;
                    break;
            }

            if (SnakeHitsSnake() || Snake![0].X < 0
                || Snake[0].X >= TableSize ||
                 Snake[0].Y < 0 || Snake[0].Y >= TableSize)
            {
                Die("You lost! Your snake collided!");
            }
            else if (Table[Snake[0].X, Snake[0].Y] == Field.Wall)
            {
                Die("You lost! Your snake collided with a wall!");
            }
            else
            {
                SetField(Snake[0].X, Snake[0].Y, Field.Head);
                if (Snake[0].X == Egg!.X && Snake[0].Y == Egg.Y)
                {
                    Eat();
                }
            }
        }

        public void MoveBody()
        {
            for (int i = Snake!.Count - 1; i > 0; i--)
            {
                if (i == Snake.Count - 1)
                {
                    SetField(Snake[i].X, Snake[i].Y, Field.Empty);
                }
                if (Snake[i - 1].X < 0
                || Snake[i - 1].X > TableSize ||
                 Snake[i - 1].Y < 0 || Snake[i - 1].Y > TableSize)
                {
                    Die("You lost! Your snake collided!");
                }
                Snake[i].X = Snake[i - 1].X;
                Snake[i].Y = Snake[i - 1].Y;
                SetField(Snake[i].X, Snake[i].Y, Field.Body);
            }
        }

        private bool SnakeHitsSnake()
        {
            int i = 1;
            bool eatsItself = false;
            while (i < Snake!.Count && !eatsItself)
            {
                if (Snake[i].X == Snake[0].X && Snake[i].Y == Snake[0].Y)
                {
                    eatsItself = true;
                }
                ++i;
            }
            return eatsItself;
        }
        private bool CanBePlaced(int x, int y)
        {
            if (x < 0 || y < 0) { return false; }
            if (x >= TableSize || y >= TableSize) { return false; }
            return Table[x, y] == Field.Empty;
        }


        private void NewEgg()
        {
            Random rnd = new Random();
            int rndXTest = rnd.Next(0, TableSize);
            int rndYTest = rnd.Next(0, TableSize);
            while (!CanBePlaced(rndXTest, rndYTest))
            {
                rndXTest = rnd.Next(0, TableSize);
                rndYTest = rnd.Next(0, TableSize);
            }
            Egg = new Point { X = rndXTest, Y = rndYTest };
            SetField(Egg.X, Egg.Y, Field.Egg);
        }

        private void NewSnake()
        {
            Snake!.Clear();
            Point head = new Point { X = TableSize / 2, Y = TableSize / 2 };
            Snake.Add(head);
            SetField(head.X, head.Y, Field.Head);
            for (int i = 0; i < 4; i++)
            {
                Point body = new Point { X = Snake[i].X, Y = Snake[i].Y + 1 };
                Snake.Add(body);
                SetField(body.X, body.Y, Field.Body);
            }
        }
        private void NewWalls()
        {
            int wallcount = GetWallNumber();
            int c = 0;
            while (c < wallcount)
            {
                Random rnd = new Random();
                int rndXTest = rnd.Next(0, TableSize);
                int rndYTest = rnd.Next(0, TableSize);
                while (!CanBePlaced(rndXTest, rndYTest) || rndXTest==TableSize/2)
                {
                    rndXTest = rnd.Next(0, TableSize);
                    rndYTest = rnd.Next(0, TableSize);
                }
                SetField(rndXTest, rndYTest, Field.Wall);
                c++;
            }
        }
        private void Eat()
        {
            Point body = new Point { X = Snake![Snake.Count - 1].X, Y = Snake[Snake.Count - 1].Y };
            Snake.Add(body);
            SetField(body.X, body.Y, Field.Body);
            ++Score;
            NewEgg();
            ScoreWriter?.Invoke(this, new GameEventArgs(Score));
            if (Score % 10 == 0)
            {
                if (Speed > 50) Speed -= 50;
                SpeedChange?.Invoke(this, new GameEventArgs(Speed));
            }
        }
        private void Die(string text)
        {
            OnGameOver?.Invoke(this,new GameEventArgs(Score,text));
        }
        public void ToggleGameState(bool state)
        {
            State = state;
        }
        public void SetDirection(Direction d)
        {
            if (ChangedDirection) return;
            if (!State) return;
            if (Direction == Direction.Up && d == Direction.Down) return;
            if (Direction == Direction.Down && d == Direction.Up) return;
            if (Direction == Direction.Left && d == Direction.Right) return;
            if (Direction == Direction.Right && d == Direction.Left) return;
            Direction = d;
            ChangedDirection = true;
        }
        private int GetWallNumber()
        {
            return (TableSize switch
            {
                10 => 6,
                15 => 12,
                20 => 18,
                _ => 6
            });
        }
    }
}