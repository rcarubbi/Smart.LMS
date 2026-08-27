using SmartLMS.Domain.Entities.Content;

namespace SmartLMS.Domain.Entities.Delivery;

public class ClassDeliveryPlan
{
    public ClassDeliveryPlan(Guid classId, DateTime deliveryDate, DeliveryPlan? deliveryPlan = null)
    {
        ClassId = classId;
        DeliveryDate = deliveryDate;

        if (deliveryPlan is not null)
        {
            AttachDeliveryPlan(deliveryPlan);
        }
    }

    // ORM materialization.
    private ClassDeliveryPlan()
    {
    }

    public long Id { get; private set; }

    public Guid DeliveryPlanId { get; private set; }

    private DeliveryPlan _deliveryPlan = null!;

    public DeliveryPlan DeliveryPlan
    {
        get => _deliveryPlan;
        private set => _deliveryPlan = value;
    }

    public Guid ClassId { get; private set; }

    private Class _class = null!;

    public Class Class
    {
        get => _class;
        private set => _class = value;
    }

    private DateTime _deliveryDate;

    public DateTime DeliveryDate
    {
        get => _deliveryDate;
        private set => _deliveryDate = value;
    }

    public void AttachDeliveryPlan(DeliveryPlan plan)
    {
        DeliveryPlan = plan;
        DeliveryPlanId = plan.Id;
    }
}
