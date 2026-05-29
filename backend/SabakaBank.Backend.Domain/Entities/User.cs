using SabakaBank.Backend.Domain.Common;

namespace SabakaBank.Backend.Domain.Entities;

public class User : BaseEntity
{
    public string Username     { get; private set; }
    public string Email        { get; private set; }
    public string PasswordHash { get; private set; }
    public string FirstName    { get; private set; }
    public string LastName     { get; private set; }
    public bool   IsActive     { get; private set; }

    private User() { }

    public User(string username, string email, string passwordHash, string firstName, string lastName)
    {
        Username     = username;
        Email        = email;
        PasswordHash = passwordHash;
        FirstName    = firstName;
        LastName     = lastName;
        IsActive     = true;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }
}