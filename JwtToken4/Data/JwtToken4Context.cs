using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using JwtToken4.Model;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace JwtToken4.Data
{
    public class JwtToken4Context : IdentityDbContext
    {
        public JwtToken4Context (DbContextOptions<JwtToken4Context> options)
            : base(options)
        {
        }

        public DbSet<JwtToken4.Model.Student> Student { get; set; } = default!;
    }
}
