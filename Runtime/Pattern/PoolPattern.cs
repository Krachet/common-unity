using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Com.Krackhet.Runtime.Pattern.Pooling
{
    public class PoolPattern
    {
        private GameObject prefab;
        private List<GameObject> active;
        private Queue<GameObject> inactive;
        public string Group { get; private set; }
        public PoolPattern(GameObject prefab, string group)
        {
            active = new List<GameObject>();
            inactive = new Queue<GameObject>();
            Group = string.IsNullOrWhiteSpace(group) ? prefab.name : group;
            this.prefab = prefab;
        }
        public void Despawn(GameObject gameObject)
        {
            gameObject.transform.SetParent(ObjectPool.GetHolder(Group));
            gameObject.SetActive(false);
            active.Remove(gameObject);
            inactive.Enqueue(gameObject);
        }
        public void DespawnAll()
        {
            Transform holder = ObjectPool.GetHolder(Group);
            foreach (GameObject gameObject in active)
            {
                gameObject.transform.SetParent(holder);
                gameObject.SetActive(false);
                inactive.Enqueue(gameObject);
            }
            active.Clear();
        }
        public void Destroy()
        {
            DespawnAll();
            foreach (GameObject gameObject in inactive)
                Object.Destroy(gameObject);
            prefab = null;
            inactive.Clear();
        }
        public GameObject Spawn()
        {
            GameObject gameObject;
            if (inactive.Count > 0)
            {
                gameObject = inactive.Dequeue();
                gameObject.SetActive(true);
                gameObject.transform.SetParent(null);
            }
            else
            {
                gameObject = Object.Instantiate(prefab);
                gameObject.name = prefab.name;
            }
            active.Add(gameObject);
            return gameObject;
        }
    }
    public static class ObjectPool
    {
        private static Dictionary<string, PoolPattern> pools = new Dictionary<string, PoolPattern>();
        private static Dictionary<string, Transform> holders = new Dictionary<string, Transform>();
        private static PoolPattern GetPool(GameObject prefab, string group = default)
        {
            if (prefab == null) return null;
            string poolName = prefab.name;
            if (!pools.ContainsKey(poolName))
            {
                PoolPattern pool = new PoolPattern(prefab, group);
                pools.Add(poolName, pool);
            }
            return pools[poolName];
        }
        public static Transform GetHolder(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName)) return null;
            if (!holders.ContainsKey(groupName))
            {
                GameObject holder = new GameObject(groupName);
                holders.Add(groupName, holder.transform);
            }
            return holders[groupName];
        }
        public static T Spawn<T>(GameObject prefab, string group = default) where T : Component
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity, null, group).GetComponent<T>();
        }
        public static T Spawn<T>(GameObject prefab, Transform parent, string group = default) where T : Component
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity, parent, group).GetComponent<T>();
        }
        public static T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation, string group = default) where T : Component
        {
            return Spawn(prefab, position, rotation, null, group).GetComponent<T>();
        }
        public static T Spawn<T>(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, string group = default) where T : Component
        {
            return Spawn(prefab, position, rotation, parent, group).GetComponent<T>();
        }
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent, string group = default)
        {
            PoolPattern pool = GetPool(prefab, group);
            GameObject clone = pool.Spawn();
            clone.transform.position = position;
            clone.transform.rotation = rotation;
            if (parent != null) clone.transform.SetParent(parent);
            return clone;
        }
        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, string group = default)
        {
            return Spawn(prefab, position, rotation, null, group);
        }
        public static GameObject Spawn(GameObject prefab, Transform parent, string group = default)
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity, parent, group);
        }
        public static GameObject Spawn(GameObject prefab, string group = default)
        {
            return Spawn(prefab, Vector3.zero, Quaternion.identity, null, group);
        }
        public static void Despawn(GameObject gameObject)
        {
            if (gameObject == null) return;
            GetPool(gameObject).Despawn(gameObject);
        }
        public static void DespawnAll()
        {
            foreach (KeyValuePair<string, PoolPattern> pool in pools)
                pool.Value.DespawnAll();
        }
        public static void DespawnPool(GameObject gameObject)
        {
            if (gameObject == null) return;
            if (pools.TryGetValue(gameObject.name, out PoolPattern pool))
                pool.DespawnAll();
            else Despawn(gameObject);
        }
        public static void DespawnPoolGroup(string group)
        {
            foreach (KeyValuePair<string, PoolPattern> pool in pools)
                if (pool.Value.Group.Equals(group)) pool.Value.DespawnAll();
        }
        public static void DestroyPool(GameObject gameObject)
        {
            if (gameObject == null) return;
            string poolName = gameObject.name;
            if (pools.ContainsKey(poolName))
            {
                pools[poolName].Destroy();
                if (pools[poolName].Group.Equals(poolName))
                {
                    Object.Destroy(holders[poolName].gameObject);
                    holders.Remove(poolName);
                }
                pools.Remove(poolName);
            }
            else Object.Destroy(gameObject);
        }
        public static void DestroyPoolGroup(string group)
        {
            if (!holders.ContainsKey(group)) return;
            foreach (KeyValuePair<string, PoolPattern> pool in pools)
                if (pool.Value.Group.Equals(group)) pool.Value.Destroy();
            Object.Destroy(holders[group].gameObject);
            holders.Remove(group);
        }
        public static void DestroyAll()
        {
            foreach (KeyValuePair<string, PoolPattern> pool in pools) pool.Value.Destroy();
            foreach (KeyValuePair<string, Transform> holder in holders) Object.Destroy(holder.Value.gameObject);
            pools.Clear();
            holders.Clear();
        }
    }
}