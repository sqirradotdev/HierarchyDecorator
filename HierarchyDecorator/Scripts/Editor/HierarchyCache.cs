﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HierarchyDecorator
{
    [InitializeOnLoad]
    public static class HierarchyCache
    {
        public class HierarchyData
        {
            public readonly int ID;
            public readonly Transform Transform;

            public bool Foldout { get; set; }

            public bool HasChildren => Transform.childCount > 0;

            public HierarchyData(Transform transform)
            {
                ID = transform.GetInstanceID();
                Transform = transform;
            }
        }

        public class SceneCache
        {
            // --- Fields

            private List<int> validIDs = new List<int>();
            private Dictionary<int, HierarchyData> Lookup = new Dictionary<int, HierarchyData>();

            // --- Properties

            public readonly Scene Scene; 

            public HierarchyData First { get; private set; }
            public HierarchyData Current { get; private set; }
            public HierarchyData Previous { get; private set; }

            // --- Creation

            public SceneCache(Scene scene)
            {
                Scene = scene;
            }

            // --- Methods

            public bool Add(Transform instance)
            {
                if (instance == null)
                {
                    Debug.Log("TODO: [Wooshii]");
                    return false;
                }

                HierarchyData data = new HierarchyData(instance);
                int id = data.ID;

                if (validIDs.Contains(id))
                {
                    return false;
                }

                Lookup.Add(id, data);

                // Update

                SetTarget(data);

                return true;
            }

            public bool Remove(Transform instance)
            {
                if (instance == null)
                {
                    Debug.Log("TODO: [Wooshii]");
                    return false;
                }

                int id = instance.GetInstanceID();

                if (!Lookup.ContainsKey(id))
                {
                    return false;
                }

                return Lookup.Remove(id);
            }

            public bool TryGetInstance(int id, out HierarchyData instance)
            {
                return Lookup.TryGetValue(id, out instance);
            }

            public void SetTarget(HierarchyData data)
            {
                if (data == null)
                {
                    Debug.LogWarning("TODO: [Wooshii]");
                }

                Previous = Current;
                Current = data;

                // Check foldout state for last instance

                if (Previous != null && Previous.HasChildren)
                {
                    Previous.Foldout = Current.Transform.parent == Previous.Transform;
                }

                // Refresh if at the start of the hierarchy

                Transform transform = data.Transform;

                if (First == data)
                {
                    Refresh();
                }

                if (transform.parent == null && transform.GetSiblingIndex() == 0)
                {
                    First = data;
                }

                validIDs.Add(data.ID);
            }

            public void SetTarget(Transform transform)
            {
                int id = transform.GetInstanceID();
                if (TryGetInstance(id, out HierarchyData data))
                {
                    SetTarget(data);
                    return;
                }

                Add(transform);
            }

            public void Clear()
            {
                Lookup.Clear();
            }

            private void Refresh()
            {
                List<int> invalidKeys = new List<int>();
                foreach (int key in Lookup.Keys)
                {
                    if (!validIDs.Contains(key))
                    {
                        invalidKeys.Add(key);
                    }
                }

                for (int i = 0; i < invalidKeys.Count; i++)
                {
                    Lookup.Remove(invalidKeys[i]);
                }

                validIDs.Clear();
            }
        }

        private static Dictionary<Scene, SceneCache> Scenes = new Dictionary<Scene, SceneCache>();

        public static SceneCache Target;

        static HierarchyCache() { }

        public static bool TryGetScene(Scene scene, out SceneCache cache)
        {
            return Scenes.TryGetValue(scene, out cache);
        }

        public static bool RegisterScene(Scene scene)
        {
            if (!scene.IsValid())
            {
                Debug.LogWarning("a");
                return false;
            }

            if (Exists(scene))
            {
                Debug.LogWarning("b");
                return false;
            }

            SceneCache cache = new SceneCache(scene);

            Scenes.Add(scene, cache);

            if (Target == null)
            {
                Target = cache;
            }

            return true;
        }

        public static bool Exists(Scene scene)
        {
            return Scenes.ContainsKey(scene);
        }

        public static bool IsTarget(Scene scene)
        {
            return Target.Scene.handle == scene.handle;
        }

        public static SceneCache SetTarget(Scene scene)
        {
            if (IsTarget(scene))
            {
                return Target;
            }

            if (!TryGetScene(scene, out SceneCache cache))
            {
                return Target;
            }

            SetTarget(cache);
            return Target;
        }

        public static void SetTarget(SceneCache cache)
        {
            if (cache == null)
            {
                Debug.LogError("a");
                return;
            }

            Target = cache;
        }
    }
}