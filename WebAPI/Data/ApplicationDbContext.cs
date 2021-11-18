using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Data.Entities;

namespace WebAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }
    }
}
