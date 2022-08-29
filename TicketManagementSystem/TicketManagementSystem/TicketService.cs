using System;
using System.Diagnostics.Metrics;


namespace TicketManagementSystem 
{
    public enum Priority
    {
        High,
        Medium,
        Low
    }
    public class TicketService
    {

        ITicketRepository ticketRepository;
        UserRepository userRepository;

       public TicketService(ITicketRepository ticketRepository, UserRepository userRepository)
       {
            this.ticketRepository = ticketRepository;
            this.userRepository = userRepository;
        }

        public int CreateTicket(string title, Priority priority, string assignedTo, string desc, DateTime dateTime, bool isPayingCustomer)
        {
            
            if (String.IsNullOrEmpty(title) || String.IsNullOrEmpty(desc))
            {
                throw new InvalidTicketException("Title or description were null");
            }

            User user = userRepository.GetUser(assignedTo) != null ? userRepository.GetUser(assignedTo) : throw new UnknownUserException("User " + assignedTo + " not found"); 
 

            var priorityRaised = false;
            if (dateTime < DateTime.UtcNow - TimeSpan.FromHours(1)) 
            {
                priority = UpdatePriority(priority);
                priorityRaised = true;
                if (priority == Priority.Low)
                {
                    priorityRaised = false;
                }
            }

            if ((title.Contains("Crash") || title.Contains("Important") || title.Contains("Failure"))  && !priorityRaised)
            {
                priority = UpdatePriority(priority);
            }

            double price = 0;
            User accountManager = null;
            if (isPayingCustomer)
            {
                accountManager = userRepository.GetAccountManager(); 
                price = (priority == Priority.High) ? 100 : 50;
            }
  
            var ticket = new Ticket()
            {
                Title = title,
                AssignedUser = user,
                Priority = priority,
                Description = desc,
                CreatedDateTime = dateTime,
                PriceInDollars = price,
                AccountManager = accountManager
            };

            var id = ticketRepository.CreateTicket(ticket);
            return id;
        }
        private static Priority UpdatePriority(Priority priority)
        {
            switch (priority)
            {
                case Priority.Low:
                    return Priority.Medium;

                case Priority.Medium:
                    return Priority.High;

                default:
                    return priority;
            }

        }

        public void AssignTicket(int id, string userName)
        {
            User user = userRepository.GetUser(userName) != null ? userRepository.GetUser(userName) : throw new UnknownUserException("User not found"); ;

            Ticket ticket = ticketRepository.GetTicket(id) != null ? ticketRepository.GetTicket(id) : throw new ApplicationException("No ticket found for id " + id);
            ticket.AssignedUser = user;

            ticketRepository.UpdateTicket(ticket);
        }
    }

  
}
