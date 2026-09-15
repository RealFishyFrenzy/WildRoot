// Minimal stand-ins for running the actual inventory/crafting C# outside Unity.
// These do not test Unity lifecycle, scene bindings, or UI behavior.
using System;

namespace UnityEngine
{
    public class MonoBehaviour
    {
        public Transform transform = new Transform();
        public GameObject gameObject = new GameObject();
        protected Coroutine StartCoroutine(System.Collections.IEnumerator routine) => new Coroutine();
        protected void StopCoroutine(Coroutine routine) { }
        protected void Destroy(GameObject target) { }
        protected T Instantiate<T>(T original, Vector3 position, Quaternion rotation) where T : new() => new T();
    }
    public class Transform { public Quaternion localRotation; }
    public class Coroutine { }
    public struct Vector3 { }
    public struct Quaternion
    {
        public static Quaternion identity => new Quaternion();
        public static Quaternion Euler(float x, float y, float z) => new Quaternion();
        public static Quaternion operator *(Quaternion a, Quaternion b) => a;
    }
    public static class Time { public static float deltaTime; }
    public static class Random { public static float value; }
    public class ScriptableObject { }
    public class Sprite { }
    public class GameObject
    {
        public string name;
        public object component;
        public T GetComponentInParent<T>() where T : class => component as T;
    }
    public class SerializeField : Attribute { }
    public class MinAttribute : Attribute { public MinAttribute(int minimum) { } }
    public class HeaderAttribute : Attribute
    {
        public HeaderAttribute(string text) { }
    }
    public class CreateAssetMenuAttribute : Attribute
    {
        public string fileName;
        public string menuName;
    }
    public static class Mathf
    {
        public static int Min(int a, int b) => Math.Min(a, b);
        public static float Sin(float value) => (float)Math.Sin(value);
    }
    public static class Debug
    {
        public static void Log(object message) { }
    }
}

// Terrain/drop engine boundary only; tests execute the real TerrainToolSystem.
public class TerrainManager
{
    public static TerrainManager Instance;
    public TerrainType terrain;
    public TerrainType GetTerrainType(UnityEngine.Vector3 position) => terrain;
}
public class WorldItemDrop
{
    public static int Spawned;
    public void Setup(ItemData item, int amount) { Spawned += amount; }
}
