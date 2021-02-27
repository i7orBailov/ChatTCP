using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server
{
    class UserContextDB : DbContext
    {
        public UserContextDB() : base("DbConnection") {}

        public DbSet<UserDB> users { get; set; }
    }
}
