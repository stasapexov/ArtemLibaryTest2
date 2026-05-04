namespace ArtemLibaryTest.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Login { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public double Money { get; set; }
        public byte[] Img { get; set; }

        public Users(int id, string name, string password, string login, string phone, string status, double money, byte[]? img)
        {
            Id = id;
            Name = name;
            Password = password;
            Login = login;
            Phone = phone;
            Status = status;
            Money = money;
            Img = img ?? [];
        }
    }
}
