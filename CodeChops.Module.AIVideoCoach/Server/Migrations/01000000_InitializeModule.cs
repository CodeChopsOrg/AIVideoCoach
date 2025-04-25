using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Trifoia.Module.AIVideoCoach.Migrations.EntityBuilders;
using Trifoia.Module.AIVideoCoach.Repository;

namespace CodeChops.Module.AIVideoCoach.Migrations
{
    [DbContext(typeof(AIVideoCoachContext))]
    [Migration("Trifoia.Module.AIVideoCoach.01.00.00.00")]
    public class InitializeModule : MultiDatabaseMigration
    {
        public InitializeModule(IDatabase database) : base(database)
        {
        }

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var entityBuilder = new AIVideoCoachEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilder.Create();
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var entityBuilder = new AIVideoCoachEntityBuilder(migrationBuilder, ActiveDatabase);
            entityBuilder.Drop();
        }
    }
}
