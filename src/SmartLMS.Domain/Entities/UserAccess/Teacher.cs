namespace SmartLMS.Domain.Entities.UserAccess;

public sealed class Teacher : User
{
    public Teacher(string name, string email)
        : base(name, email)
    {
    }

    // ORM materialization.
    private Teacher()
    {
    }
}
