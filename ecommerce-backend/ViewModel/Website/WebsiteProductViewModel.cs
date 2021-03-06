﻿using System.Collections.Generic;

namespace EcommerceApi.ViewModel.Website
{
    public class WebsiteProductViewModel
    {
        public WebsiteProductViewModel()
        {
            Tags = new List<WebsiteProductTag>();
        }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string ProductTypeName { get; set; }
        public string Balance { get; set; }
        public string Description { get; set; }
        public string UserManualPath { get; set; }
        public string WarrantyInformation { get; set; }
        public string AdditionalInformation { get; set; }
        public string HeaderImagePath { get; set; }
        public string Detail { get; set; }
        public string SlugsUrl { get; set; }
        public string[] ImagePaths { get; set; }
        public List<WebsiteProductTag> Tags { get; set; }
    }
}