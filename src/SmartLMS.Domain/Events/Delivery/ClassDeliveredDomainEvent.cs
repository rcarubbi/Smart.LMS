using SmartLMS.Domain.Events;

namespace SmartLMS.Domain.Events.Delivery;

/// <summary>
/// Raised when a delivery plan releases a class to its enrolled students.
/// Carries everything the integration layer needs to notify students by e-mail.
/// </summary>
public sealed record ClassDeliveredDomainEvent(
    Guid PlanId,
    Guid ClassId,
    string ClassName,
    Guid CourseId,
    string CourseName,
    string SubjectName,
    string ClassroomName,
    IReadOnlyList<ClassDeliveredRecipient> Recipients,
    DateTime OccurredOn) : IDomainEvent;

public sealed record ClassDeliveredRecipient(Guid StudentId, string Name, string Email);
