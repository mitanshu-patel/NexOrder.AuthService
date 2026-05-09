using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NexOrder.AuthService.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexOrder.AuthService.Infrastructure.EntityConfigurations
{
    public class RefreshTokenEntityConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            ArgumentNullException.ThrowIfNull(builder, nameof(builder));
            builder.HasIndex(v => v.Id);
            builder.Property(v => v.Token).IsRequired();
            builder.Property(v => v.Invalidated).IsRequired();
            builder.Property(v => v.ExpiryDate).IsRequired();
            builder.Property(v => v.JwtId).IsRequired();
            builder.Property(v => v.UserId).IsRequired();
            builder.Property(v => v.Email).IsRequired();
        }
    }
}
