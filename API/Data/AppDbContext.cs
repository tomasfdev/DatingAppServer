using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<AppUser> Users { get; set; }
        public DbSet<UserLike> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            #region AppUser-UserLike
            builder.Entity<UserLike>()
                .HasKey(k => new {k.SourceUserId, k.TargetUserId });

            builder.Entity<UserLike>()
                .HasOne(s => s.SourceUser)  //Um user pode gostar de muitos users. ex: Lisa gosta de...
                .WithMany(l => l.LikedUsers)    //ex:... Peter, Paul, Bob
                .HasForeignKey(s => s.SourceUserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<UserLike>()
                .HasOne(t => t.TargetUser)  //Um user pode gostar de muitos users. ex: Lisa gosta de...
                .WithMany(l => l.LikedByUsers)    //ex:... Peter, Paul, Bob
                .HasForeignKey(t => t.TargetUserId)
                .OnDelete(DeleteBehavior.NoAction);
            #endregion

            #region AppUser-Message
            builder.Entity<Message>()
                .HasOne(s => s.Sender)
                .WithMany(m => m.MessagesSent)
                .OnDelete(deleteBehavior: DeleteBehavior.Restrict);

            builder.Entity<Message>()
                .HasOne(s => s.Recipient)
                .WithMany(m => m.MessagesReceived)
                .OnDelete(deleteBehavior: DeleteBehavior.Restrict);
            #endregion
        }
    }
}
