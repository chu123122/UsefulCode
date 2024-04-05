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
// ͨ�ö������
public class ObjectPool<T>: MonoBehaviour, IObjectPool<T> where T : MonoBehaviour, IPoolable
{
    private readonly T _prefab;
    private readonly Queue<T> _pool;
    private readonly Transform _parent;
    private readonly int _initialSize=50;//����س�ʼ��С
    private readonly int _maxSize; // ������������
    private readonly float _unUsedObjectLifetime; // δʹ�ö��������ʱ��
    private readonly Dictionary<T, float> _unUsedObjects; // ��¼δʹ�ö�����ֵ�

    /// <summary>
    /// Ԥ���壬���������������,����δʹ������ʱ��
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
            SpriteRenderer spriteRenderer = obj.GetComponent<SpriteRenderer>(); //��ɫ
            spriteRenderer.color = UnityEngine.Random.ColorHSV();
            obj.gameObject.SetActive(false);
            _pool.Enqueue(obj);
        }
    }
    /// <summary>
    /// ������Ӷ������ȡ�����ʹ��������ڶ���ص����ⷽ��
    /// </summary>
    /// <returns></returns>
    public virtual T Get()
    {
        if (_pool.Count > 0)
        {
            T obj = _pool.Dequeue();
            obj.gameObject.SetActive(true);
            obj.OnActivate();
            _unUsedObjects.Remove(obj); // ��δʹ�ö����ֵ����Ƴ�
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
            Debug.LogWarning("������Ѵﵽ�������,�޷������¶���!");
            return null;
        }
    }
    /// <summary>
    /// �����󷵻ص�����ص����ⷽ��
    /// </summary>
    /// <param name="obj"></param>
    public virtual void Return(T obj)
    {
        obj.gameObject.SetActive(false);
        obj.OnDeactivate();
        _pool.Enqueue(obj);
        _unUsedObjects[obj] = Time.time; // ��������Ϊδʹ��,����¼ʱ��
    }
    /// <summary>
    /// ʵ�ֶ���ض�̬ɾ�����ܵ�Э��
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

