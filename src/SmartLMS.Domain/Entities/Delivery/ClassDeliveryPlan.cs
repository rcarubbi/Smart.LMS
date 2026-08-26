using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.Domain.Entities.Delivery;

public class ClassDeliveryPlan
{
    public long Id { get; private set; }

    public long DeliveryPlanId { get; set; }

    public DeliveryPlan DeliveryPlan { get; set; } = null!;

    public Guid ClassId { get; set; }

    public Class Class { get; set; } = null!;

    public DateTime DeliveryDate { get; set; }
}
