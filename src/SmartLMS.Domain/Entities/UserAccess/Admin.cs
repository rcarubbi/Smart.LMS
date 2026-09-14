namespace SmartLMS.Domain.Entities.UserAccess;

public sealed class Admin : User
{
    public Admin(string name, string email)
        : base(name, email)
    {
    }

    public override UserRole Kind => UserRole.Admin;

    // ORM materialization.
    private Admin()
    {
    }
}
