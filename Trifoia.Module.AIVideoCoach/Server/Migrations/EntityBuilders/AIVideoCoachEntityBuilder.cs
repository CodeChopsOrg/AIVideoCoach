using CodeChops.Module.AIVideoCoach.Shared.Enums;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Migrations.Operations.Builders;
using Oqtane.Databases.Interfaces;
using Oqtane.Migrations;
using Oqtane.Migrations.EntityBuilders;

namespace CodeChops.Module.AIVideoCoach.Migrations.EntityBuilders
{
    public class AIVideoCoachEntityBuilder : AuditableBaseEntityBuilder<AIVideoCoachEntityBuilder>
    {
        private const string _entityTableName = "TrifoiaAIVideoCoach";
        private readonly PrimaryKey<AIVideoCoachEntityBuilder> _primaryKey = new("PK_TrifoiaAIVideoCoach", x => x.AIVideoCoachId);
        private readonly ForeignKey<AIVideoCoachEntityBuilder> _moduleForeignKey = new("FK_TrifoiaAIVideoCoach_Module", x => x.ModuleId, "Module", "ModuleId", ReferentialAction.Cascade);

        public AIVideoCoachEntityBuilder(MigrationBuilder migrationBuilder, IDatabase database) : base(migrationBuilder, database)
        {
            EntityTableName = _entityTableName;
            PrimaryKey = _primaryKey;
            ForeignKeys.Add(_moduleForeignKey);
        }

        protected override AIVideoCoachEntityBuilder BuildTable(ColumnsBuilder table)
        {
            AIVideoCoachId = AddAutoIncrementColumn(table,"AIVideoCoachId");
            ModuleId = AddIntegerColumn(table,"ModuleId");
            Name = AddMaxStringColumn(table,"Name");
            AddAuditableColumns(table);
            return this;
        }

        public OperationBuilder<AddColumnOperation> AIVideoCoachId { get; set; }
        public OperationBuilder<AddColumnOperation> ModuleId { get; set; }
        public OperationBuilder<AddColumnOperation> Name { get; set; }
    }
}
