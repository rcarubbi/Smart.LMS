using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Application.Abstractions.Messaging;
using ErrorOr;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Application.Users.CreateUser;

public sealed record CreateUserCommand(string Name, string Email, string Password, UserRole Role, bool Active = true)
    : ICommand<Guid>;

internal sealed class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(c => c.Password).NotEmpty().MinimumLength(8);
        RuleFor(c => c.Role).IsInEnum();
    }
}

internal sealed class CreateUserHandler(
    ISmartLmsDbContext context,
    IPasswordHasher passwordHasher) : ICommandHandler<CreateUserCommand, Guid>
{
    public async ValueTask<ErrorOr<Guid>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var emailTaken = await context.Users
            .AnyAsync(u => u.Email == command.Email, cancellationToken);

        if (emailTaken)
        {
            return Error.Conflict("Users.EmailAlreadyExists", $"The email '{command.Email}' is already registered.");
        }

        User user = command.Role switch
        {
            UserRole.Admin => new Admin(command.Name, command.Email),
            UserRole.Teacher => new Teacher(command.Name, command.Email),
            UserRole.Student => new Student(command.Name, command.Email),
            _ => throw new InvalidOperationException("Unknown role."),
        };

        user.UpdatePasswordHash(passwordHasher.Hash(command.Password));
        if (!command.Active)
        {
            user.Deactivate();
        }

        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
