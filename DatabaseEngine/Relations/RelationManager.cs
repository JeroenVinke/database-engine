using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseEngine.Relations
{
    public class RelationManager
    {
        private StorageFile _storageFile;
        public static List<Relation> Relations = new List<Relation>();
        public static List<Table> Tables = new List<Table>();

        public RelationManager(StorageFile storageFile)
        {
            _storageFile = storageFile;

            LoadDefaultRelations();
            LoadRelationsFromStorage();
        }

        private void LoadRelationsFromStorage()
        {
            Table tablesTable = GetTable("Tables");
            Table columnsTable = GetTable("Columns");
            Table indexesTable = GetTable("Indexes");

            foreach (CustomTuple table in tablesTable.All())
            {
                int relationId = table.GetValueFor<int>("Id");

                TableDefinition tableDefinition = new TableDefinition
                {
                    Id = relationId,
                    Name = table.GetValueFor<string>("Name")
                };

                foreach (CustomTuple column in columnsTable.All().Where(x => x.GetValueFor<int>("RelationId") == relationId))
                {
                    tableDefinition.Add(new AttributeDefinition() { Name = column.GetValueFor<string>("Name"), Type = (ValueType)column.GetValueFor<int>("Type") });
                }

                foreach (CustomTuple index in indexesTable.All().Where(x => x.GetValueFor<int>("RelationId") == relationId))
                {
                    if (index.GetValueFor<bool>("IsClustered"))
                    {
                        tableDefinition.AddClusteredIndex(new List<AttributeDefinition> {
                                tableDefinition.First(x => x.Name == index.GetValueFor<string>("Columns"))
                            });
                    }
                    else
                    {
                        tableDefinition.AddNonClusteredIndex(new List<AttributeDefinition> {
                                tableDefinition.First(x => x.Name == index.GetValueFor<string>("Columns"))
                            });
                    }
                }

                Relations.Add(tableDefinition);
                Tables.Add(new Table(this, _storageFile, tableDefinition, new Pointer(table.GetValueFor<int>("RootBlock"))));
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
            TablesTable.Add(new AttributeDefinition() { Name = "RootBlock", Type = ValueType.Integer });
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
            IndexesTable.Add(new AttributeDefinition() { Name = "Columns", Type = ValueType.String });
            Relations.Add(IndexesTable);

            Table tablesTable = new Table(this, _storageFile, TablesTable, new Pointer(0, 0));
            Tables.Add(tablesTable);

            Table columnsTable = new Table(this, _storageFile, ColumnsTable, new Pointer(1, 0));
            Tables.Add(columnsTable);

            Table indexesTable = new Table(this, _storageFile, IndexesTable, new Pointer(2, 0));
            Tables.Add(indexesTable);
        }

        public bool TableExists(string v)
        {
            return GetTable(v) != null;
        }

        public void CreateTable(TableDefinition table)
        {
            Table tablesTable = GetTable("Tables");
            Table columnsTable = GetTable("Columns");
            Table indexesTable = GetTable("Indexes");

            int rootBlock = _storageFile.GetFreeBlock().Short;

            tablesTable.Insert(new object[] { table.Id, table.Name, rootBlock });

            foreach(AttributeDefinition column in table)
            {
                columnsTable.Insert(new object[] { table.Id, column.Type, column.Name });
            }

            foreach(Index index in table.GetIndexes())
            {
                indexesTable.Insert(new object[] { table.Id, index.Clustered, string.Join("|", index.Columns.Select(x => x.Name)) });
            }

            Tables.Add(new Table(this, _storageFile, table, new Pointer(rootBlock)));
        }

        private void CreateIndexRelation(ValueType type)
        {
            int id = -1;

            switch(type)
            {
                case ValueType.Integer:
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
                indexRelation.Add(new AttributeDefinition() { Name = "LeftPointer", Type = ValueType.Integer });
                indexRelation.Add(new AttributeDefinition() { Name = "ValuePointer", Type = ValueType.Integer });
                indexRelation.Add(new AttributeDefinition() { Name = "RightPointer", Type = ValueType.Integer });
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
