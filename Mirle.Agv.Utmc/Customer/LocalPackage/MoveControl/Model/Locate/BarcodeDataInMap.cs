using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Model
{
    public class BarcodeDataInMap
    {
        public string BarcodeLineID { get; set; } = "";
        public int BarcodeID { get; set; }
        public MapPosition Position { get; set; } = new MapPosition();
        public EnumAGVPositionType Type { get; set; } = EnumAGVPositionType.Normal;

        public BarcodeDataInMap(BarocdeLineMap barcodeLine, int barcodeID)
        {
            BarcodeLineID = barcodeLine.ID;
            BarcodeID = barcodeID;
            Type = barcodeLine.Type;
            Position.X = barcodeLine.Start_Position.X + (barcodeLine.End_Position.X - barcodeLine.Start_Position.X) *
                          (barcodeID - barcodeLine.Start_Number) / (barcodeLine.End_Number - barcodeLine.Start_Number);
            Position.Y = barcodeLine.Start_Position.Y + (barcodeLine.End_Position.Y - barcodeLine.Start_Position.Y) *
                          (barcodeID - barcodeLine.Start_Number) / (barcodeLine.End_Number - barcodeLine.Start_Number);
        }
    }
}
