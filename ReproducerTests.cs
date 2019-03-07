using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TrackingErrorReproducer.Model;
using Xunit;

namespace TrackingErrorReproducer
{
    namespace Model
    {
        public class LeftModel {
            public int Id { get; set; }
            public List<MiddleModel> Other { get; set; }
        }

        public class MiddleModel {
            public int LeftId { get; set; }
            public LeftModel Left { get; set; }
            public int RightId { get; set; }
        }

        public class ReproducerContext : DbContext {
            public ReproducerContext(DbContextOptions options) : base(options)
            {
            }

            public DbSet<LeftModel> Left { get; set; }
            public DbSet<MiddleModel> Middle { get; set; }

            protected override void OnModelCreating (ModelBuilder modelBuilder) {
                modelBuilder.Entity<MiddleModel>().HasKey(key => new { key.LeftId, key.RightId });
            }
        }
    }

    namespace Test
    {
        public class TrackingErrorReproducerTests {
            internal static class SeedData {
                public static LeftModel LeftModelEntry { get; set; } = new LeftModel { Id = 1 };
            }
            [Fact]
            public void Test1() {
                // var sqlite = new SqliteConnection("DataSource=:memory:");
                // sqlite.Open();
                // var options = new DbContextOptionsBuilder<ReproducerContext>().UseSqlite(sqlite).Options;

                var options = new DbContextOptionsBuilder<ReproducerContext>().UseInMemoryDatabase("Test1-Database").Options;
                var context = new ReproducerContext(options);
                context.Database.EnsureCreated();

                context.Left.Add(SeedData.LeftModelEntry);
                context.Middle.Add(new MiddleModel() { LeftId = 1 , RightId = 1 });
            }

            [Fact]
            public void Test2() {
                // var sqlite = new SqliteConnection("DataSource=:memory:");
                // sqlite.Open();
                // var options = new DbContextOptionsBuilder<ReproducerContext>().UseSqlite(sqlite).Options;

                var options = new DbContextOptionsBuilder<ReproducerContext>().UseInMemoryDatabase("Test2-Database").Options;
                var context = new ReproducerContext(options);
                context.Database.EnsureCreated();

                context.Left.Add(SeedData.LeftModelEntry);

                // Adding this entry fails with the error "The instance of entity type 'CompositeKeyModel' cannot be tracked because another instance with the same key value for {'LeftId', 'RightId'} is already being tracked"
                // This it is the same enty from Test1. But the Context isn't the same.
                context.Middle.Add(new MiddleModel() { LeftId = 1, RightId = 1 });
            }
        }
    }
}