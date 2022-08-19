using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SimpleCore.UndoRedoes.Example
{
    public class UndoRedoDemo : MonoBehaviour
    {
        #region private members

        [SerializeField] private Button _undoBtn, _redoBtn; //撤销还原按钮
        [SerializeField] private Text _undoCountTxt, _redoCountTxt;
        [SerializeField] private Button _spawnBtn, _delBtn;
        [SerializeField] private Text _spawnCountTxt;
        [SerializeField] private Button _clearBtn;//清除记录
        
        private RecordContainer _container;//撤销还原记录的容器。
        private readonly List<GameObject> _spawnGos = new(); //保存生成的物体

        #endregion

        #region unity life cycles

        private void Start()
        {
            InitContainer();
            AddListeners();
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        #endregion

        #region public functions

        public void AddGameObject(GameObject go)
        {
            _spawnGos.Add(go);
            _spawnCountTxt.text = _spawnGos.Count.ToString();
        }

        public void RemoveGameObject(GameObject go)
        {
            _spawnGos.Remove(go);
            _spawnCountTxt.text = _spawnGos.Count.ToString();
        }

        #endregion

        #region private functions

        /// <summary>
        ///     初始化撤销还原容器
        /// </summary>
        private void InitContainer()
        {
            var container = new RecordContainer(20); //设置容器只保存20条记录
            container.OnEndRecordHandler += _ => ResetView();
            container.OnEndUndoHandler += _ => ResetView();
            container.OnEndRedoHandler += _ => ResetView();
            _container = container;
        }

        /// <summary>
        ///     注册监听事件
        /// </summary>
        private void AddListeners()
        {
            _undoBtn.onClick.AddListener(_container.Undo);
            _redoBtn.onClick.AddListener(_container.Redo);
            _clearBtn.onClick.AddListener(() =>
            {
                _container.Clear();
                ResetView();
                //重新注册一下事件
                _container.OnEndRecordHandler += _ => ResetView();
                _container.OnEndUndoHandler += _ => ResetView();
                _container.OnEndRedoHandler += _ => ResetView();
            });
            _spawnBtn.onClick.AddListener(SpawnGO);
            _delBtn.onClick.AddListener(DeleteGO);
        }

        /// <summary>
        ///     注销监听事件
        /// </summary>
        private void RemoveListeners()
        {
            _undoBtn.onClick.RemoveAllListeners();
            _redoBtn.onClick.RemoveAllListeners();
            _clearBtn.onClick.RemoveAllListeners();
            _spawnBtn.onClick.RemoveAllListeners();
            _delBtn.onClick.RemoveAllListeners();
        }

        /// <summary>
        ///     刷新页面显示
        /// </summary>
        private void ResetView()
        {
            var container = _container;
            _undoBtn.interactable = container.CanUndo;
            _redoBtn.interactable = container.CanRedo;
            _undoCountTxt.text = container.UndoCount.ToString();
            _redoCountTxt.text = container.RedoCount.ToString();
        }

        /// <summary>
        ///     随机生成物体
        /// </summary>
        private void SpawnGO()
        {
            var goType = (PrimitiveType) Random.Range(0, 6);
            var go = GameObject.CreatePrimitive(goType);
            var x = Random.Range(-15f, 15f);
            var y = Random.Range(-7f, 5f);
            go.transform.position = new Vector3(x, y, 15);
            AddGameObject(go);

            //记录撤销还原
            var spawnRecord = new SpawnRecord(this, go);
            _container.Record(spawnRecord);
        }

        /// <summary>
        ///     随机删除物体
        /// </summary>
        private void DeleteGO()
        {
            var total = _spawnGos.Count;
            if (total == 0) return;

            var index = Random.Range(0, total);
            var go = _spawnGos[index];
            go.SetActive(false);
            RemoveGameObject(go);

            //记录撤销还原
            var delRecord = new DelRecord(this, go);
            _container.Record(delRecord);
        }

        #endregion
    }
}