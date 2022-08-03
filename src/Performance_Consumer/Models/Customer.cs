namespace Performance_Consumer.Models
{
    public class Customer
    {
        public string DocumentNumber { get; set; }
        public string Name { get; set; }
        public string MotherName { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime Created { get; set; }
        public CustomerStatusType Status { get; set; }
        public Address Address { get; set; }
        public IEnumerable<Contact> Contacts { get; set; }
    }

    public enum ContactType
    {
        Email,
        Phone
    }

    public enum CustomerStatusType
    {
        None = 0,
        Simple = 1,
        Partial = 2,
        Complete = 3
    }

    public class Address
    {
        public string Street { get; set; }
        public string Number { get; set; }
        public string Neighborhood { get; set; }
        public string ZipCode { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Complement { get; set; }
    }

    public class Contact
    {
        public string Value { get; set; }
        public ContactType Type { get; set; }
    }
}
