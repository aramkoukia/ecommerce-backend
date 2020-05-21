namespace EcommerceApi.Models
{
    public class ApplicationStepDetail
    {
        public int StepDetailId { get; set; }
        public int StepId { get; set; }
        public string StepDetailTitle { get; set; }
        public string StepDetailDescription { get; set; }
        public bool ThumbnailImagePath { get; set; }
        public int SortOrder { get; set; }
    }
}