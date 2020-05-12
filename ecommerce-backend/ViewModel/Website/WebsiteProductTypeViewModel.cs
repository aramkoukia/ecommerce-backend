using System;

namespace EcommerceApi.ViewModel.Website
{
    public class WebsiteProductTypeViewModel
    {
        public int ProductTypeId { get; set; }
        public string ProductTypeName { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string ThumbnailImagePath { get; set; }
        public string HeaderImagePath { get; set; }
        public string Description { get; set; }
        public int SortOrder { get; set; }
        public bool ShowOnWebsite { get; set; }
        public bool Disabled { get; set; }
        public int? ParentProductTypeId { get; set; }
        public int ProductCount { get; set; }
        public int Rank { get; set; }
    }
}
