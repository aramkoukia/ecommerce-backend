namespace EcommerceApi.Models
{
    public class ApplicationStep
    {
        public int ApplicationStepId { get; set; }
        public string StepTitle { get; set; }
        public string StepDescription { get; set; }
        public bool IsRangeValue { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public string ValueUnit { get; set; }
        public int SortOrder { get; set; }
    }
}