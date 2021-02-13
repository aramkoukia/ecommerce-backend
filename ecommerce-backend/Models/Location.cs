using System;

namespace EcommerceApi.Models
{
    [Serializable]
    public class Location
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; }
        public string LocationAddress { get; set; }
        public string Country { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public bool Disabled { get; set; }
        public bool ShowOnInvoice { get; set; }
        public string City { get; set; }
        public string MapUrl { get; set; }
        public string WorkingHours { get; set; }
        public string Email { get; set; }
        public int SortOrder { get; set; }

    }
}
