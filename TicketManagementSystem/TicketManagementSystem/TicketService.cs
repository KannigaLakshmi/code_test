using System;
using System.Configuration;
using System.IO;
using System.Text.Json;

namespace TicketManagementSystem
{
    public class TicketService
    {
        ITicketRepository ticketRepository;

        public TicketService(ITicketRepository ticketRepository)
        {
            this.ticketRepository = ticketRepository;
        }

        public int CreateTicket(string t, Priority p, string assignedTo, string desc, DateTime d, bool isPayingCustomer)
        {
            // Check if t or desc are null or if they are invalid and throw exception
            if (t == null || desc == null || t == "" || desc == "")
            {
                throw new InvalidTicketException("Title or description were null");
            }

            User user = null;
            var ur = new UserRepository();
            if (assignedTo != null)
            {
                user = ur.GetUser(assignedTo);
            }

            if (user == null)
            {
                throw new UnknownUserException("User " + assignedTo + " not found");
            }

            var priorityyRaised = false;
            if (d < DateTime.UtcNow - TimeSpan.FromHours(1))
            {
                if (p == Priority.Low)
                {
                    p = Priority.Medium;
                    priorityyRaised = true;
                }
                else if (p == Priority.Medium)
                {
                    p = Priority.High;
                    priorityyRaised = true;
                }
            }

            // If the title contains some special worrds and the priority has not yet been raised, raise it here.
            if ((t.Contains("Crash") || t.Contains("Important") || t.Contains("Failure")) && !priorityyRaised)
            {
                if (p == Priority.Low)
                {
                    p = Priority.Medium;
                }
                else if (p == Priority.Medium)
                {
                    p = Priority.High;
                }
            }

            double price = 0;
            User accountManager = null;
            if (isPayingCustomer)
            {
                // Only paid customers have an account manager.
                accountManager = new UserRepository().GetAccountManager();
                if (p == Priority.High)
                {
                    price = 100;
                }
                else
                {
                    price = 50;
                }
            }

            // Create the tickket
            var ticket = new Ticket()
            {
                Title = t,
                AssignedUser = user,
                Priority = p,
                Description = desc,
                Created = d,
                PriceDollars = price,
                AccountManager = accountManager
            };

            var id = ticketRepository.CreateTicket(ticket);

            // Return the id
            return id;
        }

        public void AssignTicket(int id, string username)
        {
            User user = null;
            var ur = new UserRepository();
            if (username != null)
            {
                user = ur.GetUser(username);
            }

            if (user == null)
            {
                throw new UnknownUserException("User not found");
            }

            var ticket = ticketRepository.GetTicket(id);

            if (ticket == null)
            {
                throw new ApplicationException("No ticket found for id " + id);
            }

            ticket.AssignedUser = user;

            ticketRepository.UpdateTicket(ticket);
        }

        private void WriteTicketToFile(Ticket ticket)
        {
            var ticketJson = JsonSerializer.Serialize(ticket);
            File.WriteAllText(Path.Combine(Path.GetTempPath(), $"ticket_{ticket.Id}.json"), ticketJson);
        }
    }

    public enum Priority
    {
        High,
        Medium,
        Low
    }
}
