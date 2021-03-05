namespace Server
{
    class UserAuthorization
    {
        public bool successfullyVerified { get; private set; }

        public UserAuthorization(UserInstance userToAuthorize, bool register)
        {
            if (register)
            {
                var userToRegister = new UserRegistration(userToAuthorize);
                successfullyVerified = userToRegister.successfullyRegistered ? true : false;
            }
            else
            {
                var userToLogin = new UserLogging(userToAuthorize);
                successfullyVerified = userToLogin.successfullyLogged ? true : false;
            }
        }
    }
}