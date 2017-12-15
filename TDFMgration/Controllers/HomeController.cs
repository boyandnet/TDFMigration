using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TDFMgration.Models;
using System.Data.SqlClient;
using System.Data;

namespace TDFMgration.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            var dfDealers = GetDFDealers();
            var ftDealers = GetFTDealers();

            var crossDiff = from d in dfDealers
                            join f in ftDealers on d.Email equals f.Email
                            where !d.Equals(f)
                            select new Result
                            {
                                DFDealer = d,
                                FTDealer = f,
                                ResultType = ResultType.DIFF
                            };

            var onlyExistInDF = from d in dfDealers
                                join f in ftDealers on d.Email equals f.Email into gp
                                from g in gp.DefaultIfEmpty()
                                where g == null
                                select new Result
                                {
                                    DFDealer = d,
                                    FTDealer = g ?? null,
                                    ResultType = ResultType.OnlyInDF
                                };

            var onlyExistInFT = from f in ftDealers
                                join d in dfDealers on f.Email equals d.Email into gp
                                from g in gp.DefaultIfEmpty()
                                where g == null
                                select new Result
                                {
                                    DFDealer = g ?? null,
                                    FTDealer = f,
                                    ResultType = ResultType.OnlyInFT
                                };

            var result = new List<Result>();
            result.AddRange(crossDiff);
            result.AddRange(onlyExistInDF);
            result.AddRange(onlyExistInFT);

            return View(result);
        }

        private List<DFDealer> GetDFDealers()
        {
            var dealers = new List<DFDealer>();
            var connectionStr = "Data Source=.;Initial Catalog=DealersForum;Integrated Security=True;";
            //var connectionStr = "Data Source=.;Initial Catalog=DealersForum;User ID=sa;password=P@ssw0rd123;";
            using (var connection = new SqlConnection(connectionStr))
            {
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.Connection.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
                                select D.DealerId,
                                       D.DealerName, 
                                       D.DealerNumber, 
                                       D.StreetAddress, 
                                       D.City, 
                                       S.AbbreviationName as StateCode, 
                                       D.ZipCode, 
                                       D.Phone, 
                                       D.Fax, 
                                       D.Website, 
                                       U.FirstName, 
                                       U.Lastname, 
                                       U.Email,
                                       U.Cellphone,
                                       D.DMVClerkFirstName,
                                       D.DMVClerkLastName,
                                       D.DMVClerkEmail,
                                       D.DMVClerkPhone,
                                       D.AcctRepEmail
                                from Dealer D 
                                join State S on D.StateId = S.StateId 
                                join [User] U on D.DealerId = U.DealerId and U.IsPrimaryUser = 1 and U.IsDeleted = 0 
                                where D.IsDeleted = 0 AND D.DealerType = 1 
                                order by D.LastChangeDateTime";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var dealer = new DFDealer
                            {
                                DealerId = reader.GetInt32(0),
                                DealerName = reader.GetString(1),
                                DealerNumber = reader.GetString(2),
                                Street = reader.GetString(3),
                                City = reader.GetString(4),
                                StateCode = reader.GetString(5),
                                ZipCode = reader.GetString(6),
                                Phone = reader.GetString(7),
                                Fax = reader.GetString(8),
                                Website = reader.GetString(9),
                                FirstName = reader.GetString(10),
                                LastName = reader.GetString(11),
                                Email = reader.GetString(12),
                                CellPhone = reader.GetString(13),
                                DMVClerkFirstName = reader.IsDBNull(14) ? null : reader.GetString(14),
                                DMVClerkLastName = reader.IsDBNull(15) ? null : reader.GetString(15),
                                DMVClerkEmail = reader.IsDBNull(16) ? null : reader.GetString(16),
                                DMVClerkPhone = reader.IsDBNull(17) ? null : reader.GetString(17),
                                AcctRepEmail = reader.IsDBNull(18) ? null : reader.GetString(18)
                            };

                            dealers.Add(dealer);
                        }
                    }
                }
            }

            return dealers;
        }

        private List<FTDealer> GetFTDealers()
        {
            var dealers = new List<FTDealer>();
            var connectionStr = "Data Source=.;Initial Catalog=FreshTrade17;Integrated Security=True;";
            //var connectionStr = "Data Source=juzdd0tgzn.database.windows.net,1433;Initial Catalog=FreshTrade;User ID=saperium-testdb-user@juzdd0tgzn;Password=P@ssw0rd123;";
            using (var connection = new SqlConnection(connectionStr))
            {
                using (var cmd = new SqlCommand())
                {
                    cmd.Connection = connection;
                    cmd.Connection.Open();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = @"
                                select DD.DealershipId as DealershipId,
		                                DD.DealershipName as DealerName,
		                                DD.DealershipNumber as DealerNumber,
		                                DD.Street as Street,
		                                DD.City as City,
		                                ST.Code as StateCode,
		                                DD.ZipCode as ZipCode,
		                                DD.Phone as Phone,
		                                dd.Fax as Fax,
		                                dd.Website as Website,
		                                U.FirstName as FirstName,
		                                U.LastName as LastName,
		                                U.Email as Email,
		                                e.CellPhone as CellPhone,
		                                TitleClerkUser.FirstName as DMVClerkFirstName,
		                                TitleClerkUser.LastName as DMVClerkLastName,
		                                TitleClerkEmployee.CellPhone as DMVClerkPhone,
		                                TitleClerkUser.Email as DMVClerkEmail,
		                                AcctRepUser.Email as AcctRepEmail
                                from Dealership DD
                                join Dealer D on DD.DealershipId = D.DealershipId and D.DeleteDateTO is null
                                join Employee E on D.EmployeeId = E.EmployeeId and E.EmployeeTypeId = 2 /* Dealer */ and E.DeleteDateTO is null
                                join [User] U on E.UserId = U.UserId and U.DeleteDateTO is null
                                join UserRole UR on U.UserId = UR.UserId and UR.RoleId = 5 /* Seller */ and UR.DeleteDateTO is null
                                join State ST on ST.StateId = DD.StateId
                                left join Employee AcctRepEmployee on DD.AccountRepId = AcctRepEmployee.EmployeeId
                                left join [User] AcctRepUser on AcctRepEmployee.UserId = AcctRepUser.UserId
                                left join Employee TitleClerkEmployee on DD.TitleClerkId = TitleClerkEmployee.EmployeeId
                                left join [User] TitleClerkUser on TitleClerkEmployee.UserId = TitleClerkUser.UserId";

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var dealer = new FTDealer
                            {
                                DealerId = reader.GetInt32(0),
                                DealerName = reader.IsDBNull(1) ? null : reader.GetString(1),
                                DealerNumber = reader.IsDBNull(2) ? null : reader.GetString(2),
                                Street = reader.IsDBNull(3) ? null : reader.GetString(3),
                                City = reader.IsDBNull(4) ? null : reader.GetString(4),
                                StateCode = reader.IsDBNull(5) ? null : reader.GetString(5),
                                ZipCode = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Phone = reader.IsDBNull(7) ? null : reader.GetString(7),
                                Fax = reader.IsDBNull(8) ? null : reader.GetString(8),
                                Website = reader.IsDBNull(9) ? null : reader.GetString(9),
                                FirstName = reader.IsDBNull(10) ? null : reader.GetString(10),
                                LastName = reader.IsDBNull(11) ? null : reader.GetString(11),
                                Email = reader.IsDBNull(12) ? null : reader.GetString(12),
                                CellPhone = reader.IsDBNull(13) ? null : reader.GetString(13),
                                DMVClerkFirstName = reader.IsDBNull(14) ? null : reader.GetString(14),
                                DMVClerkLastName = reader.IsDBNull(15) ? null : reader.GetString(15),
                                DMVClerkEmail = reader.IsDBNull(16) ? null : reader.GetString(16),
                                DMVClerkPhone = reader.IsDBNull(17) ? null : reader.GetString(17),
                                AcctRepEmail = reader.IsDBNull(18) ? null : reader.GetString(18)
                            };

                            dealers.Add(dealer);
                        }
                    }
                }
            }

            return dealers;
        }

        public IActionResult Details(string id)
        {
            var dfDealers = GetDFDealers();
            var ftDealers = GetFTDealers();

            var result = new Result()
            {
                DFDealer = dfDealers.FirstOrDefault(d => d.Email.Equals(id, StringComparison.CurrentCultureIgnoreCase)),
                FTDealer = ftDealers.FirstOrDefault(d => d.Email.Equals(id, StringComparison.CurrentCultureIgnoreCase))
            };

            result.ResultType = result.DFDealer == null ? ResultType.OnlyInFT : result.FTDealer == null ? ResultType.OnlyInDF : ResultType.DIFF;

            return View(result);
        }


        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
