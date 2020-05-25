﻿namespace EcommerceApi.Models
{
    public class ApplicationStepDetail
    {
        public int ApplicationStepDetailId { get; set; }
        public int ApplicationStepId { get; set; }
        public string StepDetailTitle { get; set; }
        public string StepDetailDescription { get; set; }
        public bool ThumbnailImagePath { get; set; }
        public int SortOrder { get; set; }
    }
}