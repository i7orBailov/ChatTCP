namespace Server
{
    class UsersDateBase
    {
        public int ID { get; private set; }

        public string nickName { get; private set; }

        public string password { get; private set; }

        public UsersDateBase() { }

        public UsersDateBase(string nickName, string password)
        {
            this.nickName = nickName;
            this.password = password;
        }
    }
}