namespace SmartLMS.DAL.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialModel : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Parameter",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Key = c.String(),
                        Value = c.String(),
                        Active = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Log",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        OldState = c.String(),
                        NewState = c.String(),
                        DateTime = c.DateTime(nullable: false),
                        EntityId = c.Guid(nullable: false),
                        Type = c.String(),
                        User_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.User",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Login = c.String(),
                        Password = c.String(),
                        Email = c.String(),
                        Active = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        Discriminator = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.ClassAccess",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AccessDateTime = c.DateTime(nullable: false),
                        Percentual = c.Int(nullable: false),
                        WatchedSeconds = c.Decimal(nullable: false, precision: 18, scale: 2),
                        Class_Id = c.Guid(nullable: false),
                        User_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Class", t => t.Class_Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.Class_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Class",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DeliveryDays = c.Int(nullable: false),
                        Name = c.String(),
                        Content = c.String(),
                        ContentType = c.Int(nullable: false),
                        Order = c.Int(nullable: false),
                        Active = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        Course_Id = c.Guid(nullable: false),
                        Teacher_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Course", t => t.Course_Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.Teacher_Id)
                .Index(t => t.Course_Id)
                .Index(t => t.Teacher_Id);
            
            CreateTable(
                "dbo.Comment",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        DateTime = c.DateTime(nullable: false),
                        CommentText = c.String(),
                        User_Id = c.Guid(),
                        Class_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.User", t => t.User_Id)
                .ForeignKey("dbo.Class", t => t.Class_Id, cascadeDelete: true)
                .Index(t => t.User_Id)
                .Index(t => t.Class_Id);
            
            CreateTable(
                "dbo.Course",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Order = c.Int(nullable: false),
                        Image = c.String(),
                        Name = c.String(),
                        Active = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        Subject_Id = c.Guid(nullable: false),
                        TeacherInCharge_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Subject", t => t.Subject_Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.TeacherInCharge_Id)
                .Index(t => t.Subject_Id)
                .Index(t => t.TeacherInCharge_Id);
            
            CreateTable(
                "dbo.ClassroomCourse",
                c => new
                    {
                        CourseId = c.Guid(nullable: false),
                        ClassroomId = c.Guid(nullable: false),
                        Order = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.CourseId, t.ClassroomId })
                .ForeignKey("dbo.Classroom", t => t.ClassroomId, cascadeDelete: true)
                .ForeignKey("dbo.Course", t => t.CourseId, cascadeDelete: true)
                .Index(t => t.CourseId)
                .Index(t => t.ClassroomId);
            
            CreateTable(
                "dbo.Classroom",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        Active = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.DeliveryPlan",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        StartDate = c.DateTime(nullable: false),
                        Concluded = c.Boolean(nullable: false),
                        Classroom_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Classroom", t => t.Classroom_Id, cascadeDelete: true)
                .Index(t => t.Classroom_Id);
            
            CreateTable(
                "dbo.ClassDeliveryPlan",
                c => new
                    {
                        ClassId = c.Guid(nullable: false),
                        DeliveryPlanId = c.Long(nullable: false),
                        DeliveryDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.ClassId, t.DeliveryPlanId })
                .ForeignKey("dbo.DeliveryPlan", t => t.DeliveryPlanId, cascadeDelete: true)
                .ForeignKey("dbo.Class", t => t.ClassId, cascadeDelete: true)
                .Index(t => t.ClassId)
                .Index(t => t.DeliveryPlanId);
            
            CreateTable(
                "dbo.Notice",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Text = c.String(),
                        DateTime = c.DateTime(nullable: false),
                        DeliveryPlan_Id = c.Long(),
                        User_Id = c.Guid(),
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
                        UserId = c.Guid(nullable: false),
                        NoticeId = c.Long(nullable: false),
                        VisualizationDateTime = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => new { t.UserId, t.NoticeId })
                .ForeignKey("dbo.Notice", t => t.NoticeId, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.UserId, cascadeDelete: true)
                .Index(t => t.UserId)
                .Index(t => t.NoticeId);
            
            CreateTable(
                "dbo.FileAccess",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        AccessDateTime = c.DateTime(nullable: false),
                        File_Id = c.Guid(nullable: false),
                        User_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.File", t => t.File_Id, cascadeDelete: true)
                .ForeignKey("dbo.User", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.File_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.File",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Name = c.String(),
                        PhysicalPath = c.String(),
                        Active = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        Course_Id = c.Guid(),
                        Class_Id = c.Guid(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Course", t => t.Course_Id)
                .ForeignKey("dbo.Class", t => t.Class_Id, cascadeDelete: true)
                .Index(t => t.Course_Id)
                .Index(t => t.Class_Id);
            
            CreateTable(
                "dbo.Subject",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Order = c.Int(nullable: false),
                        Name = c.String(),
                        Active = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                        KnowledgeArea_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.KnowledgeArea", t => t.KnowledgeArea_Id, cascadeDelete: true)
                .Index(t => t.KnowledgeArea_Id);
            
            CreateTable(
                "dbo.KnowledgeArea",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Order = c.Int(nullable: false),
                        Name = c.String(),
                        Active = c.Boolean(nullable: false),
                        CreatedAt = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.StudentDeliveryPlan",
                c => new
                    {
                        StudentId = c.Guid(nullable: false),
                        DeliveryPlanId = c.Long(nullable: false),
                    })
                .PrimaryKey(t => new { t.StudentId, t.DeliveryPlanId })
                .ForeignKey("dbo.User", t => t.StudentId, cascadeDelete: true)
                .ForeignKey("dbo.DeliveryPlan", t => t.DeliveryPlanId, cascadeDelete: true)
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
            DropIndex("dbo.StudentDeliveryPlan", new[] { "DeliveryPlanId" });
            DropIndex("dbo.StudentDeliveryPlan", new[] { "StudentId" });
            DropIndex("dbo.Subject", new[] { "KnowledgeArea_Id" });
            DropIndex("dbo.File", new[] { "Class_Id" });
            DropIndex("dbo.File", new[] { "Course_Id" });
            DropIndex("dbo.FileAccess", new[] { "User_Id" });
            DropIndex("dbo.FileAccess", new[] { "File_Id" });
            DropIndex("dbo.UserNotice", new[] { "NoticeId" });
            DropIndex("dbo.UserNotice", new[] { "UserId" });
            DropIndex("dbo.Notice", new[] { "User_Id" });
            DropIndex("dbo.Notice", new[] { "DeliveryPlan_Id" });
            DropIndex("dbo.ClassDeliveryPlan", new[] { "DeliveryPlanId" });
            DropIndex("dbo.ClassDeliveryPlan", new[] { "ClassId" });
            DropIndex("dbo.DeliveryPlan", new[] { "Classroom_Id" });
            DropIndex("dbo.ClassroomCourse", new[] { "ClassroomId" });
            DropIndex("dbo.ClassroomCourse", new[] { "CourseId" });
            DropIndex("dbo.Course", new[] { "TeacherInCharge_Id" });
            DropIndex("dbo.Course", new[] { "Subject_Id" });
            DropIndex("dbo.Comment", new[] { "Class_Id" });
            DropIndex("dbo.Comment", new[] { "User_Id" });
            DropIndex("dbo.Class", new[] { "Teacher_Id" });
            DropIndex("dbo.Class", new[] { "Course_Id" });
            DropIndex("dbo.ClassAccess", new[] { "User_Id" });
            DropIndex("dbo.ClassAccess", new[] { "Class_Id" });
            DropIndex("dbo.Log", new[] { "User_Id" });
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
