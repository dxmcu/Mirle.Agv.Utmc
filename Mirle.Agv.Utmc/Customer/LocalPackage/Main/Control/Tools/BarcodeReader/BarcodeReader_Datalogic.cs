
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Mirle.Agv.INX.Controller
{
    public class BarcodeReader_Datalogic : BarcodeReader
    {
        private Socket socket = null;

        private string startByte = "\x2";
        private string endByte = "\x3";
        private byte[] ASCIIbytes = Encoding.ASCII.GetBytes("LON");
        private string noDataString = "ReadFail";
        private int errorCount = 0;

        private bool ConnectSocket(string ip, ref string errorMessage)
        {
            try
            {
                socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                this.socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, 500);

                string[] split = Regex.Split(ip, ":", RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(500));

                if (split == null || split.Length != 2)
                {
                    errorMessage = String.Concat("IP格式錯誤(IP:port) : ", ip);
                    return false;
                }

                this.socket.Connect(IPAddress.Parse(split[0]), Int32.Parse(split[1]));

                return true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.ToString();
                return false;
            }
        }

        override public bool Connect(string ip, ref string errorMessage)
        {
            try
            {
                if (Connected)
                {
                    if (!Disconnect(ref errorMessage))
                        return false;
                }

                if (ConnectSocket(ip, ref errorMessage))
                {
                    ipOrComport = ip;
                    Connected = true;
                    return true;
                }
                else
                {
                    errorMessage = String.Concat("連線失敗, IP : ", ip, ", errorMessage : ", errorMessage);
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
                    socket.Disconnect(true);
                    socket.Dispose();

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

        private string GetFeedbackString(byte[] receiveBytes)
        {
            if (receiveBytes == null)
                return "";

            int start = -1;
            int end = -1;

            for (int i = 0; i < receiveBytes.Length; i++)
            {
                if (receiveBytes[i] == startByte[0])
                    start = i;
                else if (receiveBytes[i] == endByte[0])
                    end = i;

                if (start != -1 && end != -1)
                    break;
            }

            if (start != -1 && end != -1 && end > start)
                return System.Text.Encoding.ASCII.GetString(receiveBytes, start + 1, end - 1);
            else
                return "";
        }

        private bool TriggerBarcode(int timeout, ref string message, ref string errorMessage)
        {
            try
            {
                byte[] receiveBytes = new byte[256];
                socket.Send(ASCIIbytes, 0, ASCIIbytes.GetLength(0), SocketFlags.None);
                socket.ReceiveTimeout = timeout;
                socket.Receive(receiveBytes, 0, 256, SocketFlags.None);

                message = GetFeedbackString(receiveBytes);
                errorCount = 0;
                return message != noDataString;

            }
            catch (Exception ex)
            {
                errorCount++;

                if (errorCount > 10)
                    Error = true;

                errorMessage = ex.ToString();
                return false;
            }
        }

        override public bool ReadBarcode(ref string message, int timeout, ref string errorMessage)
        {
            try
            {
                if (!Connected)
                {
                    errorMessage = "Datalogic尚未連線";
                    return false;
                }
                else if (Error)
                {
                    errorMessage = "Datalogic Error中";
                    return false;
                }
                else
                {
                    if (TriggerBarcode(timeout, ref message, ref errorMessage))
                        return true;
                    else
                        return false;
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
                return true;
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
                errorMessage = "datalogic無模式切換";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = String.Concat("LON Exception : ", ex.ToString());
                return false;
            }
        }

        override public bool SaveMode(int loadNum, ref string errorMessage)
        {
            try
            {
                errorMessage = "datalogic無SaveMode";
                return false;
            }
            catch (Exception ex)
            {
                errorMessage = String.Concat("LON Exception : ", ex.ToString());
                return false;
            }
        }
    }
}