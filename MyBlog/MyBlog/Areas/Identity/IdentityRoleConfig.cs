using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.ComponentModel.DataAnnotations;

namespace MyBlog.Areas.Identity
{
    public class IdentityRoleConfig : IEntityTypeConfiguration<IdentityRole>
    {
        public void Configure(EntityTypeBuilder<IdentityRole> builder)
        {
            builder.HasData(
                new IdentityRole {Id = "5c59aeb6-264d-4693-a36c-34e32ee2d9f4", Name = "Admin", NormalizedName = "ADMIN" },
                new IdentityRole {Id = "8706a370-a288-4536-9412-15cbb61bd7e2", Name = "Editör", NormalizedName = "EDITOR" },
                new IdentityRole {Id = "b4f504ab-6d9a-47c5-b7ea-a14867b0a834", Name = "User", NormalizedName = "USER" }
            );
        }
       
        
    }
}
