using System;
using Moq;
using NUnit.Framework;

namespace TicketManagementSystem.Test
{
    public class Tests
    {
        private TicketService ticketService;
        private Mock<ITicketRepository> ticketRepositoryMock;
        private Mock<UserRepository> userRepositoryMock;


        [SetUp]
        public void Setup()
        {
            ticketRepositoryMock = new Mock<ITicketRepository>();
            userRepositoryMock = new Mock<UserRepository>();
            ticketService = new TicketService(ticketRepositoryMock.Object, userRepositoryMock.Object);
        }

        [Test]
        public void ShallThrowExceptionIfTitleIsNull()
        {
            Assert.That(() => ticketService.CreateTicket(null, Priority.High, "jim", "high prio ticket", DateTime.Now, false), Throws.InstanceOf<InvalidTicketException>().With.Message.EqualTo("Title or description were null"));
        }

        [Test]
        public void ShallCreateTicket()
        {
            const string title = "MyTicket";
            const Priority prio = Priority.High;
            const string assignedTo = "jsmith";
            const string description = "This is a high ticket"; 
            DateTime when = DateTime.Now;

            ticketService.CreateTicket(title, prio, assignedTo, description, when, false);

            ticketRepositoryMock.Verify(a => a.CreateTicket(It.Is<Ticket>(t =>
                t.Title == title && 
                t.Priority == Priority.High && 
                t.Description == description &&
                t.AssignedUser.UserName == assignedTo && 
                t.CreatedDateTime == when)));
        }
    }
}