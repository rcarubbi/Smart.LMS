using System.Data.Entity.Migrations;

namespace SmartLMS.DAL.Migrations
{
    public partial class InitialMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                    "dbo.Parameter",
                    c => new
                    {
                        Id = c.Guid(false),
                        Key = c.String(),
                        Value = c.String(),
                        Active = c.Boolean(false),
                        CreatedAt = c.DateTime(false)
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                    "dbo.Log",
                    c => new
                    {
                        Id = c.Long(false, true),
                        OldState = c.String(),
                        NewState = c.String(),
                        DateTime = c.DateTime(false),
                        EntityId = c.Guid(false),
                        Type = c.String(),
                        User_Id = c.Guid()
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.User_Id);

            CreateTable(
                    "dbo.User",
                    c => new
                    {
                        Id = c.Guid(false),
                        Name = c.String(),
                        Login = c.String(),
                        Password = c.String(),
                        Email = c.String(),
                        Active = c.Boolean(false),
                        CreatedAt = c.DateTime(false),
                        Discriminator = c.String(false, 128)
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                    "dbo.ClassAccess",
                    c => new
                    {
                        Id = c.Long(false, true),
                        AccessDateTime = c.DateTime(false),
                        Percentual = c.Int(false),
                        WatchedSeconds = c.Decimal(false, 18, 2),
                        Class_Id = c.Guid(false),
                        User_Id = c.Guid(false)
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Class", t => t.Class_Id, true)
                .ForeignKey("dbo.User", t => t.User_Id, true)
                .Index(t => t.Class_Id)
                .Index(t => t.User_Id);

            CreateTable(
                    "dbo.Class",
                    c => new
                    {
                        Id = c.Guid(false),
                        DeliveryDays = c.Int(false),
                        Name = c.String(),
                        Content = c.String(),
                        ContentType = c.Int(false),
                        Order = c.Int(false),
                        Active = c.Boolean(false),
                        CreatedAt = c.DateTime(false),
                        Course_Id = c.Guid(false),
                        Teacher_Id = c.Guid()
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Course", t => t.Course_Id, true)
                .ForeignKey("dbo.User", t => t.Teacher_Id)
                .Index(t => t.Course_Id)
                .Index(t => t.Teacher_Id);

            CreateTable(
                    "dbo.Comment",
                    c => new
                    {
                        Id = c.Long(false, true),
                        DateTime = c.DateTime(false),
                        CommentText = c.String(),
                        User_Id = c.Guid(),
                        Class_Id = c.Guid(false)
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .ForeignKey("dbo.Class", t => t.Class_Id, true)
                .Index(t => t.User_Id)
                .Index(t => t.Class_Id);

            CreateTable(
                    "dbo.Course",
                    c => new
                    {
                        Id = c.Guid(false),
                        Order = c.Int(false),
                        Image = c.String(),
                        Name = c.String(),
                        Active = c.Boolean(false),
                        CreatedAt = c.DateTime(false),
                        Subject_Id = c.Guid(false),
                        TeacherInCharge_Id = c.Guid()
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subject", t => t.Subject_Id, true)
                .ForeignKey("dbo.User", t => t.TeacherInCharge_Id)
                .Index(t => t.Subject_Id)
                .Index(t => t.TeacherInCharge_Id);

            CreateTable(
                    "dbo.ClassroomCourse",
                    c => new
                    {
                        CourseId = c.Guid(false),
                        ClassroomId = c.Guid(false),
                        Order = c.Int(false)
                    })
                .PrimaryKey(t => new {t.CourseId, t.ClassroomId})
                .ForeignKey("dbo.Classroom", t => t.ClassroomId, true)
                .ForeignKey("dbo.Course", t => t.CourseId, true)
                .Index(t => t.CourseId)
                .Index(t => t.ClassroomId);

            CreateTable(
                    "dbo.Classroom",
                    c => new
                    {
                        Id = c.Guid(false),
                        Name = c.String(),
                        Active = c.Boolean(false),
                        CreatedAt = c.DateTime(false)
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                    "dbo.DeliveryPlan",
                    c => new
                    {
                        Id = c.Long(false, true),
                        StartDate = c.DateTime(false),
                        Concluded = c.Boolean(false),
                        Classroom_Id = c.Guid(false)
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Classroom", t => t.Classroom_Id, true)
                .Index(t => t.Classroom_Id);

            CreateTable(
                    "dbo.ClassDeliveryPlan",
                    c => new
                    {
                        ClassId = c.Guid(false),
                        DeliveryPlanId = c.Long(false),
                        DeliveryDate = c.DateTime(false)
                    })
                .PrimaryKey(t => new {t.ClassId, t.DeliveryPlanId})
                .ForeignKey("dbo.DeliveryPlan", t => t.DeliveryPlanId, true)
                .ForeignKey("dbo.Class", t => t.ClassId, true)
                .Index(t => t.ClassId)
                .Index(t => t.DeliveryPlanId);

            CreateTable(
                    "dbo.Notice",
                    c => new
                    {
                        Id = c.Long(false, true),
                        Text = c.String(),
                        DateTime = c.DateTime(false),
                        DeliveryPlan_Id = c.Long(),
                        User_Id = c.Guid()
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DeliveryPlan", t => t.DeliveryPlan_Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.DeliveryPlan_Id)
                .Index(t => t.User_Id);

            CreateTable(
                    "dbo.UserNotice",
                    c => new
                    {
                        UserId = c.Guid(false),
                        NoticeId = c.Long(false),
                        VisualizationDateTime = c.DateTime(false)
                    })
                .PrimaryKey(t => new {t.UserId, t.NoticeId})
                .ForeignKey("dbo.Notice", t => t.NoticeId, true)
                .ForeignKey("dbo.User", t => t.UserId, true)
                .Index(t => t.UserId)
                .Index(t => t.NoticeId);

            CreateTable(
                    "dbo.FileAccess",
                    c => new
                    {
                        Id = c.Long(false, true),
                        AccessDateTime = c.DateTime(false),
                        File_Id = c.Guid(false),
                        User_Id = c.Guid(false)
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.File", t => t.File_Id, true)
                .ForeignKey("dbo.User", t => t.User_Id, true)
                .Index(t => t.File_Id)
                .Index(t => t.User_Id);

            CreateTable(
                    "dbo.File",
                    c => new
                    {
                        Id = c.Guid(false),
                        Name = c.String(),
                        PhysicalPath = c.String(),
                        Active = c.Boolean(false),
                        CreatedAt = c.DateTime(false),
                        Course_Id = c.Guid(),
                        Class_Id = c.Guid()
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Course", t => t.Course_Id)
                .ForeignKey("dbo.Class", t => t.Class_Id, true)
                .Index(t => t.Course_Id)
                .Index(t => t.Class_Id);

            CreateTable(
                    "dbo.Subject",
                    c => new
                    {
                        Id = c.Guid(false),
                        Order = c.Int(false),
                        Name = c.String(),
                        Active = c.Boolean(false),
                        CreatedAt = c.DateTime(false),
                        KnowledgeArea_Id = c.Guid(false)
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.KnowledgeArea", t => t.KnowledgeArea_Id, true)
                .Index(t => t.KnowledgeArea_Id);

            CreateTable(
                    "dbo.KnowledgeArea",
                    c => new
                    {
                        Id = c.Guid(false),
                        Order = c.Int(false),
                        Name = c.String(),
                        Active = c.Boolean(false),
                        CreatedAt = c.DateTime(false)
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                    "dbo.StudentDeliveryPlan",
                    c => new
                    {
                        StudentId = c.Guid(false),
                        DeliveryPlanId = c.Long(false)
                    })
                .PrimaryKey(t => new {t.StudentId, t.DeliveryPlanId})
                .ForeignKey("dbo.User", t => t.StudentId, true)
                .ForeignKey("dbo.DeliveryPlan", t => t.DeliveryPlanId, true)
                .Index(t => t.StudentId)
                .Index(t => t.DeliveryPlanId);
        }

        public override void Down()
        {
            DropForeignKey("dbo.Log", "User_Id", "dbo.User");
            DropForeignKey("dbo.FileAccess", "User_Id", "dbo.User");
            DropForeignKey("dbo.ClassAccess", "User_Id", "dbo.User");
            DropForeignKey("dbo.Class", "Teacher_Id", "dbo.User");
            DropForeignKey("dbo.File", "Class_Id", "dbo.Class");
            DropForeignKey("dbo.ClassDeliveryPlan", "ClassId", "dbo.Class");
            DropForeignKey("dbo.Course", "TeacherInCharge_Id", "dbo.User");
            DropForeignKey("dbo.Subject", "KnowledgeArea_Id", "dbo.KnowledgeArea");
            DropForeignKey("dbo.Course", "Subject_Id", "dbo.Subject");
            DropForeignKey("dbo.File", "Course_Id", "dbo.Course");
            DropForeignKey("dbo.ClassroomCourse", "CourseId", "dbo.Course");
            DropForeignKey("dbo.ClassroomCourse", "ClassroomId", "dbo.Classroom");
            DropForeignKey("dbo.DeliveryPlan", "Classroom_Id", "dbo.Classroom");
            DropForeignKey("dbo.FileAccess", "File_Id", "dbo.File");
            DropForeignKey("dbo.StudentDeliveryPlan", "DeliveryPlanId", "dbo.DeliveryPlan");
            DropForeignKey("dbo.StudentDeliveryPlan", "StudentId", "dbo.User");
            DropForeignKey("dbo.UserNotice", "UserId", "dbo.User");
            DropForeignKey("dbo.UserNotice", "NoticeId", "dbo.Notice");
            DropForeignKey("dbo.Notice", "User_Id", "dbo.User");
            DropForeignKey("dbo.Notice", "DeliveryPlan_Id", "dbo.DeliveryPlan");
            DropForeignKey("dbo.ClassDeliveryPlan", "DeliveryPlanId", "dbo.DeliveryPlan");
            DropForeignKey("dbo.Class", "Course_Id", "dbo.Course");
            DropForeignKey("dbo.Comment", "Class_Id", "dbo.Class");
            DropForeignKey("dbo.Comment", "User_Id", "dbo.User");
            DropForeignKey("dbo.ClassAccess", "Class_Id", "dbo.Class");
            DropIndex("dbo.StudentDeliveryPlan", new[] {"DeliveryPlanId"});
            DropIndex("dbo.StudentDeliveryPlan", new[] {"StudentId"});
            DropIndex("dbo.Subject", new[] {"KnowledgeArea_Id"});
            DropIndex("dbo.File", new[] {"Class_Id"});
            DropIndex("dbo.File", new[] {"Course_Id"});
            DropIndex("dbo.FileAccess", new[] {"User_Id"});
            DropIndex("dbo.FileAccess", new[] {"File_Id"});
            DropIndex("dbo.UserNotice", new[] {"NoticeId"});
            DropIndex("dbo.UserNotice", new[] {"UserId"});
            DropIndex("dbo.Notice", new[] {"User_Id"});
            DropIndex("dbo.Notice", new[] {"DeliveryPlan_Id"});
            DropIndex("dbo.ClassDeliveryPlan", new[] {"DeliveryPlanId"});
            DropIndex("dbo.ClassDeliveryPlan", new[] {"ClassId"});
            DropIndex("dbo.DeliveryPlan", new[] {"Classroom_Id"});
            DropIndex("dbo.ClassroomCourse", new[] {"ClassroomId"});
            DropIndex("dbo.ClassroomCourse", new[] {"CourseId"});
            DropIndex("dbo.Course", new[] {"TeacherInCharge_Id"});
            DropIndex("dbo.Course", new[] {"Subject_Id"});
            DropIndex("dbo.Comment", new[] {"Class_Id"});
            DropIndex("dbo.Comment", new[] {"User_Id"});
            DropIndex("dbo.Class", new[] {"Teacher_Id"});
            DropIndex("dbo.Class", new[] {"Course_Id"});
            DropIndex("dbo.ClassAccess", new[] {"User_Id"});
            DropIndex("dbo.ClassAccess", new[] {"Class_Id"});
            DropIndex("dbo.Log", new[] {"User_Id"});
            DropTable("dbo.StudentDeliveryPlan");
            DropTable("dbo.KnowledgeArea");
            DropTable("dbo.Subject");
            DropTable("dbo.File");
            DropTable("dbo.FileAccess");
            DropTable("dbo.UserNotice");
            DropTable("dbo.Notice");
            DropTable("dbo.ClassDeliveryPlan");
            DropTable("dbo.DeliveryPlan");
            DropTable("dbo.Classroom");
            DropTable("dbo.ClassroomCourse");
            DropTable("dbo.Course");
            DropTable("dbo.Comment");
            DropTable("dbo.Class");
            DropTable("dbo.ClassAccess");
            DropTable("dbo.User");
            DropTable("dbo.Log");
            DropTable("dbo.Parameter");
        }
    }
}