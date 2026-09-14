using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using ErrorOr;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Application.Users.CreateUser;

public sealed record CreateUserInput(string Name, string Email, string Password, UserRole Role);

public sealed record CreateUserCommand(CreateUserInput Input) : ICommand<Guid>;

internal sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(c => c.Input.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Input.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.Input.Password).NotEmpty().MinimumLength(8);
        RuleFor(c => c.Input.Role).IsInEnum();
    }
}

internal sealed class CreateUserHandler(
    ISmartLmsDbContext context,
    IPasswordHasher passwordHasher) : ICommandHandler<CreateUserCommand, Guid>
{
    /// <summary>Registry of per-role user constructors (strategy table): adding a role means
    /// adding its user subclass plus one entry here, not editing a dispatch switch.</summary>
    private static readonly IReadOnlyDictionary<UserRole, Func<string, string, User>> Activators =
        new Dictionary<UserRole, Func<string, string, User>>
        {
            [UserRole.Admin] = static (name, email) => new Admin(name, email),
            [UserRole.Teacher] = static (name, email) => new Teacher(name, email),
            [UserRole.Student] = static (name, email) => new Student(name, email),
        };

    public async ValueTask<ErrorOr<Guid>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var emailTaken = await context.Users
            .AnyAsync(u => u.Email == command.Input.Email, cancellationToken);

        if (emailTaken)
        {
            return Error.Conflict("Users.EmailAlreadyExists", $"The email '{command.Input.Email}' is already registered.");
        }

        if (!Activators.TryGetValue(command.Input.Role, out var activate))
        {
            throw new InvalidOperationException("Unknown role.");
        }

        var user = activate(command.Input.Name, command.Input.Email);

        user.UpdatePasswordHash(passwordHasher.Hash(command.Input.Password));
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
