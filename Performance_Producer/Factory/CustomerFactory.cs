using Bankly.Sdk.Kafka.Notifications;
using Performance_Producer.Models;
using Performance_Producer.Notification;

namespace Performance_Producer.Factory
{
    public static class CustomerFactory
    {
        private static int _counter = 0;
        public static Customer GetCustomer()
        {
            int refer = _counter++;
            return new Customer
            {
                Name = $"Test Name {refer}",
                BirthDate = DateTime.Now,
                Created = DateTime.Now,
                MotherName = $"Test Mother {refer}",
                Status = refer % 2 == 0 ? CustomerStatusType.Simple : CustomerStatusType.Complete,
                DocumentNumber = $"DocumentNumber {refer}",
                Address = new Address
                {
                    City = $"City {refer}",
                    ZipCode = $"ZipCode {refer}",
                    Complement = $"Complement {refer}",
                    Neighborhood = $"Neighborhood {refer}",
                    Number = $"Number {refer}",
                    State = $"State {refer}",
                    Street = $"Street {refer}"
                },
                Contacts = new List<Contact> { new Contact { Type = ContactType.Email, Value = $"Value {refer}" } }
            };
        }


        public static CustomerNotification GetCustomerNotification()
        {
            int refer = _counter++;
            var notification = new CustomerNotification();
            var customer = GetCustomer();

            notification.Name = refer % 2 == 0 ? "CUSTOMER_WAS_CREATED" : "CUSTOMER_WAS_UPDATED";
            notification.Timestamp = DateTime.Now;
            notification.Data = customer;
            notification.EntityId = Guid.NewGuid().ToString();
            notification.Context = Context.Account;
            notification.Metadata = new Dictionary<string, object>();
            notification.Metadata.Add("Test", "test");
            notification.CompanyKey = "BANKLY";

            return notification;
        }
    }
}
