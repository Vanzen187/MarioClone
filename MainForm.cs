using System;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.IO;

//TODO: Make optimize collision and make into a function
namespace MarioWindows
{

	struct Entity
	{
		public int left {get;set;}
		public int leftProgress {get;set;}
		public int top {get;set;}
		public int image {get;set;}
		public int leftBound {get;set;}
		public int topBound {get;set;}
		public bool isGround {get;set;}
		public bool isEnemy {get;set;}
	}

	public class DoubleBufferedPanel : Panel
	{
		public DoubleBufferedPanel()
		{
			this.SetStyle(ControlStyles.AllPaintingInWmPaint |
				ControlStyles.OptimizedDoubleBuffer |
				ControlStyles.UserPaint, true);
		}
	}

	class MainForm : Form
	{
		protected override CreateParams CreateParams 
		{
			get 
			{
				CreateParams cp = base.CreateParams;
				cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
				return cp;
			}
		} 

		int ground = 600;
		int xAccel = 5;
		int yAccel = 0;
		int panelHeight = 650;
		int panelWidth = 1050;
		int gravity = 1;
		int jumpSpeed = -20;
		int marioXSize = 50;
		int marioYSize = 50;
		int squareXSize = 50;
		int squareYSize = 50;
		int xPos = 130;
		int yPos = 50;
		int walkingIterator = 0;
		int spriteLength = 10;
		int spriteFrame = 0;
		int onRow = 0;
		int left1;

		bool moveLeft;
		bool moveRight;
		bool isJumping;
		bool isFacingRight;

		string filepath = @"C:\Users\EVAN.uRZEN\Desktop\CS\MarioWindows";
		//string filepath = @"C:\Users\Evan\Desktop\MarioWindows";
		string line;
		string[] row;

		StreamReader reader;

		Image currentMario;
		Image standingRight;
		Image standingLeft;
		Image jumpingRight;
		Image jumpingLeft;
		Image step1Right;
		Image step1Left;
		Image step2Right;
		Image step2Left;
		Image ground1;

		Image[] walkingRight;
		Image[] walkingLeft;

		List<Entity[]> column = new List<Entity[]>();

		DoubleBufferedPanel mainPanel;
		PictureBox pbBackground1 = new PictureBox ();
		PictureBox pbBackground2 = new PictureBox ();
		Entity eMario;
		Timer tmrMovement = new Timer {Interval = 15};

		[STAThread]
		public static void Main()
		{
			Application.EnableVisualStyles();
			Application.Run(new MainForm());
		}

		public MainForm()
		{
			initialize ();
		}
			
		private void mainPanel_Paint(object sender, PaintEventArgs e)
		{
			var g = e.Graphics;

			g.DrawImage(currentMario, eMario.left, eMario.top, marioXSize, marioYSize);

			for (int i = 0; i < column.Count; i++)
			{
				for (int j = 0; j < column [i].Length; j++)
				{
					if (column[i][j].image == 1)
						g.DrawImage(ground1, column[i][j].left, column[i][j].top, squareXSize, squareYSize);
				}
			}
			onRow++;
		}


