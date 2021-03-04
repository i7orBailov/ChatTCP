using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class UserRegistration
    {
        readonly string nickName;

        readonly string password;

        public bool successfullyRegistered { get; private set; } = true;

        public UserRegistration(UserInstance userToRegister)
        {
            nickName = userToRegister.userNickName;
            password = userToRegister.userPassword;
            CheckIfUserAlreadyExists();
            if (successfullyRegistered)
                SaveUserInDateBase();
        }

        void CheckIfUserAlreadyExists()
        {
            using (var dateBase = new UserContextDB())
            {
                foreach (var user in dateBase.users)
                {
                    if (user.nickName == nickName && user.password == password)
                        successfullyRegistered = false;
                }
            }
        }

        void SaveUserInDateBase()
        {
            using (var dateBase = new UserContextDB())
            {
                var currentUser = new UsersDateBase(nickName, password);
                dateBase.users.Add(currentUser);
                dateBase.SaveChanges();
            }
        }
    }
}