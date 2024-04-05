using Pool.BasicPool;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public interface IPoolable
{
    void OnActivate();
    void OnDeactivate();
}
public interface IObjectPool<T> where T : MonoBehaviour, IPoolable
{
    T Get();
    void Return(T obj);
    IEnumerator CheckUnusedObjectsCoroutine();
}
// 通用对象池类
public class ObjectPool<T>: MonoBehaviour, IObjectPool<T> where T : MonoBehaviour, IPoolable
{
    private readonly T _prefab;
    private readonly Queue<T> _pool;
    private readonly Transform _parent;
    private readonly int _initialSize=50;//对象池初始大小
    private readonly int _maxSize; // 对象池最大容量
    private readonly float _unUsedObjectLifetime; // 未使用对象的生存时间
    private readonly Dictionary<T, float> _unUsedObjects; // 记录未使用对象的字典

    /// <summary>
    /// 预制体，最大容量，父物体,对象未使用生存时间
    /// </summary>
    /// <param name="prefab"></param>
    /// <param name="initialSize"></param>
    /// <param name="parent"></param>
    public  ObjectPool(T prefab, int maxSize, Transform parent, float unUsedObjectLifetime)
    {
        _prefab = prefab;
        _pool = new Queue<T>(_initialSize);
        _parent = parent;
        _maxSize=maxSize;
        _unUsedObjectLifetime=unUsedObjectLifetime;
        _unUsedObjects = new Dictionary<T, float>();

        for (int i = 0; i < _initialSize; i++)
        {
            T obj = Instantiate(_prefab, _parent);
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>(); //颜色
            spriteRenderer.color = UnityEngine.Random.ColorHSV();
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
    /// <summary>
    /// 将对象从对象池提取出来和创建对象在对象池的虚拟方法
    /// </summary>
    /// <returns></returns>
    public virtual T Get()
    {
        if (_pool.Count > 0)
        {
            T obj = _pool.Dequeue();
            obj.gameObject.SetActive(true);
            obj.OnActivate();
            _unUsedObjects.Remove(obj); // 从未使用对象字典中移除
            return obj;
        }
        else if(_pool.Count + _unUsedObjects.Count < _maxSize)
        {
            T obj = Instantiate(_prefab, _parent);
            obj.OnActivate();
            return obj;
        }
        else
        {
            Debug.LogWarning("对象池已达到最大容量,无法创建新对象!");
            return null;
        }
    }
    /// <summary>
    /// 将对象返回到对象池的虚拟方法
    /// </summary>
    /// <param name="obj"></param>
    public virtual void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        obj.OnDeactivate();
        _pool.Enqueue(obj);
        _unUsedObjects[obj] = Time.time; // 将对象标记为未使用,并记录时间
    }
    /// <summary>
    /// 实现对象池动态删减功能的协程
    /// </summary>
    /// <returns></returns>
    public IEnumerator CheckUnusedObjectsCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            List<T> objectsToRemove = new List<T>();

            foreach (var pair in _unUsedObjects)
            {
                T obj = pair.Key;
                float lastUsedTime = pair.Value;

                if (Time.time - lastUsedTime >= _unUsedObjectLifetime)
                {
                    objectsToRemove.Add(obj);
                }
            }

            foreach (T obj in objectsToRemove)
            {
                _unUsedObjects.Remove(obj);
                Destroy(obj.gameObject);
            }
        }
    }
}

