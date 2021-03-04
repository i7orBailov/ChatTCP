using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class UserLogging
    {
        readonly string nickName;

        readonly string password;

        public bool successfullyLogged { get; private set; }

        public UserLogging(UserInstance userToLogin)
        {
            nickName = userToLogin.userNickName;
            password = userToLogin.userPassword;
            VerifyUser();
        }

        void VerifyUser()
        {
            using (var dateBase = new UserContextDB())
            {
                foreach (var user in dateBase.users)
                {
                    if (user.nickName == nickName && user.password == password)
                        successfullyLogged = true;
                }
            }
        }
    }
}