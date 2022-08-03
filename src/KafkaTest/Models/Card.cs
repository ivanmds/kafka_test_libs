namespace KafkaTest.Models
{
    public class Card
    {
        public string Proxy { get; set; }
        public string ActivateCode { get; set; }
        public string Alias { get; set; }
        public string Name { get; set; }
        public Program Program { get; set; }
        public string LastFourDigits { get; set; }
        public string CardType { get; set; }
        public string Status { get; set; }
        public string Function { get; set; }
        public bool AllowContactless { get; set; }
        public AddressCArd Address { get; set; }
        public Holder Holder { get; set; }
    }

    public class Account
    {
        public string Branch { get; set; }
        public string Number { get; set; }
    }

    public class AddressCArd
    {
        public string ZipCode { get; set; }
        public string AddressLine { get; set; }
        public string Number { get; set; }
        public string Neighborhood { get; set; }
        public string Complement { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
    }

    public class Document
    {
        public string Value { get; set; }
        public string Type { get; set; }
    }

    public class Holder
    {
        public Document Document { get; set; }
        public string Name { get; set; }
        public Account Account { get; set; }
    }

    public class Program
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
    }
}
