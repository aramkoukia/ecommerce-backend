using System;

namespace EcommerceApi.Models.Website
{
    public partial class BlogPost
    {
        public BlogPost()
        {
        }
        public int Id { get; set; }
        public string Title { get; set; }
        public string ShortDescription { get; set; }
        public string LongDescription { get; set; }
        public string SlugsUrl { get; set; }
        public string Tags { get; set; }
        public string SmallImagePath { get; set; }
        public string LargeImagePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime PublishedDate { get; set; }
        public bool Published { get; set; }
    }
}