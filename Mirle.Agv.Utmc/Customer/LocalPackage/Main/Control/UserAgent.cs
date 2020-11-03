using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Mirle.Agv.INX.Controller.Tools;
using Mirle.Agv.INX.Model;

namespace Mirle.Agv.INX.Controller
{
    public class UserAgent
    {
        private LocalData localData = LocalData.Instance;
        
        public UserAgent()
        {
        }

        private void LoginLevelChange(EnumLoginLevel newLevel)
        {
            localData.LoginLevel = newLevel;
        }

        public void Login(string account, string password)
        {
            try
            {
                if (String.Compare(account, EnumLoginLevel.Engineer.ToString(), true) == 0)
                {

                }
                else if (String.Compare(account, EnumLoginLevel.Admin.ToString(), true) == 0)
                {

                }
                else if (String.Compare(account, "ChiaWei", true) == 0)
                {
                    if (password == "22099478")
                        LoginLevelChange(EnumLoginLevel.MirleAdmin);
                }
            }
            catch
            {
            }
        }

        public void Logout()
        {
            localData.LoginLevel = EnumLoginLevel.User;
        }
    }
}
