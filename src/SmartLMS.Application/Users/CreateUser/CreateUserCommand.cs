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
    public async ValueTask<ErrorOr<Guid>> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {
        var emailTaken = await context.Users
            .AnyAsync(u => u.Email == command.Input.Email, cancellationToken);

        if (emailTaken)
        {
            return Error.Conflict("Users.EmailAlreadyExists", $"The email '{command.Input.Email}' is already registered.");
        }

        User user = command.Input.Role switch
        {
            UserRole.Admin => new Admin(command.Input.Name, command.Input.Email),
            UserRole.Teacher => new Teacher(command.Input.Name, command.Input.Email),
            UserRole.Student => new Student(command.Input.Name, command.Input.Email),
            _ => throw new InvalidOperationException("Unknown role."),
        };

        user.UpdatePasswordHash(passwordHasher.Hash(command.Input.Password));
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);

        return user.Id;
    }
}
