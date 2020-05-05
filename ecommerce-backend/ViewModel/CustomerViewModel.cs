namespace EcommerceApi.ViewModel
{
    public class CustomerViewModel
    {
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyName { get; set; }
        public string PhoneNumber { get; set; }
        public string Mobile { get; set; }
        public string Country { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string PostalCode { get; set; }
        public string PstNumber { get; set; }
        public decimal CreditLimit { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public decimal AccountBalance { get; set; }
        public decimal StoreCredit { get; set; }
        public string PstExempt => string.IsNullOrEmpty(PstNumber) ? "No" : "Yes";
        public bool Disabled { get; set; }
        public bool CreditCardOnFile { get; set; }
        public string IsDisabled => Disabled ? "Yes" : "No";
        public string IsCreditCardOnFile => CreditCardOnFile ? "Yes" : "No";
    }
}
