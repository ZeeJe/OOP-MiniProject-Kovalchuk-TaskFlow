namespace TaskFlow.Domain.Entities;

public class User : BaseEntity
{
    public string Name { get; private set; }
    public string Email { get; private set; }

    public User(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Ім'я не може бути порожнім.");
        Name = name;
        Email = email;
    }
}