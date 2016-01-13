using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Security.Claims;

namespace LightNlpWebApiSelfHost.Models
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext()
            : base("LightNlpWebApiSelfHostDb")
        {

        }

        static ApplicationDbContext()
        {
            Database.SetInitializer(new ApplicationDbInitializer());
        }

        //public IDbSet<Company> Companies { get; set; }
        public IDbSet<CustomUser> Users { get; set; }
        public IDbSet<CustomUserClaim> Claims { get; set; }
    }


    public class ApplicationDbInitializer 
        : DropCreateDatabaseAlways<ApplicationDbContext>
    {
        protected async override void Seed(ApplicationDbContext context)
        {
            //context.Companies.Add(new Company { Name = "Microsoft" });
            //context.Companies.Add(new Company { Name = "Apple" });
            //context.Companies.Add(new Company { Name = "Google" });
            //context.SaveChanges();

            // Set up two initial users with different role claims:
            var john = new CustomUser { Email = "test1@example.com" };
            var jimi = new CustomUser { Email = "test1@Example.com" };

            john.Claims.Add(new CustomUserClaim { ClaimType = ClaimTypes.Name, UserId = john.Id, ClaimValue = john.Email });
            john.Claims.Add(new CustomUserClaim { ClaimType = ClaimTypes.Role, UserId = john.Id, ClaimValue = "Admin" });

            jimi.Claims.Add(new CustomUserClaim { ClaimType = ClaimTypes.Name, UserId = jimi.Id, ClaimValue = jimi.Email });
            jimi.Claims.Add(new CustomUserClaim { ClaimType = ClaimTypes.Role, UserId = john.Id, ClaimValue = "User" });

            var store = new CustomUserStore(context);
            await store.AddUserAsync(john, "Test1Password");
            await store.AddUserAsync(jimi, "Test1Password");
        }
    }
}
