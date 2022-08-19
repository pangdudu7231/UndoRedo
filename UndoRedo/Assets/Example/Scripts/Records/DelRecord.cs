using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SimpleCore.UndoRedoes.Example
{
    /// <summary>
    ///     删除物体的记录
    /// </summary>
    public sealed class DelRecord : IRecord
    {
        #region private members

        private readonly UndoRedoDemo _view;
        private readonly GameObject _gameObject;
        private bool _isDel = true; //记录当前物体状态是否是被删除的

        #endregion

        #region ctor

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="view"></param>
        /// <param name="gameObject"></param>
        public DelRecord(UndoRedoDemo view, GameObject gameObject)
        {
            _view = view;
            _gameObject = gameObject;
        }

        #endregion

        #region public functions

        public void Undo()
        {
            _gameObject.SetActive(true);
            _view.AddGameObject(_gameObject);
            _isDel = false;
        }

        public void Redo()
        {
            _gameObject.SetActive(false);
            _view.RemoveGameObject(_gameObject);
            _isDel = true;
        }

        public void OnRemove()
        {
            if (_isDel) Object.Destroy(_gameObject); //销毁物体
        }

        public void OnException(Exception ex)
        {
            Debug.LogError($"Execute delete record failure, message: {ex.Message}.");
        }

        #endregion
    }
}