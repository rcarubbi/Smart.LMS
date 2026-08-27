using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.Communication;

public class Notice : Entity
{
    public const int MaxTextLength = 5000;

    public Notice(string text, DateTime dateTime, Guid? deliveryPlanId = null, Guid? userId = null)
    {
        Text = text;
        DateTime = dateTime;
        DeliveryPlanId = deliveryPlanId;
        UserId = userId;
    }

    // ORM materialization.
    private Notice()
    {
    }

    private string _text = string.Empty;

    /// <summary>Rich text (HTML) content of the notice.</summary>
    public string Text
    {
        get => _text;
        private set => _text = value?.Trim() ?? string.Empty;
    }

    private DateTime _dateTime;

    public DateTime DateTime
    {
        get => _dateTime;
        private set => _dateTime = value;
    }

    public Guid? DeliveryPlanId { get; private set; }

    private DeliveryPlan? _deliveryPlan;

    public DeliveryPlan? DeliveryPlan
    {
        get => _deliveryPlan;
        private set => _deliveryPlan = value;
    }

    /// <summary>Target user for personal notices; null means broadcast to the classroom.</summary>
    public Guid? UserId { get; private set; }

    private User? _user;

    public User? User
    {
        get => _user;
        private set => _user = value;
    }

    public ICollection<UserNotice> Users { get; private set; } = [];

    public void SetText(string text) => Text = text;

    public void AttachDeliveryPlan(DeliveryPlan plan)
    {
        DeliveryPlan = plan;
        DeliveryPlanId = plan.Id;
    }
}
