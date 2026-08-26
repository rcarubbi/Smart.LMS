using SmartLMS.Domain.Entities.Delivery;
using SmartLMS.Domain.Entities.UserAccess;

namespace SmartLMS.Domain.Entities.Communication;

public class Notice : Entity
{
    /// <summary>Rich text (HTML) content of the notice.</summary>
    public string Text { get; set; } = string.Empty;

    public DateTime DateTime { get; set; }

    public Guid? DeliveryPlanId { get; set; }

    public DeliveryPlan? DeliveryPlan { get; set; }

    /// <summary>Target user for personal notices; null means broadcast to the classroom.</summary>
    public Guid? UserId { get; set; }

    public User? User { get; set; }

    public ICollection<UserNotice> Users { get; set; } = [];
}
