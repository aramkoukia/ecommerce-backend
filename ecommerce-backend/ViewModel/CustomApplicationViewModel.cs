using System.Collections.Generic;

namespace EcommerceApi.ViewModel
{
    public class CustomApplicationViewModel
    {
        public CustomApplicationViewModel()
        {
            StepDetails = new List<CustomApplicationDetail>();
        }
        public int ApplicationStepId { get; set; }
        public string StepTitle { get; set; }
        public string StepDescription { get; set; }
        public bool IsRangeValue { get; set; }
        public decimal MinValue { get; set; }
        public decimal MaxValue { get; set; }
        public string ValueUnit { get; set; }
        public int SortOrder { get; set; }
        public List<CustomApplicationDetail> StepDetails { get; set; }
    }

    public class CustomApplicationDetail
    {
        public CustomApplicationDetail()
        {
            Tags = new List<CustomApplicationStepDetailTag>();
        }
        public int ApplicationStepDetailId { get; set; }
        public int ApplicationStepId { get; set; }
        public string StepDetailTitle { get; set; }
        public string StepDetailDescription { get; set; }
        public string ThumbnailImagePath { get; set; }
        public int SortOrder { get; set; }
        public List<CustomApplicationStepDetailTag> Tags { get; set; }
    }

    public class CustomApplicationStepDetailTag
    {
        public int ApplicationStepDetailId { get; set; }
        public int TagId { get; set; }
        public string TagName { get; set; }
    }
}
