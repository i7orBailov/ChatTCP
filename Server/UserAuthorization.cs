using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class UserAuthorization
    {
        public bool successfullyVerified { get; private set; } = false;

        public UserAuthorization(UserInstance userToAuthorize, bool register)
        {
            if (register)
            {
                var userToRegister = new UserRegistration(userToAuthorize);
                register = userToRegister.userAlreadyExists ? false : true;
            }
            else
            {
                var userToLogin = new UserLogging(userToAuthorize);
                register = userToLogin.successfullyVerified ? false : true;
            }
        }
    }
}