using Compiler.Common;
using DatabaseEngine.BTree;
using DatabaseEngine.LogicalPlan;
using System;
using System.Collections.Generic;

namespace DatabaseEngine.Operations
{
    public class IndexSeekOperation : PhysicalOperation
    {
        public Table Table { get; set; }
        public Condition Condition { get; }

        public IBPlusTreeNode _index;
        public IBPlusTreeNode _currentNode;
        private ScanEnumerator _enumerator;

        public IndexSeekOperation(LogicalElement logicalElement, Table table, IBPlusTreeNode index, Condition condition)
            : base(logicalElement)
        {
            Table = table;
            Condition = condition;
            _index = index;
        }

        public override string ToString()
        {
            return typeof(IndexSeekOperation).Name + "(" + Table.TableDefinition.Name + ")";
        }

        public override void Prepare()
        {
            base.Prepare();

            _currentNode = _index.GetFirstLeaf();
            _enumerator = new ScanEnumerator(Table.TableDefinition, _index, Condition, IsLeftToRightCondition(Condition));
        }

        public override int EstimateIOCost()
        {
            int b = Program.StatisticsManager.B(Table.TableDefinition);

            if (b == 0)
            {
                return 0;
            }

            int t = Program.StatisticsManager.T(Table.TableDefinition);

            if (t == 0)
            {
                return 0;
            }

            double ratioRecordToBlock = (double)b / (double)t;

            return (int)Math.Ceiling(((ReadLogicalElement)LogicalElement).T() * ratioRecordToBlock);
        }

        private bool IsLeftToRightCondition(Condition condition)
        {
            LeafCondition leaf = (LeafCondition)condition;
            RelOp operation = leaf.Operation;

            return operation == RelOp.GreaterOrEqualThan
                || operation == RelOp.GreaterThan;
        }

        public override CustomTuple GetNext()
        {
            if (_enumerator.MoveNext())
            {
                Block block = Table.MemoryManager.Read(Table.TableDefinition, _enumerator.CurrentValue.Pointer);
                Record record = block.GetRecordForRowId(_enumerator.CurrentValue.Pointer.Index);

                return new CustomTuple(Table.TableDefinition).FromRecord(record);
            }

            return null;
        }
    }
}
