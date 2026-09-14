namespace SmartLMS.Domain.Entities.UserAccess;

public sealed class Teacher : User
{
    public Teacher(string name, string email)
        : base(name, email)
    {
    }

    public override UserRole Kind => UserRole.Teacher;

    // ORM materialization.
    private Teacher()
    {
    }
}
