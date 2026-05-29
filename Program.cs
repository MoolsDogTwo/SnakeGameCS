using Raylib_cs;

namespace SnakeGame;

using Point = (int Y, int X);

public static class Game
{
    public const int Width = 640;
    public const int Height = 640;
    public const int FontSize = 128;
    public static int Score = 0;
    public static bool GameOver = false; 

    public static void DrawGameOverScreen()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Color.DarkBlue);
        Raylib.DrawText($"Game over! Score was {Score}", 0, 0, FontSize, Color.Red);
        Raylib.EndDrawing();
    }
}

public enum CellType
{
    Empty,
    Food,
    Snake,
}

public enum SnakeDirection
{
    Left,
    Right,
    Up,
    Down
}

public class Cell(Point pos, CellType type)
{
    public Point Pos = pos;
    public CellType Type = type;
}

public class Map
{
    public const int Size = 32;
    public const int GridDiv = Game.Width / Size;
    public Cell[,] map = new Cell[Size, Size];

    public Map()
    {
        for (int i = 0; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                map[i, j] = new((i, j), CellType.Empty);
            }
        }
    }

    public void SetCell(Point pos, CellType state)
    {
        map[pos.Y,pos.X].Pos = (pos.Y, pos.X);
        map[pos.Y,pos.X].Type = state;
    }

    // Simple wrapper for SetCell to clear
    public void ClearCell(Point pos)
    {
        SetCell(pos, CellType.Empty);
    }

    public void Draw()
    {
        Color col;

        for (int i = 0 ; i < Size; i++)
        {
            for (int j = 0; j < Size; j++)
            {
                switch (map[i,j].Type)
                {
                    case CellType.Empty:
                    {
                        col = Color.Black;
                        break;
                    }
                    case CellType.Food:
                    {
                        col = Color.Red;
                        break;
                    }
                    case CellType.Snake:
                    {
                        col = Color.Green;
                        break;
                    }
                    default:
                    {
                        col = Color.Black;
                        break;
                    }
                }
                Raylib.DrawLine(j * GridDiv, 0, j * GridDiv, Game.Height, Color.DarkGray);
                Raylib.DrawRectangle(j * GridDiv, i * GridDiv, GridDiv, GridDiv, col);
            }
            Raylib.DrawLine(0, i * GridDiv, Game.Width * GridDiv * i, 0, Color.DarkGray);
        }
    }
}

public class Snake
{
    private List<Cell> _segments = [];
    public Cell Head => _segments.First();
    public SnakeDirection Dir = SnakeDirection.Down;

    public Snake()
    {
        int middle = Map.Size / 2;
        _segments.Add(new Cell((middle, middle), CellType.Snake));
    }

    private void _moveBody()
    {
        for (int i = _segments.Count - 1; i > 0; --i)
        {
            _segments[i].Pos = _segments[i - 1].Pos;
        }
    }

    public void Move()
    {
        switch (Dir)
        {
            case SnakeDirection.Left:
            {
                --Head.Pos.X;
                break;
            }
            case SnakeDirection.Right:
            {
                ++Head.Pos.X;
                break;
            }
            case SnakeDirection.Up:
            {
                --Head.Pos.Y;
                break;
            }
            case SnakeDirection.Down:
            {
                ++Head.Pos.Y;
                break;
            }
            default:
                break;
        }
        _moveBody();
    }

    public void Grow(Map map)
    {
        _segments.Add(new Cell(Food.FoodPos, CellType.Snake));
        Food.GenerateFood(map);
    }

    public void Draw(Map map)
    {
        foreach (Cell segment in _segments)
        {
            map.SetCell((segment.Pos.Y, segment.Pos.X), CellType.Snake);
        }
    }

    public void Clear(Map map)
    {
        foreach (Cell segment in _segments)
        {
            map.ClearCell((segment.Pos.Y, segment.Pos.X));
        }
    }
}

public class Food
{
    public static Point FoodPos = (-1, -1);
    private static Random _rng = new();

    public static void GenerateFood(Map map)
    {
        FoodPos.X = _rng.Next(0, Map.Size - 1);
        FoodPos.Y = _rng.Next(0, Map.Size - 1);
        map.ClearCell(FoodPos);
    }
    
    public static void Draw(Map map)
    {
        map.SetCell(FoodPos, CellType.Food);
    }

    public static void CheckCollision(Snake snake, Map map)
    {
        if (snake.Head.Pos == FoodPos)
        {
            ++Game.Score;
            snake.Grow(map);
        }
    }
}

class Program
{
    static void Main()
    {
        Raylib.InitWindow(Game.Width, Game.Height, "Snek");
        Raylib.SetTargetFPS(12);

        Map map = new();
        Snake snake = new();
        Food.GenerateFood(map);

        while (!Raylib.WindowShouldClose())
        {
            if (Game.GameOver)
            {
                Game.DrawGameOverScreen();
                continue;
            }

            /* Capture Input */
            if (Raylib.IsKeyPressed(KeyboardKey.Left) && snake.Dir != SnakeDirection.Right)
                snake.Dir = SnakeDirection.Left;
            if (Raylib.IsKeyPressed(KeyboardKey.Right) && snake.Dir != SnakeDirection.Left)
                snake.Dir = SnakeDirection.Right;
            if (Raylib.IsKeyPressed(KeyboardKey.Up) && snake.Dir != SnakeDirection.Down)
                snake.Dir = SnakeDirection.Up;
            if (Raylib.IsKeyPressed(KeyboardKey.Down) && snake.Dir != SnakeDirection.Up)
                snake.Dir = SnakeDirection.Down;

            Food.CheckCollision(snake, map);

            snake.Clear(map);  // Clear previously drawn cells
            
            snake.Move();

            /* Draw */
            try
            {
                Raylib.BeginDrawing();

                Food.Draw(map);
                snake.Draw(map);
                map.Draw();
                Raylib.EndDrawing();
            }
            catch (IndexOutOfRangeException e)
            {
                Game.GameOver = true;
                Console.WriteLine(e);
            }
        }

        Raylib.CloseWindow();
    }
}