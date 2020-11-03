using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Controller
{
    public class BarcodeReader
    {
        public EnumBarcodeReaderType BarcodeReaderType = EnumBarcodeReaderType.None;
        public bool Connected { get; set; } = false;
        public bool Error { get; set; } = false;
        protected string ipOrComport = "";

        virtual public bool Connect(string ipOrComport, ref string errorMessage)
        {
            return false;
        }

        virtual public bool Disconnect(ref string errorMessage)
        {
            return false;
        }

        virtual public void ResetError()
        {
        }

        virtual public bool ReadBarcode(ref string message, int timeout, ref string errorMessage)
        {
            return false;
        }

        virtual public bool StopReadBarcode(ref string message, ref string errorMessage)
        {
            return false;
        }

        virtual public bool ChangeMode(int mode, ref string errorMessage)
        {
            return false;
        }

        virtual public bool SaveMode(int mode, ref string errorMessage)
        {
            return false;
        }
    }
}
