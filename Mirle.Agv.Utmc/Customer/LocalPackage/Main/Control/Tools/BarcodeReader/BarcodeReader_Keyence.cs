using Keyence.AutoID.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Controller
{
    public class BarcodeReader_Keyence : BarcodeReader
    {
        private ReaderAccessor reader = null;
        private bool triggering = false;

        override public bool Connect(string ip, ref string errorMessage)
        {
            try
            {
                if (Connected)
                {
                    if (!Disconnect(ref errorMessage))
                        return false;
                }

                reader = new ReaderAccessor(ip);

                if (reader.Connect())
                {
                    ipOrComport = ip;
                    Connected = true;
                    return true;
                }
                else
                {
                    errorMessage = String.Concat("連線失敗, IP : ", ip, ", ErrorCode : ", reader.LastErrorInfo.ToString());
                    return false;
                }

            }
            catch (Exception ex)
            {
                errorMessage = String.Concat("連線失敗 : ", ex.ToString());
                return false;
            }
        }

        override public bool Disconnect(ref string errorMessage)
        {
            try
            {
                if (!Connected)
                    return true;
                else
                {
                    string message = "";

                    if (triggering)
                        StopReadBarcode(ref message, ref errorMessage);

                    reader.Dispose();
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = String.Concat("斷線失敗 : ", ex.ToString());
                return false;
            }
        }

        public override void ResetError()
        {
            string errorMessage = "";

            if (Error)
            {
                if (Connect(ipOrComport, ref errorMessage))
                    Error = false;
            }
        }

        override public bool ReadBarcode(ref string message, int timeout, ref string errorMessage)
        {
            try
            {
                if (!Connected)
                {
                    errorMessage = "Keyence尚未連線";
                    return false;
                }
                else if (Error)
                {
                    errorMessage = "Keyence Error中";
                    return false;
                }
                else
                {
                    triggering = true;
                    message = reader.ExecCommand("LON", timeout);

                    if (message == null || message == "")
                    {
                        if (reader.LastErrorInfo == ErrorCode.Closed)
                            Error = true;

                        errorMessage = String.Concat("讀取失敗, ErrorCode : ", reader.LastErrorInfo.ToString());
                        return false;
                    }
                    else
                        return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = String.Concat("LON Exception : ", ex.ToString());
                return false;
            }
        }

        override public bool StopReadBarcode(ref string message, ref string errorMessage)
        {
            try
            {
                if (!Connected)
                {
                    errorMessage = "Keyence尚未連線";
                    return false;
                }
                else if (!triggering)
                    return true;
                else
                {
                    message = reader.ExecCommand("LOFF");
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = String.Concat("LOff Exception : ", ex.ToString());
                return false;
            }
        }

        override public bool ChangeMode(int loadNum, ref string errorMessage)
        {
            try
            {
                if (!Connected)
                {
                    errorMessage = "Keyence尚未連線";
                    return false;
                }
                else
                {
                    reader.ExecCommand(String.Concat("BLOAD,", loadNum.ToString()));
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = String.Concat("ChangeMode Exception : ", ex.ToString());
                return false;
            }
        }

        override public bool SaveMode(int loadNum, ref string errorMessage)
        {
            try
            {
                if (!Connected)
                {
                    errorMessage = "Keyence尚未連線";
                    return false;
                }
                else
                {
                    reader.ExecCommand(String.Concat("BSAVE,", loadNum.ToString()));
                    return true;
                }
            }
            catch (Exception ex)
            {
                errorMessage = String.Concat("Save Mode Exception : ", ex.ToString());
                return false;
            }
        }
    }
}
