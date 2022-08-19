using System;

namespace SimpleCore.UndoRedoes
{
    /// <summary>
    ///     撤销还原记录的接口。
    /// </summary>
    public interface IRecord
    {
        /// <summary>
        ///     撤销。
        /// </summary>
        void Undo();

        /// <summary>
        ///     还原。
        /// </summary>
        void Redo();

        /// <summary>
        ///     撤销还原记录移除时的回调。
        /// </summary>
        void OnRemove();

        /// <summary>
        ///     执行撤销还原时异常的回调。
        /// </summary>
        /// <param name="ex"></param>
        void OnException(Exception ex);
    }
}