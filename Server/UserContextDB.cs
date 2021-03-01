using System.Data.Entity;

namespace Server
{
    class UserContextDB : DbContext
    {
        public UserContextDB() : base("DbConnection") {}

        public DbSet<UsersDateBase> users { get; set; }
    }
}
