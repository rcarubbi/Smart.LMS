using SmartLMS.Domain.Entities.Content;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.Communication;

public class Comment : Entity
{
    public const int MaxCommentLength = 2000;

    public Comment(string commentText, Guid classId, Guid userId, DateTime dateTime)
    {
        CommentText = commentText;
        ClassId = classId;
        UserId = userId;
        DateTime = dateTime;
    }

    // ORM materialization.
    private Comment()
    {
    }

    private string _commentText = string.Empty;

    public string CommentText
    {
        get => _commentText;
        private set => _commentText = value?.Trim() ?? string.Empty;
    }

    private DateTime _dateTime;

    public DateTime DateTime
    {
        get => _dateTime;
        private set => _dateTime = value;
    }

    public Guid UserId { get; private set; }

    private User _user = null!;

    public User User
    {
        get => _user;
        private set => _user = value;
    }

    public Guid ClassId { get; private set; }

    private Class _class = null!;

    public Class Class
    {
        get => _class;
        private set => _class = value;
    }

    public void SetText(string commentText) => CommentText = commentText;
}
