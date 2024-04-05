using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pool.BasicPool
{
    // ����ع�����
    public class ObjcctPoolManager : MonoBehaviour
    {
        #region ����ģʽ
        private static ObjcctPoolManager _instance;
        public static ObjcctPoolManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ObjcctPoolManager>();
                    if (_instance == null)
                    {
                        GameObject obj = new GameObject("ObjcctPoolManager");
                        _instance = obj.AddComponent<ObjcctPoolManager>();
                    }
                }
                return _instance;
            }
        }
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        #endregion

        private readonly Dictionary<string, object> _pools = new Dictionary<string, object>();
        /// <summary>
        /// ͨ������ع������������������ض�������е�һ������
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="poolName"></param>
        /// <param name="prefab"></param>
        /// <param name="initialSize"></param>
        /// <param name="parent"></param>
        /// <param name="unUsedObjectLifetime"></param>
        /// <returns></returns>
        public T GetObject<T>(string poolName, T prefab, int initialSize, Transform parent, float unUsedObjectLifetime) where T : MonoBehaviour, IPoolable
        {
            //(���ڸ���Ч���Ż������Ż���ɶ����½�)
            if (!_pools.ContainsKey(poolName))
            {
                if (typeof(T) == typeof(Enemy))
                {
                    EnemyPool pool = new EnemyPool(prefab as Enemy, initialSize, parent, unUsedObjectLifetime);
                    _pools.Add(poolName, pool);
                    StartCoroutine(pool.CheckUnusedObjectsCoroutine()); // ����Э��
                }
                else
                {
                    ObjectPool<T> pool = new ObjectPool<T>(prefab, initialSize, parent, unUsedObjectLifetime);
                    _pools.Add(poolName, pool);
                    StartCoroutine(pool.CheckUnusedObjectsCoroutine()); // ����Э��
                }
            }

            if (_pools[poolName] is EnemyPool enemyPool)
            {
                return enemyPool.Get() as T;
            }
            else
            {
                ObjectPool<T> objectPool = (ObjectPool<T>)_pools[poolName];
                return objectPool.Get();
            }
        }
        /// <summary>
        /// ͨ������ع��������������嵽�ض��Ķ����
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="poolName"></param>
        /// <param name="obj"></param>
        public void ReturnObject<T>(string poolName, T obj) where T : MonoBehaviour, IPoolable
        {
            if (_pools.ContainsKey(poolName))
            {
                if (_pools[poolName] is EnemyPool enemyPool)
                {
                    enemyPool.Return(obj as Enemy);
                }
                else
                {
                    ObjectPool<T> objectPool = (ObjectPool<T>)_pools[poolName];
                    objectPool.Return(obj);
                }
            }
            else
            {
                Debug.LogWarning($"����� '{poolName}' ������!");
            }
        }

    }
}

