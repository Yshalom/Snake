using System.Xml.Serialization;

namespace SnakeGame
{
    public partial class Form1 : Form
    {
        private const int BoardSize = 25;
        private const int SnakeSpeed = 30;
        private bool[] NextMoveThreadPlay = new bool[1];

        GameBoard gameBoard;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            gameBoard = new GameBoard(BoardSize);
            ShowGame();
        }
        
        private void ShowGame()
        {
            GamePictureBox.Image = gameBoard.GetBoardToImage();
        }

        private void ShowGameOver(int score)
        {
            Image image = gameBoard.GetBoardToImage();
            Graphics g = Graphics.FromImage(image);
            g.FillRectangle(new Pen(Color.FromArgb(70, 255, 218, 218)).Brush, 0, 0, 301, 301);
            g.DrawString("Game Over", new Font("Calibri, Arial", 37), new Pen(Color.FromArgb(200, 0, 150, 0)).Brush, 15, 80);
            g.DrawString("Score: " + score.ToString(), new Font("Calibri, Arial", 22), new Pen(Color.FromArgb(200, 0, 0, 255)).Brush, 60, 150);
            GamePictureBox.Image = image;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            gameBoard = new GameBoard(BoardSize);
            ShowGame();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            NextMoveThreadPlay[0] = true;

            Thread thread = new Thread((object? playThread) =>
            {
                int index = 0;
                while (((bool[])playThread)[0] && index < 1000000)
                {
                    index++;
                    int? score = gameBoard.NextMove();
                    if (score != null)
                    {
                        ShowGameOver(score.Value);
                        return;
                    }
            
                    ShowGame();
                    Thread.Sleep(SnakeSpeed);
                }
            });
            
            thread.Start(NextMoveThreadPlay);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            NextMoveThreadPlay[0] = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            NextMoveThreadPlay[0] = false;
        }
    }
}
