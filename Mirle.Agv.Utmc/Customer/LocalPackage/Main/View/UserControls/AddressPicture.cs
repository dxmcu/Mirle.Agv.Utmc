using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mirle.Agv.INX.Model;

namespace Mirle.Agv.INX.View
{
    public partial class AddressPicture : UserControl
    {
        public string AddressID { get; set; } = "";
        public PictureBox PB_Address { get; set; }
        public Bitmap Bitmap_Address { get; set; }

        public AddressPicture(MapAddress address, int pictureWidth, int width, float x, float y)
        {
            this.AddressID = address.Id;
            InitializeComponent();
            InitailPictrue(pictureWidth, width, address);
            this.Location = new Point((int)(x - pictureWidth / 2), (int)(y - pictureWidth / 2));
            //PB_Address.Location = this.Location;
        }

        private void InitailPictrue(int pictureWidth, int width, MapAddress address)
        {
            PB_Address = new PictureBox();
            this.Controls.Add(PB_Address);

            this.BackColor = Color.White;
            int deltaMin = width / 2;
            int deltaMax = (width + 1) / 2;
            this.Size = new Size(pictureWidth, pictureWidth);
            PB_Address.Size = new Size(pictureWidth, pictureWidth);

            Bitmap bitmap = new Bitmap(pictureWidth, pictureWidth);
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.Clear(Color.White);

            if (address.CanSpin)
            {
                RectangleF rectangleF = new RectangleF(0, 0, pictureWidth - deltaMax, pictureWidth - deltaMax);
                graphics.DrawEllipse(new Pen(Color.DarkBlue, width), rectangleF);
                //graphics.FillPath(new Brush(Color.Red), rectangleF);
            }
            else
            {
                PointF[] xPt = new PointF[4]
                {
                new PointF(deltaMin,deltaMin),
                new PointF(deltaMin,pictureWidth-deltaMax),
                new PointF(pictureWidth-deltaMax,pictureWidth-deltaMax),
                new PointF(pictureWidth-deltaMax,deltaMin)
                };

                graphics.DrawPolygon(new Pen(Color.Red, width), xPt);
            }

            Bitmap_Address = bitmap;
            bitmap.MakeTransparent(Color.White);
            PB_Address.Image = bitmap;
        }

        public void ChangeAddressBackColor()
        {
            if (this.BackColor == Color.White)
                this.BackColor = Color.LightGray;
            else if (this.BackColor == Color.LightGray)
                this.BackColor = Color.Gray;
            else
                this.BackColor = Color.Black;
        }

        public void ResetBackColor()
        {
            this.BackColor = Color.White;
        }
    }
}
