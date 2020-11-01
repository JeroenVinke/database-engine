using DatabaseEngine.Models;
using DatabaseEngine.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DatabaseEngine.Relations
{
    public class RelationManager
    {
        private MemoryManager _memoryManager;
        public List<Relation> Relations = new List<Relation>();
        public List<Table> Tables = new List<Table>();

        public RelationManager(MemoryManager memoryManager)
        {
            _memoryManager = memoryManager;
        }

        public void Initialize()
        {
            LoadDefaultRelations();
            LoadRelationsFromStorage();
        }

        private void LoadRelationsFromStorage()
        {
            foreach (CustomTuple tableTuple in Program.ExecuteQuery("SELECT * FROM Tables"))
            {
                TableDefinition tableDefinition = tableTuple.ToModel<TableDefinition>();

                foreach (CustomTuple columnTuple in Program.ExecuteQuery("SELECT * FROM Columns WHERE RelationId = " + tableDefinition.Id))
                {
                    tableDefinition.Add(columnTuple.ToModel<AttributeDefinition>());
                }

                foreach (CustomTuple indexTuple in Program.ExecuteQuery("SELECT * FROM Indexes WHERE RelationId = " + tableDefinition.Id))
                {
                    Index index = indexTuple.ToModel<Index>();
                    tableDefinition.AddIndex(index);
                }

                Relations.Add(tableDefinition);
                Tables.Add(new Table(this, _memoryManager, tableDefinition, new Pointer(tableDefinition.RootBlockId)));
            }
        }

        private void LoadDefaultRelations()
        {
            foreach(int valueId in Enum.GetValues(typeof(ValueType)))
            {
                CreateIndexRelation((ValueType)valueId);
            }

            TableDefinition TablesTable = new TableDefinition()
            {
                Name = "Tables",
                Id = 4
            };
            TablesTable.Add(new AttributeDefinition() { Name = "Id", Type = ValueType.Integer });
            TablesTable.Add(new AttributeDefinition() { Name = "Name", Type = ValueType.String });
            TablesTable.Add(new AttributeDefinition() { Name = "RootBlockId", Type = ValueType.UnsignedInteger });
            Relations.Add(TablesTable);


            TableDefinition ColumnsTable = new TableDefinition()
            {
                Name = "Columns",
                Id = 5
            };
            ColumnsTable.Add(new AttributeDefinition() { Name = "RelationId", Type = ValueType.Integer });
            ColumnsTable.Add(new AttributeDefinition() { Name = "Type", Type = ValueType.Integer });
            ColumnsTable.Add(new AttributeDefinition() { Name = "Name", Type = ValueType.String });
            Relations.Add(ColumnsTable);


            TableDefinition IndexesTable = new TableDefinition()
            {
                Name = "Indexes",
                Id = 6
            };
            IndexesTable.Add(new AttributeDefinition() { Name = "RelationId", Type = ValueType.Integer });
            IndexesTable.Add(new AttributeDefinition() { Name = "IsClustered", Type = ValueType.Boolean });
            IndexesTable.Add(new AttributeDefinition() { Name = "Column", Type = ValueType.String });
            IndexesTable.Add(new AttributeDefinition() { Name = "RootBlockId", Type = ValueType.UnsignedInteger });
            Relations.Add(IndexesTable);


            //TableDefinition StatisticsTable = new TableDefinition()
            //{
            //    Name = "Indexes",
            //    Id = 7
            //};
            //StatisticsTable.Add(new AttributeDefinition() { Name = "RelationId", Type = ValueType.Integer });
            //StatisticsTable.Add(new AttributeDefinition() { Name = "TotalSize", Type = ValueType.Integer });
            //StatisticsTable.Add(new AttributeDefinition() { Name = "Column", Type = ValueType.String });
            //StatisticsTable.Add(new AttributeDefinition() { Name = "RootBlockId", Type = ValueType.UnsignedInteger });
            //Relations.Add(StatisticsTable);

            Table tablesTable = new Table(this, _memoryManager, TablesTable, new Pointer(1, 0));
            Tables.Add(tablesTable);

            Table columnsTable = new Table(this, _memoryManager, ColumnsTable, new Pointer(2, 0));
            Tables.Add(columnsTable);

            Table indexesTable = new Table(this, _memoryManager, IndexesTable, new Pointer(3, 0));
            Tables.Add(indexesTable);

            //Table statisticsTable = new Table(this, _memoryManager, StatisticsTable, new Pointer(4, 0));
            //Tables.Add(statisticsTable);
        }

        public bool TableExists(string v)
        {
            return GetTable(v) != null;
        }

        public Table CreateTable(TableDefinition table)
        {
            Table tablesTable = GetTable("Tables");
            Table columnsTable = GetTable("Columns");
            Table indexesTable = GetTable("Indexes");

            uint rootBlock = _memoryManager.GetFreeBlock().Short;

            tablesTable.Insert(new object[] { table.Id, table.Name, rootBlock });

            foreach(AttributeDefinition column in table)
            {
                columnsTable.Insert(new object[] { table.Id, column.Type, column.Name });
            }

            foreach(Index index in table.GetIndexes())
            {
                index.RootPointer = index.IsClustered ? new Pointer(rootBlock): _memoryManager.GetFreeBlock();

                indexesTable.Insert(new object[] { table.Id, index.IsClustered, index.Column, index.RootPointer.Short});
            }

            Tables.Add(new Table(this, _memoryManager, table, new Pointer(rootBlock)));
            Relations.Add(table);

            return Tables.Last();
        }

        private void CreateIndexRelation(ValueType type)
        {
            int id = -1;

            switch(type)
            {
                case ValueType.Integer:
                    id = Constants.IntIndexRelationId;
                    break;
                case ValueType.UnsignedInteger:
                    id = Constants.IntIndexRelationId;
                    break;
                case ValueType.String:
                    id = Constants.StringIndexRelationId;
                    break;
            }

            if (id > -1)
            {
                Relation indexRelation = new Relation() { Name = "IndexRelation", Id = id };
                indexRelation.Add(new AttributeDefinition() { Name = "Value", Type = type });
                indexRelation.Add(new AttributeDefinition() { Name = "LeftPointer", Type = ValueType.UnsignedInteger });
                indexRelation.Add(new AttributeDefinition() { Name = "ValuePointer", Type = ValueType.UnsignedInteger });
                indexRelation.Add(new AttributeDefinition() { Name = "RightPointer", Type = ValueType.UnsignedInteger });
                Relations.Add(indexRelation);
            }
        }

        public Relation GetRelation(int id)
        {
            return Relations.FirstOrDefault(x => x.Id == id);
        }

        public Table GetTable(string name)
        {
            return Tables.FirstOrDefault(x => x.TableDefinition.Name.ToLower() == name.ToLower());
        }

        public Table GetTable(int id)
        {
            return Tables.FirstOrDefault(x => x.TableDefinition.Id == id);
        }
    }
}
