using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Snake_Game_Project
{
    public partial class Form1 : Form
    {
        private List<Circle> Snake = new List<Circle>();
        private Circle food = new Circle();
        private Random rnd = new Random();
        private int gridColumns;
      private int gridRows;
        private bool isInitialized = false;

        public Form1()
        {
          // Initialize Settings BEFORE InitializeComponent to prevent DivideByZeroException
            new Settings();

            InitializeComponent();

      // Set KeyPreview to true to capture key events
     this.KeyPreview = true;

     // Set form to fullscreen maximized state
  this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MinimizeBox = true;
            this.MaximizeBox = true;

        // Apply dark theme
            ApplyDarkTheme();

  // Hook up custom paint for disabled button to keep white text
    btnPause.Paint += BtnPause_Paint;

          gameTimer.Interval = 1000 / Settings.Speed;
            gameTimer.Tick += updateScreen;

         // Mark as initialized
          isInitialized = true;

            // Don't start the game automatically, show start screen
showStartScreen();
      }

  private void BtnPause_Paint(object sender, PaintEventArgs e)
        {
    // Custom paint to ensure white text even when disabled
            Button btn = (Button)sender;
        
        // Fill background
            using (SolidBrush bgBrush = new SolidBrush(Color.FromArgb(50, 50, 50)))
            {
 e.Graphics.FillRectangle(bgBrush, btn.ClientRectangle);
      }

       // Draw border
          using (Pen borderPen = new Pen(Color.Orange, 2))
            {
     e.Graphics.DrawRectangle(borderPen, 1, 1, btn.Width - 3, btn.Height - 3);
            }

// Draw text in white (always)
            using (SolidBrush textBrush = new SolidBrush(Color.White))
    {
        StringFormat sf = new StringFormat();
      sf.Alignment = StringAlignment.Center;
      sf.LineAlignment = StringAlignment.Center;
          e.Graphics.DrawString(btn.Text, btn.Font, textBrush, btn.ClientRectangle, sf);
            }
   }

        private void ApplyDarkTheme()
        {
      // Form background
    this.BackColor = Color.FromArgb(30, 30, 30);

            // Canvas (game area) - darker gray
     pbCanvas.BackColor = Color.FromArgb(20, 20, 20);

      // Labels - white text
     label1.ForeColor = Color.White;
         label2.ForeColor = Color.Cyan;
    label3.ForeColor = Color.Red;
lblHighScoreLabel.ForeColor = Color.White;
            lblHighScore.ForeColor = Color.Gold;

      // Buttons - dark theme
        btnStart.BackColor = Color.FromArgb(50, 50, 50);
            btnStart.ForeColor = Color.Lime;
    btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.FlatAppearance.BorderColor = Color.Lime;
 btnStart.FlatAppearance.BorderSize = 2;

            btnPause.BackColor = Color.FromArgb(50, 50, 50);
 btnPause.ForeColor = Color.White;
  btnPause.FlatStyle = FlatStyle.Flat;
   btnPause.FlatAppearance.BorderColor = Color.Orange;
            btnPause.FlatAppearance.BorderSize = 2;

    // Instructions panel - dark theme
            pnlInstructions.BackColor = Color.FromArgb(40, 40, 40);
            pnlInstructions.BorderStyle = BorderStyle.FixedSingle;
 lblInstructions.ForeColor = Color.White;
       lblInstructions.BackColor = Color.FromArgb(40, 40, 40);
      }

private void Form1_Resize(object sender, EventArgs e)
   {
         // Safety check to prevent divide by zero
   if (!isInitialized || Settings.Width == 0 || Settings.Height == 0)
                return;

// Recalculate canvas size based on form size
        int headerHeight = 100;
        int margin = 20;

      pbCanvas.Width = this.ClientSize.Width - (margin * 2);
        pbCanvas.Height = this.ClientSize.Height - headerHeight - margin;
       pbCanvas.Location = new Point(margin, headerHeight);

            // Calculate grid dimensions
            gridColumns = pbCanvas.Width / Settings.Width;
        gridRows = pbCanvas.Height / Settings.Height;

            // Position buttons with equal margins from top
            int buttonMarginTop = 20;
  int buttonMarginRight = 20;
    int buttonSpacing = 20;
            int buttonWidth = 140;
            int buttonHeight = 60;

            // Position Pause button (rightmost)
 btnPause.Location = new Point(
       this.ClientSize.Width - buttonMarginRight - buttonWidth,
       buttonMarginTop
     );
            btnPause.Size = new Size(buttonWidth, buttonHeight);

            // Position Start button (left of Pause)
         btnStart.Location = new Point(
   btnPause.Left - buttonSpacing - buttonWidth,
 buttonMarginTop
            );
            btnStart.Size = new Size(buttonWidth, buttonHeight);

            // Center instructions panel (now larger: 700x350)
            pnlInstructions.Location = new Point(
    (this.ClientSize.Width - pnlInstructions.Width) / 2,
    (this.ClientSize.Height - pnlInstructions.Height) / 2
            );

    pbCanvas.Invalidate();
        }

     protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
   // Handle arrow keys directly to prevent default behavior
    if (Settings.CurrentGameState == GameState.Playing)
       {
             switch (keyData)
           {
       case Keys.Up:
        if (Settings.direction != Directions.Down)
       {
            Settings.direction = Directions.Up;
       }
          return true; // Key handled
          case Keys.Down:
    if (Settings.direction != Directions.Up)
           {
            Settings.direction = Directions.Down;
        }
      return true;
 case Keys.Left:
     if (Settings.direction != Directions.Right)
       {
   Settings.direction = Directions.Left;
      }
       return true;
              case Keys.Right:
             if (Settings.direction != Directions.Left)
       {
    Settings.direction = Directions.Right;
      }
    return true;
 case Keys.Space:
 pauseGame();
  return true;
     }
  }
        else if (Settings.CurrentGameState == GameState.Paused)
    {
       if (keyData == Keys.Space)
        {
 resumeGame();
          return true;
      }
  }
            else if (Settings.CurrentGameState == GameState.GameOver)
            {
   if (keyData == Keys.Enter)
 {
  startGame();
     return true;
       }
     }

            return base.ProcessCmdKey(ref msg, keyData);
        }

 private void updateScreen(object sender, EventArgs e)
        {
   if (Settings.CurrentGameState == GameState.Playing)
       {
            movePlayer();
            }

            pbCanvas.Invalidate();
      }

        private void movePlayer()
        {
       for (int i = Snake.Count - 1; i >= 0; i--)
   {
              if (i == 0)
        {
         switch (Settings.direction)
  {
               case Directions.Right:
       Snake[i].x++;
    break;
       case Directions.Left:
          Snake[i].x--;
 break;
    case Directions.Up:
         Snake[i].y--;
   break;
  case Directions.Down:
     Snake[i].y++;
              break;
         }

    int maxXPos = pbCanvas.Size.Width / Settings.Width;
int maxYPos = pbCanvas.Size.Height / Settings.Height;

    if (Snake[i].x < 0 || Snake[i].y < 0 || Snake[i].x >= maxXPos || Snake[i].y >= maxYPos)
             {
   die();
                   return;
   }

               for (int j = 1; j < Snake.Count; j++)
         {
       if (Snake[i].x == Snake[j].x && Snake[i].y == Snake[j].y)
  {
         die();
      return;
     }
             }

    if (Snake[0].x == food.x && Snake[0].y == food.y)
        {
   eat();
}
   }
           else
 {
            Snake[i].x = Snake[i - 1].x;
           Snake[i].y = Snake[i - 1].y;
              }
}
      }

  private void keyisdown(object sender, KeyEventArgs e)
        {
            Input.ChangeState(e.KeyCode, true);
        }

        private void keyisup(object sender, KeyEventArgs e)
        {
   Input.ChangeState(e.KeyCode, false);
        }

        private void updateGraphics(object sender, PaintEventArgs e)
    {
      Graphics canvas = e.Graphics;
            canvas.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            // Draw grid lines
    DrawGrid(canvas);

     if (Settings.CurrentGameState == GameState.Playing || Settings.CurrentGameState == GameState.Paused)
        {
        // Draw snake with glow effect for dark theme
                Brush snakeColor;
     for (int i = 0; i < Snake.Count; i++)
       {
      if (i == 0)
    {
     // Snake head - bright lime green for dark theme
      snakeColor = Brushes.Lime;
  }
                    else
     {
             // Snake body - darker green
                snakeColor = Brushes.ForestGreen;
   }

          // Draw with border for better visibility
     Rectangle rect = new Rectangle(Snake[i].x * Settings.Width,
         Snake[i].y * Settings.Height,
     Settings.Width, Settings.Height);
          canvas.FillEllipse(snakeColor, rect);

       // Add border to snake segments
     using (Pen borderPen = new Pen(Color.DarkGreen, 2))
    {
       canvas.DrawEllipse(borderPen, rect);
            }
    }

                // Draw food - bright red/orange for dark theme with border
            Rectangle foodRect = new Rectangle(food.x * Settings.Width,
        food.y * Settings.Height,
   Settings.Width, Settings.Height);
     canvas.FillEllipse(Brushes.OrangeRed, foodRect);
     using (Pen foodBorder = new Pen(Color.Red, 2))
             {
     canvas.DrawEllipse(foodBorder, foodRect);
      }

      // Draw "PAUSED" text if paused
 if (Settings.CurrentGameState == GameState.Paused)
     {
        string pausedText = "PAUSED";
        Font font = new Font("Arial", 48, FontStyle.Bold);
        SizeF textSize = canvas.MeasureString(pausedText, font);
             float x = (pbCanvas.Width - textSize.Width) / 2;
       float y = (pbCanvas.Height - textSize.Height) / 2;

          // Draw shadow
             canvas.DrawString(pausedText, font, Brushes.Black, x + 3, y + 3);
  // Draw text in cyan for dark theme
            canvas.DrawString(pausedText, font, Brushes.Cyan, x, y);

    string resumeText = "Press SPACE or click Resume to continue";
      Font smallFont = new Font("Arial", 16);
         SizeF smallTextSize = canvas.MeasureString(resumeText, smallFont);
            float smallX = (pbCanvas.Width - smallTextSize.Width) / 2;
        float smallY = y + textSize.Height + 20;
        canvas.DrawString(resumeText, smallFont, Brushes.LightGray, smallX, smallY);
       }
    }
         else if (Settings.CurrentGameState == GameState.GameOver)
        {
         string gameOver = "Game Over \nYour final score is: " + Settings.Score + "\nPress Enter or click Start to Restart";
           label3.Text = gameOver;
                label3.Visible = true;
            }
  }

        private void DrawGrid(Graphics canvas)
  {
         // Safety check
 if (Settings.Width == 0 || Settings.Height == 0)
              return;

     int maxXPos = pbCanvas.Width / Settings.Width;
      int maxYPos = pbCanvas.Height / Settings.Height;

    using (Pen gridPen = new Pen(Color.FromArgb(40, 40, 40), 1))
          {
                // Draw vertical lines
   for (int x = 0; x <= maxXPos; x++)
    {
      canvas.DrawLine(gridPen, x * Settings.Width, 0, x * Settings.Width, maxYPos * Settings.Height);
   }

 // Draw horizontal lines
     for (int y = 0; y <= maxYPos; y++)
  {
          canvas.DrawLine(gridPen, 0, y * Settings.Height, maxXPos * Settings.Width, y * Settings.Height);
           }
            }
        }

        private void showStartScreen()
        {
         Settings.CurrentGameState = GameState.StartScreen;
       pnlInstructions.Visible = true;
            pnlInstructions.BringToFront();
 label3.Visible = false;
            btnStart.Enabled = true;
            btnPause.Enabled = false;
  btnStart.Text = "Start";
         lblHighScore.Text = Settings.HighScore.ToString();
 btnPause.Invalidate(); // Force repaint with white text
        }

        private void startGame()
    {
          label3.Visible = false;
       pnlInstructions.Visible = false;

 new Settings();
     Snake.Clear();

int maxXPos = pbCanvas.Width / Settings.Width;
            int maxYPos = pbCanvas.Height / Settings.Height;

            Circle head = new Circle();
            head.x = maxXPos / 2;
            head.y = maxYPos / 2;
            Snake.Add(head);

    Circle body1 = new Circle();
   body1.x = head.x - 1;
 body1.y = head.y;
    Snake.Add(body1);

        Circle body2 = new Circle();
       body2.x = head.x - 2;
    body2.y = head.y;
            Snake.Add(body2);

 label2.Text = Settings.Score.ToString();
  lblHighScore.Text = Settings.HighScore.ToString();
            generateFood();
  Settings.direction = Directions.Right;
     Settings.CurrentGameState = GameState.Playing;
    Settings.GameOver = false;

            btnStart.Text = "Restart";
   btnStart.Enabled = true;
    btnPause.Enabled = true;
 btnPause.Text = "Pause";
btnPause.Invalidate(); // Force repaint

         gameTimer.Start();
  pbCanvas.Invalidate();
        }

        private void pauseGame()
        {
         Settings.CurrentGameState = GameState.Paused;
      btnPause.Text = "Resume";
    btnPause.Invalidate(); // Force repaint
       pbCanvas.Invalidate();
    }

        private void resumeGame()
        {
          Settings.CurrentGameState = GameState.Playing;
      btnPause.Text = "Pause";
     btnPause.Invalidate(); // Force repaint
            pbCanvas.Invalidate();
        }

        private void generateFood()
        {
            int maxXPos = pbCanvas.Size.Width / Settings.Width;
            int maxYPos = pbCanvas.Size.Height / Settings.Height;

   bool foodOnSnake;
   do
       {
       foodOnSnake = false;
       food.x = rnd.Next(0, maxXPos);
        food.y = rnd.Next(0, maxYPos);

      foreach (Circle segment in Snake)
         {
       if (segment.x == food.x && segment.y == food.y)
      {
         foodOnSnake = true;
       break;
 }
                }
            } while (foodOnSnake);
        }

     private void eat()
{
            Circle body = new Circle();
            body.x = Snake[Snake.Count - 1].x;
      body.y = Snake[Snake.Count - 1].y;
 Snake.Add(body);
      Settings.Score += Settings.Points;
        label2.Text = Settings.Score.ToString();

        // Update high score if current score exceeds it
   if (Settings.Score > Settings.HighScore)
            {
   Settings.HighScore = Settings.Score;
                lblHighScore.Text = Settings.HighScore.ToString();
    }

     generateFood();
        }

        void die()
{
            Settings.GameOver = true;
            Settings.CurrentGameState = GameState.GameOver;
       btnPause.Enabled = false;
     btnStart.Enabled = true;
      btnPause.Invalidate(); // Force repaint with white text
        }

        private void Form1_Load(object sender, EventArgs e)
      {
    this.StartPosition = FormStartPosition.CenterScreen;

        // Trigger resize to set up initial canvas size
        Form1_Resize(this, EventArgs.Empty);
        }

        private void btnStart_Click(object sender, EventArgs e)
    {
      startGame();
        }

        private void btnPause_Click(object sender, EventArgs e)
{
            if (Settings.CurrentGameState == GameState.Playing)
            {
     pauseGame();
            }
    else if (Settings.CurrentGameState == GameState.Paused)
            {
          resumeGame();
        }
        }

        private void label3_Click(object sender, EventArgs e)
        {
        }

        private void label1_Click(object sender, EventArgs e)
   {
        }
  }
}