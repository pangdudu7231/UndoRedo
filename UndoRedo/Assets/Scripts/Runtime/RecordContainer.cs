using System;
using System.Collections.Generic;

namespace SimpleCore.UndoRedoes
{
    /// <summary>
    ///     撤销还原记录的容器。
    /// </summary>
    public sealed class RecordContainer
    {
        #region private members

        private readonly int _limit; //可以记录的最大数量（小于0表示无限）
        private readonly List<IRecord> _stack; //记录的集合
        private int _stackPointer = -1; //指针的索引 
        private bool _enable = true; //是否启用(默认是启用的)

        #endregion

        #region public properties

        /// <summary>
        ///     是否可以执行撤销操作。
        /// </summary>
        public bool CanUndo => _stackPointer >= 0;

        /// <summary>
        ///     可以撤销的记录数。
        /// </summary>
        public int UndoCount => _stackPointer + 1;

        /// <summary>
        ///     是否可以执行还原操作。
        /// </summary>
        public bool CanRedo => _stack.Count > 0 && _stackPointer < _stack.Count - 1;

        /// <summary>
        ///     可以还原的记录数。
        /// </summary>
        public int RedoCount => _stack.Count - _stackPointer - 1;

        /// <summary>
        ///     撤销还原是否启用。
        /// </summary>
        public bool Enable
        {
            get => _enable;
            set => _enable = value;
        }

        #endregion

        #region public event
        
        /// <summary>
        ///     记录新的撤销还原之后的回调
        /// </summary>
        public event Action<IRecord> OnEndRecordHandler;
        
        /// <summary>
        ///     执行撤销之后的回调
        /// </summary>
        public event Action<IRecord> OnEndUndoHandler;
        
        /// <summary>
        ///     执行还原之后的回调
        /// </summary>
        public event Action<IRecord> OnEndRedoHandler;

        #endregion

        #region ctor

        /// <summary>
        ///     构造函数。
        /// </summary>
        /// <param name="limit">-1 表示存储记录不设上限。</param>
        public RecordContainer(int limit = -1)
        {
            _limit = limit;
            _stack = new List<IRecord>();
        }

        #endregion

        #region public function

        /// <summary>
        ///     记录撤销还原。
        /// </summary>
        /// <param name="record"></param>
        public void Record(IRecord record)
        {
            if (!_enable) return;
            
            RecordInternal(record);
            OnEndRecordHandler?.Invoke(record);
        }

        /// <summary>
        ///     撤销。
        /// </summary>
        public void Undo()
        {
            if (!_enable || !CanUndo) return;

            var record = _stack[_stackPointer];
            UndoInternal();
            OnEndUndoHandler?.Invoke(record);
        }

        /// <summary>
        ///     还原。
        /// </summary>
        public void Redo()
        {
            if (!_enable || !CanRedo) return;

            var record = _stack[_stackPointer + 1];
            RedoInternal();
            OnEndRedoHandler?.Invoke(record);
        }
        
        /// <summary>
        ///     清空容器中的撤销还原记录。
        /// </summary>
        public void Clear()
        {
            //清理列表中的撤销还原操作
            RemoveRecord(0, _stack.Count);
            _stackPointer = -1;

            //清空注册事件
            OnEndRecordHandler = null;
            OnEndUndoHandler = null;
            OnEndRedoHandler = null;
        }

        #endregion

        #region private functions

        /// <summary>
        ///     记录撤销还原。
        /// </summary>
        /// <param name="record"></param>
        private void RecordInternal(IRecord record)
        {
            //移除所有的还原操作
            var removeStartIndex = _stackPointer + 1;
            var removeCount = _stack.Count - removeStartIndex;
            RemoveRecord(removeStartIndex, removeCount);
            //添加新的操作
            _stack.Add(record);
            if (!CanAdd(_limit, _stack.Count)) RemoveRecord(0, 1); //超出限制的长度，删除第一个事件
            _stackPointer = _stack.Count - 1;

            //判断是否可以添加新的记录
            bool CanAdd(int p_limit, int p_stackCount)
            {
                return p_limit < 0 || p_stackCount < p_limit;
            }
        }

        /// <summary>
        ///     撤销。
        /// </summary>
        private void UndoInternal()
        {
            var record = _stack[_stackPointer--];
            PackUndo(record, _stackPointer + 1);

            //undo 方法封装Try catch
            void PackUndo(IRecord p_record, int p_index)
            {
                try
                {
                    p_record.Undo();
                }
                catch (Exception ex)
                {
                    p_record.OnException(ex);
                    RemoveRecord(p_index, 1); //将记录移除
                }
            }
        }

        /// <summary>
        ///     还原。
        /// </summary>
        private void RedoInternal()
        {
            var record = _stack[++_stackPointer];
            PackRedo(record, _stackPointer);

            //redo 方法封装Try catch
            void PackRedo(IRecord p_record, int p_index)
            {
                try
                {
                    p_record.Redo();
                }
                catch (Exception ex)
                {
                    p_record.OnException(ex);
                    RemoveRecord(p_index, 1); //将记录移除
                }
            }
        }

        /// <summary>
        ///     移除记录。
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        private void RemoveRecord(int startIndex, int count)
        {
            if (count <= 0) return;

            var removes = _stack.GetRange(startIndex, count);
            _stack.RemoveRange(startIndex, count);
            foreach (var record in removes) PackRemove(record);

            //remove 方法封装Try catch
            void PackRemove(IRecord p_record)
            {
                try
                {
                    p_record.OnRemove();
                }
                catch (Exception ex)
                {
                    p_record.OnException(ex);
                }
            }
        }

        #endregion
    }
}