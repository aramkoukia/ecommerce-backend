using System;

namespace EcommerceApi.Models
{
    public class ProductPackage
    {
        public int ProductPackageId { get; set; }
        public int ProductId { get; set; }
        public string Package { get; set; }
        public decimal AmountInMainPackage { get; set; }
        public decimal PackagePrice { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
