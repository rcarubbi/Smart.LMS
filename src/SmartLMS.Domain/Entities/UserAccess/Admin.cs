namespace SmartLMS.Domain.Entities.UserAccess;

public sealed class Admin : User
{
    public Admin(string name, string email)
        : base(name, email)
    {
    }

    // ORM materialization.
    private Admin()
    {
    }
}
