namespace RateWatch.AlertService.Domain.Entities
{
    public enum AlertCondition
    {
        GreaterThan,
        LessThan
    }

    public class Alert
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string BaseCurrency { get; set; }
        public string TargetCurrency { get; set; }
        public AlertCondition Condition { get; set; }
        public decimal Threshold { get; set; }
        public bool IsTriggered { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
