using EcommerceApi.Untilities;
using System;
using System.Collections.Generic;

namespace EcommerceApi.ViewModel.Website
{
    public class WebsiteProductsInCategoryViewModel
    {
        public int ProductId { get; set; }
        public string ProductCode { get; set; }
        public string ProductTypeName { get; set; }
        public string Balance { get; set; }
        public string ImagePath { get; set; }
        public string ProductDescription { get; set; }
        public int Rank { get; set; }
        public string SlugsUrl { get; set; }
    }
}