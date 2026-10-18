using Microsoft.EntityFrameworkCore;
using SmartLMS.Domain.Entities;
using FileAccess = SmartLMS.Domain.Entities.History.FileAccess;
using SmartLMS.Domain.Entities.Assessments;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.History;
using SmartLMS.Domain.Entities.Integration;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Application.Abstractions;

/// <summary>
/// Persistence contract consumed by use cases; implemented by EF Core in DataAccess.
/// </summary>
public interface ISmartLmsDbContext
{
    DbSet<User> Users { get; }
    DbSet<Student> Students { get; }
    DbSet<Teacher> Teachers { get; }
    DbSet<Admin> Admins { get; }

    DbSet<KnowledgeArea> KnowledgeAreas { get; }
    DbSet<Subject> Subjects { get; }
    DbSet<Course> Courses { get; }
    DbSet<Class> Classes { get; }
    DbSet<SupportFile> Files { get; }

    DbSet<Classroom> Classrooms { get; }
    DbSet<ClassroomCourse> ClassroomCourses { get; }
    DbSet<DeliveryPlan> DeliveryPlans { get; }
    DbSet<ClassDeliveryPlan> ClassDeliveryPlans { get; }

    DbSet<Notice> Notices { get; }
    DbSet<UserNotice> UserNotices { get; }
    DbSet<Comment> Comments { get; }

    DbSet<Assessment> Assessments { get; }
    DbSet<Question> Questions { get; }
    DbSet<QuestionOption> QuestionOptions { get; }

    DbSet<ClassAccess> ClassAccesses { get; }
    DbSet<FileAccess> FileAccesses { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Parameter> Parameters { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<PasswordResetToken> PasswordResetTokens { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
