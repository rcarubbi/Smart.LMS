using Microsoft.EntityFrameworkCore;
using SmartLMS.Application.Abstractions;
using SmartLMS.Domain.Entities;
using SmartLMS.Domain.Entities.Assessments;
using SmartLMS.Domain.Entities.Communication;
using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.History;
using SmartLMS.Domain.Entities.Integration;
using SmartLMS.Domain.Entities.UserAccess;
using FileAccess = SmartLMS.Domain.Entities.History.FileAccess;

namespace SmartLMS.DataAccess;

public class SmartLmsDbContext(DbContextOptions<SmartLmsDbContext> options) : DbContext(options), ISmartLmsDbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Teacher> Teachers => Set<Teacher>();
    public DbSet<Admin> Admins => Set<Admin>();

    public DbSet<KnowledgeArea> KnowledgeAreas => Set<KnowledgeArea>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Class> Classes => Set<Class>();
    public DbSet<SupportFile> Files => Set<SupportFile>();

    public DbSet<Classroom> Classrooms => Set<Classroom>();
    public DbSet<ClassroomCourse> ClassroomCourses => Set<ClassroomCourse>();
    public DbSet<DeliveryPlan> DeliveryPlans => Set<DeliveryPlan>();
    public DbSet<ClassDeliveryPlan> ClassDeliveryPlans => Set<ClassDeliveryPlan>();

    public DbSet<Notice> Notices => Set<Notice>();
    public DbSet<UserNotice> UserNotices => Set<UserNotice>();
    public DbSet<Comment> Comments => Set<Comment>();

    public DbSet<Assessment> Assessments => Set<Assessment>();
    public DbSet<Question> Questions => Set<Question>();
    public DbSet<QuestionOption> QuestionOptions => Set<QuestionOption>();

    public DbSet<ClassAccess> ClassAccesses => Set<ClassAccess>();
    public DbSet<FileAccess> FileAccesses => Set<FileAccess>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Parameter> Parameters => Set<Parameter>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SmartLmsDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
