using System.Collections.Generic;

namespace DIVE_Utilities
{
    public class PrefabPool<T> where T : class?
    {
        readonly int _poolSize;

        LinkedList<T> _prefabsPool = new LinkedList<T>();

        public PrefabPool(int poolSize)
        {
            _poolSize = poolSize;
        }

        public void Add(T prefab)
        {
            if (prefab == null || _prefabsPool.Count > _poolSize)
                return;

            _prefabsPool.AddLast(prefab);
        }

        public T Get()
        {
            if (_prefabsPool.Count == 0)
                return null;

            var prefab = _prefabsPool.First;
            _prefabsPool.Remove(prefab);

            return prefab.Value;
        }
    }
}
