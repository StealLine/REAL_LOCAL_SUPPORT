using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Support_Bot.DB_METHODS.Entitys;

namespace Support_Bot.DB_METHODS.Config
{
    public class TicketLogsConfiguration : IEntityTypeConfiguration<TicketLogs>
    {
        public void Configure(EntityTypeBuilder<TicketLogs> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.ticket_content).HasDefaultValue("");
            builder.Property(x => x.ticket_deleted_messages).HasDefaultValue("");
            builder.Property(x => x.ticket_edited_messages).HasDefaultValue("");
            builder.Property(x=>x.isactive).HasDefaultValue(true);
        }
    }
}
