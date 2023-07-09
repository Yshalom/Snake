using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Neural_Network___2
{
    public class GameBoard
    {
        private const int maxDeep = 5;

        public enum Mode
        {
            Empty = 0,
            Border,
            Snake,
            SnakeHead,
            Food,

            Up,
            Right,
            Down,
            Left,
        }

        private Mode[][] board;
        private LinkedList<int[]> snake;
        private int amountOfMovements = 0;
        private static Random random = new Random();
        private int[] foodIndex;

        public GameBoard(int size)
        {
            if (300 / size != 300.0 / size)
                throw new Exception("300 has to be able to be divided by the size of the board.");

            board = new Mode[size][];
            for (int i = 0; i < size; i++)
                board[i] = new Mode[size];

            CreateBoard();
        }

        public object Clone()
        {
            GameBoard result = new GameBoard(board.Length);

            for (int i = 0; i < board.Length; i++)
                for (int j = 0; j < board.Length; j++)
                    result.board[i][j] = board[i][j];

            if (snake == null)
                return result;
            result.snake = new LinkedList<int[]>();
            LinkedListNode<int[]>? index = snake.First;
            while (index != null)
            {
                result.snake.AddLast((int[])index.Value.Clone());
                index = index.Next;
            }
            result.amountOfMovements = amountOfMovements;
            result.foodIndex = (int[])foodIndex.Clone();

            return result;
        }

        public Bitmap GetBoardToImage()
        {
            Bitmap bmp = new Bitmap(301, 301);
            Graphics g = Graphics.FromImage(bmp);
            Pen pen = new Pen(Color.White);
            g.FillRectangle(pen.Brush, 0, 0, 301, 301);

            // Paint grid
            int jumps = 300 / board.Length;
            for (int i = 0; i <= 300; i++)
                for (int j = 0; j <= 300; j++)
                    if (i % jumps == 0 || j % jumps == 0)
                        bmp.SetPixel(i, j, Color.Black);
            
            // Paint borders
            Pen BorederPen = new Pen(Color.Red);
            for (int i = 0; i < board.Length; i++)
                for (int j = 0; j < board.Length; j++)
                {
                    if (board[i][j] == Mode.Border)
                        g.FillRectangle(BorederPen.Brush, i * jumps + 1, j * jumps + 1, jumps - 1, jumps - 1);
                }

            // Paint Food
            Pen FoodPen = new Pen(Color.FromArgb(200, 200, 0));
            g.FillRectangle(FoodPen.Brush, foodIndex[0] * jumps + 1, foodIndex[1] * jumps + 1, jumps - 1, jumps - 1);

            // Paint Snake
            Pen SnakeHeadPen = new Pen(Color.FromArgb(0, 0, 255));
            g.FillRectangle(SnakeHeadPen.Brush, snake.First.Value[0] * jumps + 1, snake.First.Value[1] * jumps + 1, jumps - 1, jumps - 1);
            Pen SnakePen = new Pen(Color.FromArgb(0, 0, 0));
            int Index = 0;
            for (LinkedListNode<int[]> i = snake.First.Next; i != null; i = i.Next)
            {
                Index++;
                SnakePen.Color = Color.FromArgb(
                    Index / 64 % 2 == 0 ? 127 - Index * 2 % 128 : Index * 2 % 128,
                    Index / 64 % 2 == 0 ? Index * 4 % 256 : 255 - Index * 4 % 256,
                    50 + (Index / 43 % 2 == 0 ? 127 - Index * 3 % 128 : Index * 3 % 128)
                    );

                g.FillRectangle(SnakePen.Brush, i.Value[0] * jumps + 1, i.Value[1] * jumps + 1, jumps - 1, jumps - 1);
            }

            return bmp;
        }

        public GameBoard CreateBoard()
        {
            for (int i = 0; i < board.Length; i++)
                for (int j = 0; j < board.Length; j++)
                    board[i][j] = Mode.Empty;

            // Fill border
            int amount = random.Next(board.Length / 6, board.Length / 2);
            for (int i = 0; i < amount; i++)
            {
                int sizeX = random.Next(1, (int)Math.Sqrt(board.Length)),
                    sizeY = random.Next(1, (int)Math.Sqrt(board.Length)),
                    X = random.Next(0, board.Length - sizeX),
                    Y = random.Next(0, board.Length - sizeY);

                for (int j = X; j - X < sizeX; j++)
                    for (int k = Y; k - Y < sizeY; k++)
                        board[j][k] = Mode.Border;
            }

            // Fill Snake's Head
            int IndexX = random.Next(0, board.Length), IndexY = random.Next(0, board.Length);
            while (board[IndexX][IndexY] != Mode.Empty)
            {
                IndexX = random.Next(0, board.Length);
                IndexY = random.Next(0, board.Length);
            }
            board[IndexX][IndexY] = Mode.SnakeHead;
            snake = new LinkedList<int[]>();
            snake.AddFirst(new int[2] { IndexX, IndexY });

            // Fill Food
            IndexX = random.Next(0, board.Length);
            IndexY = random.Next(0, board.Length);
            while (board[IndexX][IndexY] != Mode.Empty)
            {
                IndexX = random.Next(0, board.Length);
                IndexY = random.Next(0, board.Length);
            }
            board[IndexX][IndexY] = Mode.Food;
            foodIndex = new int[] { IndexX, IndexY };

            return this;
        }

        private void MakeNormalMove(int moveX, int moveY)
        {
            // board[moveX][moveY] == Mode.Empty
            board[snake.Last.Value[0]][snake.Last.Value[1]] = Mode.Empty;
            snake.RemoveLast();

            if (snake.Count > 0)
                board[snake.First.Value[0]][snake.First.Value[1]] = Mode.Snake;

            board[moveX][moveY] = Mode.SnakeHead;
            snake.AddFirst(new int[2] { moveX, moveY });
        }

        // The return value is a flag to a victory.
        private bool MakeFoodMove(int moveX, int moveY, bool fillMoreFood = true)
        {
            board[snake.First.Value[0]][snake.First.Value[1]] = Mode.Snake;

            board[moveX][moveY] = Mode.SnakeHead;
            snake.AddFirst(new int[2] { moveX, moveY });

            if (fillMoreFood)
            {
                int IndexX = random.Next(0, board.Length), IndexY = random.Next(0, board.Length), count = 0;
                while (board[IndexX][IndexY] != Mode.Empty)
                {
                    IndexX = random.Next(0, board.Length);
                    IndexY = random.Next(0, board.Length);
                    count++;
                    if (count > Math.Pow(board.Length, 2))
                    {
                        for (int i = 0; i < board.Length; i++)
                            for (int j = 0; j < board.Length; j++)
                            {
                                if (board[i][j] == Mode.Empty)
                                {
                                    board[i][j] = Mode.Food;
                                    foodIndex = new int[] { i, j };
                                    return false;
                                }
                            }
                        return true; // Vic
                    }
                }
                board[IndexX][IndexY] = Mode.Food;
                foodIndex = new int[] { IndexX, IndexY };
            }
            return false;
        }

        private int GetMoveX(Mode direction)
        {
            int moveX = 0;
            switch (direction)
            {
                case Mode.Right:
                    moveX = 1;
                    break;
                case Mode.Left:
                    moveX = -1;
                    break;
            }

            return (snake.First.Value[0] + moveX + board.Length) % board.Length;
        }

        private int GetMoveY(Mode direction)
        {
            int moveY = 0;
            switch (direction)
            {
                case Mode.Up:
                    moveY = -1;
                    break;
                case Mode.Down:
                    moveY = 1;
                    break;
            }

            return (snake.First.Value[1] + moveY + board.Length) % board.Length;
        }

        public int? NextMove()
        {
            Mode direction = CalculateNextMove();
            if (direction == Mode.Empty)
                direction = Mode.Up + random.Next(4);
            int moveX = GetMoveX(direction), moveY = GetMoveY(direction);

            if (board[moveX][moveY] == Mode.Empty)
                MakeNormalMove(moveX, moveY);
            else if (board[moveX][moveY] == Mode.Food)
            {
                if (MakeFoodMove(moveX, moveY))
                    return snake.Count;
            }
            else
                return snake.Count;
            
            return null;
        }

        private struct GameBoardWithDirection
        {
            public GameBoard game;
            public Mode direction;
        }
        private Mode CalculateNextMove()
        {
            List<GameBoardWithDirection> NextMoves = new List<GameBoardWithDirection>(4);
            for (int i = 0; i < 4; i++)
            {
                GameBoardWithDirection Next;
                Next.game = (GameBoard)Clone();
                Next.direction = Mode.Up + i;
                Mode NextMode = Next.game.CalculateNextMoveHelper2(Next.direction);
                if (NextMode == Mode.Empty)
                    NextMoves.Add(Next);
                if (NextMode == Mode.Food && AreThereWereToGo(maxDeep * 5))
                    return Next.direction;
            }

            NextMoves.Sort(Comparer<GameBoardWithDirection>.Create((x, y) =>
                x.game.GetFoodManhattanDistance().CompareTo(y.game.GetFoodManhattanDistance())
            ));

            for (int i = 0; i < NextMoves.Count; i++)
            {
                int state = NextMoves[i].game.CalculateNextMoveHelper1(maxDeep);
                if (i == 0 && state == -1)
                {
                    NextMoves.RemoveAt(i);
                    i--;
                    continue;
                }
                if (i == 0 && !NextMoves[i].game.AreThereWereToGo(maxDeep * 5))
                {
                    NextMoves.RemoveAt(i);
                    i--;
                    continue;
                }
                if (state == 1)
                    return NextMoves[i].direction;
            }

            if (NextMoves.Count == 0)
                return Mode.Empty;

            return NextMoves[0].direction;
        }

        private int CalculateNextMoveHelper1(int Deep)
        {
            if (Deep == 0)
                return 0;

            List<GameBoardWithDirection> NextMoves = new List<GameBoardWithDirection>(4);
            for (int i = 0; i < 4; i++)
            {
                GameBoardWithDirection Next;
                Next.game = (GameBoard)Clone();
                Next.direction = Mode.Up + i;
                Mode NextMode = Next.game.CalculateNextMoveHelper2(Next.direction);
                if (NextMode == Mode.Empty)
                    NextMoves.Add(Next);
                if (NextMode == Mode.Food)
                    return 1;
            }

            NextMoves.Sort(Comparer<GameBoardWithDirection>.Create((x, y) =>
                x.game.GetFoodManhattanDistance().CompareTo(y.game.GetFoodManhattanDistance())
            ));

            for (int i = 0; i < NextMoves.Count; i++)
            {
                int state = NextMoves[i].game.CalculateNextMoveHelper1(Deep - 1);
                if (state == -1)
                {
                    NextMoves.RemoveAt(i);
                    i--;
                    continue;
                }
                if (state == 1)
                    return 1;
            }

            if (NextMoves.Count == 0)
                return -1;
            return 0;
        }

        private int CalculateScore()
        {
            return snake.Count * board.Length * 2 - GetFoodManhattanDistance();
        }

        private Mode CalculateNextMoveHelper2(Mode direction)
        {
            int X = GetMoveX(direction), Y = GetMoveY(direction);

            if (board[X][Y] == Mode.Food)
            {
                MakeFoodMove(X, Y);
                return Mode.Food;
            }
            else if (board[X][Y] == Mode.Empty)
            {
                MakeNormalMove(X, Y);
                return Mode.Empty;
            }
            else return Mode.Border;
        }

        private bool AreThereWereToGo(int Deep)
        {
            if (Deep == 0)
                return true;

            List<GameBoardWithDirection> NextMoves = new List<GameBoardWithDirection>(4);
            for (int i = 0; i < 4; i++)
            {
                GameBoardWithDirection Next;
                Next.game = (GameBoard)Clone();
                Next.direction = Mode.Up + i;
                Mode NextMode = Next.game.CalculateNextMoveHelper2(Next.direction);
                if (NextMode == Mode.Empty || NextMode == Mode.Food)
                    NextMoves.Add(Next);
            }

            for (int i = 0; i < NextMoves.Count; i++)
            {
                if (NextMoves[i].game.AreThereWereToGo(Deep - 1))
                    return true;
                NextMoves.RemoveAt(i);
                i--;
            }

            return false;
        }

        private int GetFoodManhattanDistance()
        {
            int X1 = (board.Length + foodIndex[0] - snake.First.Value[0]) % board.Length,
                X2 = (board.Length - foodIndex[0] + snake.First.Value[0]) % board.Length,
                Y1 = (board.Length + foodIndex[1] - snake.First.Value[1]) % board.Length,
                Y2 = (board.Length - foodIndex[1] + snake.First.Value[1]) % board.Length;

            return (X1 < X2 ? X1 : X2) + (Y1 < Y2 ? Y1 : Y2);
        }
    }
}
