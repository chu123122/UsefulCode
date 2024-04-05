using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Pool.BasicPool
{
    // 对象池管理器
    public class ObjcctPoolManager : MonoBehaviour
    {
        #region 单例模式
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
        /// 通过对象池管理器来创建并引用特定对象池中的一个物体
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
            //(存在更高效的优化，但优化后可读性下降)
            if (!_pools.ContainsKey(poolName))
            {
                if (typeof(T) == typeof(Enemy))
                {
                    EnemyPool pool = new EnemyPool(prefab as Enemy, initialSize, parent, unUsedObjectLifetime);
                    _pools.Add(poolName, pool);
                    StartCoroutine(pool.CheckUnusedObjectsCoroutine()); // 启动协程
                }
                else
                {
                    ObjectPool<T> pool = new ObjectPool<T>(prefab, initialSize, parent, unUsedObjectLifetime);
                    _pools.Add(poolName, pool);
                    StartCoroutine(pool.CheckUnusedObjectsCoroutine()); // 启动协程
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
        /// 通过对象池管理器来返回物体到特定的对象池
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
                Debug.LogWarning($"对象池 '{poolName}' 不存在!");
            }
        }

    }
}

