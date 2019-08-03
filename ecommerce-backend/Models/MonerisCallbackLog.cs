using System;

namespace EcommerceApi.Models
{
    public class MonerisCallbackLog
    {
        public int Id { get; set; }
        public string Response { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}