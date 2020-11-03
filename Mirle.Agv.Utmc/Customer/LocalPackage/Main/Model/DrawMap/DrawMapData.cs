using Mirle.Agv.INX.View;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mirle.Agv.INX.Model
{
    public class DrawMapData
    {
        public Bitmap ObjectAndSection { get; set; }
        public Graphics Graphics_ObjectAndSection { get; set; }

        public PictureBox PB_AllAddress { get; set; } = new PictureBox();
        public Bitmap AllAddressBitmap { get; set; }

        public Dictionary<string, AddressPicture> AllAddressPicture { get; set; } = new Dictionary<string, AddressPicture>();

        public MapPosition Max { get; set; }
        public MapPosition Min { get; set; }

        private double mapScale;
        public Size MapSize;

        public Dictionary<string, DrawMapSection> SectionData { get; set; } = new Dictionary<string, DrawMapSection>();

        public Dictionary<EnumSectionAction, Pen> AllPen { get; set; } = new Dictionary<EnumSectionAction, Pen>();

        private int penLength = 3;

        public DrawMapData()
        {
            AllPen.Add(EnumSectionAction.Idle, new Pen(Color.Blue, penLength));
            AllPen.Add(EnumSectionAction.NotGetReserve, new Pen(Color.Red, penLength));
            AllPen.Add(EnumSectionAction.GetReserve, new Pen(Color.LightGreen, penLength));
        }

        public void UpdateMaxAndMin(MapPosition position)
        {
            if (position == null)
                return;

            if (Max == null || Min == null)
            {
                Max = new MapPosition(position.X, position.Y);
                Min = new MapPosition(position.X, position.Y);
            }
            else
            {
                if (position.X > Max.X)
                    Max.X = position.X;
                else if (position.X < Min.X)
                    Min.X = position.X;

                if (position.Y > Max.Y)
                    Max.Y = position.Y;
                else if (position.Y < Min.Y)
                    Min.Y = position.Y;
            }
        }

        public void SetMapBorderLength(double mapBorderLength, double mapScale)
        {
            this.mapScale = mapScale;

            Min.X -= mapBorderLength;
            Min.Y -= mapBorderLength;

            Max.X += mapBorderLength;
            Max.Y += mapBorderLength;

            MapSize = new Size((int)((Max.X - Min.X) * mapScale), (int)((Max.Y - Min.Y) * mapScale));
        }

        public float TransferX(double x)
        {
            return (float)((x - Min.X) * mapScale);
        }

        public float TransferY(double y)
        {
            return (float)((y - Min.Y) * mapScale);
        }

        public void SetSectionData(MapSection section)
        {
            if (!SectionData.ContainsKey(section.Id))
            {
                DrawMapSection drawMapSection = new DrawMapSection();
                drawMapSection.X1 = TransferX(section.FromAddress.AGVPosition.Position.X);
                drawMapSection.Y1 = TransferY(section.FromAddress.AGVPosition.Position.Y);
                drawMapSection.X2 = TransferX(section.ToAddress.AGVPosition.Position.X);
                drawMapSection.Y2 = TransferY(section.ToAddress.AGVPosition.Position.Y);
                SectionData.Add(section.Id, drawMapSection);
            }
        }
    }
}