		public void initialize ()
		{
			tmrMovement.Tick += TmrMovement_Tick;
			this.Size = new Size (panelWidth, panelHeight + 50);

			mainPanel = new DoubleBufferedPanel();
			mainPanel.Size = new Size (panelWidth, panelHeight);
			mainPanel.Left = 0;
			mainPanel.BackColor = Color.FromArgb(153, 217, 234);
			this.DoubleBuffered = true;
			//mainPanel.Anchor = (AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right);

			this.Controls.Add(mainPanel);

			this.KeyPreview = true;
			this.KeyDown += new KeyEventHandler(MainForm_KeyDown);
			this.KeyUp += new KeyEventHandler(MainForm_KeyUp);

			isFacingRight = true;
			pbBackground1.BackgroundImage = Image.FromFile(@"..\..\Assets\Backgrounds\HappyClouds.png");
			pbBackground2.BackgroundImage = Image.FromFile(@"..\..\Assets\Backgrounds\HappyClouds.png");

			/*pbBackground1.Location = new Point (0, 0);
			pbBackground1.Size = new Size(1025, 640);
			pbBackground1.BackgroundImageLayout = ImageLayout.Zoom;
			pbBackground1.Parent = mainPanel;
			//pbBackground1.Anchor = (AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right);

			pbBackground2.Location = new Point (pbBackground1.Width, 0);
			pbBackground2.Size = new Size(1025, 640);
			pbBackground2.BackgroundImageLayout = ImageLayout.Zoom;
			pbBackground2.Parent = mainPanel;
			//pbBackground2.Anchor = (AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right);
*/
			standingLeft =  Image.FromFile(@"..\..\Assets\Sprites\MarioSprite\MarioStandingLeft.png");
			standingRight =  Image.FromFile( @"..\..\Assets\Sprites\MarioSprite\MarioStandingRight.png");
			jumpingLeft =  Image.FromFile(@"..\..\Assets\Sprites\MarioSprite\MarioJumpingLeft.png");
			jumpingRight =  Image.FromFile(@"..\..\Assets\Sprites\MarioSprite\MarioJumpingRight.png");
			step1Right = Image.FromFile(@"..\..\Assets\Sprites\MarioSprite\MarioStep1Right.png");
			step1Left = Image.FromFile(@"..\..\Assets\Sprites\MarioSprite\MarioStep1Left.png");
			step2Right = Image.FromFile(@"..\..\Assets\Sprites\MarioSprite\MarioStep2Right.png");
			step2Left = Image.FromFile(@"..\..\Assets\Sprites\MarioSprite\MarioStep2Left.png");
			ground1 = Image.FromFile(@"..\..\Assets\Sprites\Blocks\Ground1.png");

			currentMario = standingRight;
			eMario.top = 11 * 50;


			reader = new StreamReader(File.OpenRead(@"..\..\testLevel.csv"));
			while (!reader.EndOfStream && onRow <=12)
			{
				line = reader.ReadLine ();
				row = line.Split (',');


				for(int i = 0; i<row.Length; i++)
				{
					if (onRow == 0)
						column.Add (new Entity[13]);
					column [i] [onRow].image = Int32.Parse (row [i]);
					column [i] [onRow].left = i * squareXSize;
					column [i] [onRow].leftProgress = i * squareXSize;
					column [i] [onRow].top = onRow * squareYSize;

				}
				onRow++;
			}
		
			walkingRight = new Image[] { step1Right, standingRight, step2Right, standingRight };
			walkingLeft = new Image[] { step1Left, standingLeft, step2Left, standingLeft };

			mainPanel.Paint += new PaintEventHandler (mainPanel_Paint);
		}

//		public void addUnit

