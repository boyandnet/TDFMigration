using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TDFMgration.Models
{
    public class DFDealer
    {
        public int DealerId { get; set; }
        public string DealerName { get; set; }
        public string DealerNumber { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string StateCode { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Website { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CellPhone { get; set; }
        public string DMVClerkFirstName { get; set; }
        public string DMVClerkLastName { get; set; }
        public string DMVClerkEmail { get; set; }
        public string DMVClerkPhone { get; set; }
        public string AcctRepEmail { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is FTDealer)
            {
                var ftDealer = obj as FTDealer;
                return DealerName.Equals(ftDealer.DealerName, StringComparison.CurrentCultureIgnoreCase)
                    && DealerNumber.Equals(ftDealer.DealerNumber, StringComparison.CurrentCultureIgnoreCase);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
