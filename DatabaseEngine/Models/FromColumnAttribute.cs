using System;

namespace DatabaseEngine.Models
{
    public class FromColumnAttribute : Attribute
    {
        public string ColumnName { get; set; }

        public FromColumnAttribute(string columnName)
        {
            ColumnName = columnName;
        }
    }
}