		public void updateLocation()
		{

			if ((moveRight || moveLeft) && !(moveRight && moveLeft))
			{
				if (moveLeft)
				{
					if (!isJumping && spriteFrame == spriteLength)
					{
						currentMario = walkingLeft [walkingIterator];
						if (walkingIterator == walkingLeft.Length - 1)
							walkingIterator = 0;
						else
							walkingIterator++;
						spriteFrame = 0;
					}
					if (eMario.left - xAccel <= 0)
						eMario.left = 0;
					else
					{
						eMario.left -= xAccel;
						eMario.leftProgress -= xAccel;
					}
					spriteFrame++;

				}


				if (moveRight)
				{
					if (!isJumping && spriteFrame == spriteLength)
					{
						currentMario = walkingRight [walkingIterator];
						if (walkingIterator == walkingRight.Length - 1)
							walkingIterator = 0;
						else
							walkingIterator++;
						spriteFrame = 0;
					}
					if (eMario.left + squareXSize + xAccel >= panelWidth / 2)
					{
						eMario.left = panelWidth / 2 - squareXSize;
						//					pbBackground1.Left -= xAccel;
						//					pbBackground2.Left -= xAccel;
						for (int i = 0; i < column.Count; i++)
						{
							for (int j = 0; j < column [i].Length; j++)
							{
								if (column [i] [j].image > 0)
									column [i] [j].left -= xAccel;
								//column [i] [j].Location = new Point(column[i][j].left - xAccel, column[i][j].left);
							}
						}

						//					if (pbBackground1.Left + pbBackground1.Width <= 0)
						//						pbBackground1.Left = pbBackground2.Left + pbBackground2.Width;
						//					if (pbBackground2.Left + pbBackground2.Width <= 0)
						//						pbBackground2.Left = pbBackground1.Left + pbBackground1.Width;
						
					}
					else
						eMario.left += xAccel;
					spriteFrame++;
					eMario.leftProgress += xAccel;
				}

				eMario.leftBound = eMario.leftProgress / squareXSize;

				for (int i = eMario.leftBound; i < eMario.leftBound + 2; i++)
				{
					for(int j = eMario.topBound; j < eMario.topBound + 2; j++)
					{
						if (column [i] [j].image > 0)
					{
							if (eMario.left < column [i] [j].left + squareXSize &&
								eMario.left + squareXSize > column [i] [j].left &&
								eMario.top < column [i] [j].top + squareYSize &&
								eMario.top + squareYSize > column [i] [j].top)
							{
								if (moveRight)
								{
									eMario.left = column [i] [j].left - squareXSize;
									eMario.leftProgress = column [i] [j].leftProgress - squareXSize;
								} else
								{
									eMario.left = column [i] [j].left + squareXSize;
									eMario.leftProgress = column [i] [j].leftProgress + squareXSize ;
								}
								//xAccel = 0;
							}

						}

					}
				}

			}


			if (isJumping)
			{
				if(isFacingRight)
					currentMario = jumpingRight;
				else
					currentMario = jumpingLeft;
				yAccel += gravity;
				if (eMario.top + squareYSize + yAccel > 13 * 50)
				{
					if (isFacingRight)
						currentMario = standingRight;
					else
						currentMario = standingLeft;
					eMario.top = panelHeight - squareYSize;
					yAccel = 0;
					isJumping = false;
					spriteFrame = spriteLength;
				} else
					eMario.top += yAccel;
			}

			eMario.topBound = eMario.top / squareYSize;

			for (int i = eMario.leftBound; i < eMario.leftBound + 2; i++)
			{
				for(int j = eMario.topBound; j < eMario.topBound + 2; j++)
				{
					if (column [i] [j].image > 0)
					{
						if (eMario.left < column [i] [j].left + squareXSize &&
							eMario.left + squareXSize > column [i] [j].left &&
							eMario.top < column [i] [j].top + squareYSize &&
							eMario.top + squareYSize > column [i] [j].top)
						{
							if (yAccel < 0)
								eMario.top = column [i] [j].top + squareYSize;
							else
							{
								eMario.top = column [i] [j].top - squareYSize;
								if (isFacingRight)
									currentMario = standingRight;
								else
									currentMario = standingLeft;
								spriteFrame = 10;
								isJumping = false;
							}
							yAccel = 0;
						}

					}

				}
			}


			mainPanel.Invalidate();
		}

		void TmrMovement_Tick (object sender, EventArgs e)
		{
			updateLocation ();
		}


		void MainForm_KeyDown(object sender, KeyEventArgs e)
		{

 			switch (e.KeyCode)
			{
			//Up
			case Keys.Space:
				if (isJumping)
					return;
 				isJumping = true;
				yAccel = jumpSpeed;
				break;

			//Left
			case Keys.A:
				if (moveLeft)
					return;
				spriteFrame = spriteLength;
				isFacingRight = false;
				moveLeft = true;
				break;
			
			//Right
			case Keys.D:
				if (moveRight)
					return;
				spriteFrame = spriteLength;
				isFacingRight = true;
				moveRight = true;
				break;
			}

			updateLocation ();
			tmrMovement.Start ();
		}

		void MainForm_KeyUp(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				//Left
			case Keys.A:
				moveLeft = false;
				break;

				//Right
			case Keys.D:
				moveRight = false;
				break;
			}

			if (!(isJumping || moveLeft || moveRight)) 
			{
				if (isFacingRight)
					currentMario = standingRight;
				else
					currentMario = standingLeft;
				spriteFrame = 0;
				tmrMovement.Stop ();
			}
		}
	} //class MainForm : Form
} //namespace MarioWindows