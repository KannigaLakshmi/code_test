using System;
using System.Collections.Generic;
using System.Linq;

namespace TicketManagementSystem
{
    public class UserRepository 
    {
        private readonly List<User> users = new()
        {
            new() { FirstName = "John", LastName = "Smith", UserName = "jsmith" },
            new() { FirstName = "Sarah", LastName = "Berg", UserName = "sberg" }
        };
        public User GetUser(string userName)
        {
            return users.FirstOrDefault(user => user.UserName == userName);
        }

        public User GetAccountManager()
        {
            // Assume this method does not need to change.
            return GetUser("sberg");
        }
    }
}
