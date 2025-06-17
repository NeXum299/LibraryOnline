using Microsoft.EntityFrameworkCore;

namespace AspAppLibrary.EF
{
    public class ApplicationContext : DbContext
    {
        public DbSet<Book> Books { get; set; } = null!;
        public DbSet<BorrowRecord> BorrowRecords { get; set; } = null!;
        public DbSet<Reader> Readers { get; set; } = null!;

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Book>()
                .HasMany(b => b.BorrowRecords)
                .WithOne(br => br.Book)
                .HasForeignKey(br => br.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Reader>()
                .HasMany(r => r.BorrowRecords)
                .WithOne(br => br.Reader)
                .HasForeignKey(br => br.ReaderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
