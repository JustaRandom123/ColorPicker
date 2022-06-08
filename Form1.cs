using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MetroFramework;

namespace ColorPicker
{

    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public static MetroFramework.Forms.MetroForm mainFrame = null;
        public Image picture;
        private Point pictureLocation;
        private bool mouseInPic = false;
        private Point latestMousePosition;
        private Size originalSize;
        private Point originalLocation;

        private int wheelCounter = 1;   

        public Form1()
        {
            InitializeComponent();
            this.AllowDrop = true;
            this.DragDrop += Form1_DragDrop;
            this.DragEnter += Form1_DragEnter;
            this.MouseMove += Form1_MouseMove;
            this.MouseDown += Form1_MouseClick;
            this.MouseWheel += Form1_MouseWheel;
            mainFrame = this;
        }

        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (picture != null)
            {
                if (e.Delta > 0)
                {
                    if (wheelCounter < 3)
                    {
                        wheelCounter++;
                        zoomInAndOut(wheelCounter, "in",e.Location);
                    }                                 
                }
                else
                {
                    if (wheelCounter > 1)
                    {
                        wheelCounter--;
                        zoomInAndOut(wheelCounter, "out", e.Location);
                    }    
                   // else
                   // {
                        //pictureLocation = originalLocation;
                      // this.Invalidate();
                  //  }
                }            
            }
        }

        private void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (mouseInPic == false) { return; }
            if (e.Button == MouseButtons.Left && mouseInPic == true)
            {
               Clipboard.SetText(GetPixel(new Point(latestMousePosition.X, latestMousePosition.Y)).ToString());
               CustomBox.createBox(this, "Color", GetPixel(new Point(latestMousePosition.X, latestMousePosition.Y)).ToString(), new Point(latestMousePosition.X, latestMousePosition.Y), GetPixel(new Point(latestMousePosition.X, latestMousePosition.Y)));             
            }
        }

        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            if (picture != null)
            {             
                if (e.Location.X < picture.Width + pictureLocation.X && e.Location.Y > pictureLocation.Y && e.Location.Y < picture.Height + pictureLocation.Y)
                {
                    this.Cursor = Cursors.Cross;
                    mouseInPic = true;
                    latestMousePosition = e.Location;
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    mouseInPic = false;
                    latestMousePosition = new Point();
                }              
            }
        }

        private void zoomInAndOut(int factor,string direction,Point cursor)
        {
            using (var bitmap = new Bitmap(picture))
            {
                if (direction == "in")
                {
                    Console.WriteLine("Scrolled up " + wheelCounter);
                    Size newSize = new Size((int)(picture.Width * factor), (int)(picture.Height * factor));
                    Bitmap bmp = new Bitmap(picture, newSize);
                    picture = bmp;
                    pictureLocation.Y = (int)(cursor.Y - 1.25 * (cursor.Y - pictureLocation.Y));
                    pictureLocation.X = (int)(cursor.X - 1.25 * (cursor.X - pictureLocation.X));
                    this.Invalidate();

                }
                else if (direction == "out")
                {
                    Console.WriteLine("Scrolled down " + wheelCounter);
                    Size newSize = new Size((int)(originalSize.Width * factor), (int)(originalSize.Height * factor));
                    Bitmap bmp = new Bitmap(picture, newSize);
                    picture = bmp;
                    pictureLocation.Y = (int)(cursor.Y - 0.80 * (cursor.Y - pictureLocation.Y));
                    pictureLocation.X = (int)(cursor.X - 0.80 * (cursor.X - pictureLocation.X));
                    this.Invalidate();
                }
            }
        }


        Color GetPixel(Point position)
        {
            using (var bitmap = new Bitmap(picture))
            {
                return bitmap.GetPixel(position.X - pictureLocation.X, position.Y - pictureLocation.Y);  //- 80
            }
        }

    
        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
          
            if (e.Data.GetDataPresent(DataFormats.Bitmap) || e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.Count() > 1)
                {
                    MessageBox.Show("Please only drag one file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }


                var ext = System.IO.Path.GetExtension(files[0]);
                if (ext.Equals(".png", StringComparison.CurrentCultureIgnoreCase) || ext.Equals(".jpg", StringComparison.CurrentCultureIgnoreCase))
                {                               
                    this.picture = Image.FromFile(files[0]);
                    this.pictureLocation = new Point(0,80);
                    originalSize = this.picture.Size;
                    originalLocation = this.pictureLocation;
                }
                else
                {
                    MessageBox.Show("Wrong file extension! Allowed [.png,.jpg]", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

            }       
            this.Invalidate();
        }


        protected override void OnPaint(PaintEventArgs e)
        {
          
            base.OnPaint(e);
            if (this.picture != null && this.pictureLocation != Point.Empty)
            {
                e.Graphics.DrawImage(this.picture, this.pictureLocation);
                GraphicsUnit units = GraphicsUnit.Point;
                RectangleF bmpRectangleF = this.picture.GetBounds(ref units);
                Rectangle bmpRectangle = Rectangle.Round(bmpRectangleF);
                bmpRectangle.Location = this.pictureLocation;   
              
                e.Graphics.DrawRectangle(Pens.Red, bmpRectangle);


            }
        }


        private void Form1_Load(object sender, EventArgs e)
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;

            this.Size = new Size(screenWidth, screenHeight);
        }
    }


    class CustomBox
    {
        public static MetroFramework.Controls.MetroPanel activeCustomBox = null;

        public static void createBox(Control attachedTo, string headerText, string description, Point location, Color color)
        {
            if (activeCustomBox != null)
            {
                Form1.mainFrame.Controls.Remove(activeCustomBox);
            }

            var width = attachedTo.Width / 2;
            var height = attachedTo.Height / 2;

            MetroFramework.Controls.MetroPanel window = new MetroFramework.Controls.MetroPanel();
            window.Size = new Size(200, 125);
            window.Location = new Point(width / window.Width / 2, height / window.Height / 2);
            window.Text = headerText;
            window.Style = MetroColorStyle.Silver;
            window.Theme = MetroThemeStyle.Dark;
            window.Location = location;



            MetroFramework.Controls.MetroPanel colorShowcase = new MetroFramework.Controls.MetroPanel();
            colorShowcase.Size = new Size(50, 50);
            colorShowcase.Location = new Point(1, 1);
            colorShowcase.UseCustomBackColor = true;
            colorShowcase.BackColor = color;
            colorShowcase.Theme = MetroThemeStyle.Light;


            MetroFramework.Controls.MetroLabel text = new MetroFramework.Controls.MetroLabel();
            text.Style = MetroColorStyle.Silver;
            text.Theme = MetroThemeStyle.Dark;
            text.Size = new Size(window.Width, 25);
            text.Text = description;
            text.FontSize = MetroLabelSize.Small;
            text.FontWeight = MetroLabelWeight.Regular;
            text.Location = new Point(1, 56);


            MetroFramework.Controls.MetroButton okButton = new MetroFramework.Controls.MetroButton();
            okButton.Style = MetroColorStyle.Silver;
            okButton.Theme = MetroThemeStyle.Dark;
            okButton.Size = new Size(window.Width, 25);
            okButton.Text = "OK";
            okButton.Location = new Point(0, window.Height - okButton.Height);
            okButton.Tag = window;
            okButton.Click += OkButton_Click;


            window.Controls.Add(text);
            window.Controls.Add(colorShowcase);
            window.Controls.Add(okButton);
            Form1.mainFrame.Controls.Add(window);
            activeCustomBox = window;
        }

        private static void OkButton_Click(object sender, EventArgs e)
        {
            Form1.mainFrame.Controls.Remove(activeCustomBox);
            activeCustomBox = null;
        }
    }
}
