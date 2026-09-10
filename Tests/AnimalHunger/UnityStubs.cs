// Only the Unity runtime boundary is simulated; clock, animals, catching,
// animal inventory and enclosure code are compiled from the actual sources.
using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityEngine
{
    public class MonoBehaviour
    {
        public static readonly Dictionary<Type, object> Objects = new();
        private bool isEnabled = true;
        public bool enabled
        {
            get => isEnabled;
            set
            {
                if (isEnabled == value) return;
                isEnabled = value;
                GetType().GetMethod(value ? "OnEnable" : "OnDisable", BindingFlags.NonPublic | BindingFlags.Instance)
                    ?.Invoke(this, null);
            }
        }
        public GameObject gameObject = new();
        public static T FindAnyObjectByType<T>() where T : class => Objects.TryGetValue(typeof(T), out var item) ? item as T : null;
        public T GetComponent<T>() where T : class => FindAnyObjectByType<T>();
        public static void Destroy(object item) { }
    }
    public class ScriptableObject { }
    public class GameObject { }
    public class SerializeField : Attribute { }
    public class HeaderAttribute : Attribute { public HeaderAttribute(string value) { } }
    public class MinAttribute : Attribute { public MinAttribute(float value) { } }
    public class RangeAttribute : Attribute { public RangeAttribute(int min, int max) { } }
    public class CreateAssetMenuAttribute : Attribute { public string fileName; public string menuName; }
    public static class Mathf { public static float Clamp(float value, float min, float max) => Math.Clamp(value, min, max); }
    public static class Time { public static float deltaTime; }
    public static class Debug
    {
        public static void Log(object message, object context = null) { }
        public static void LogWarning(object message, object context = null) { }
    }
}
public interface IInteractable { void Interact(); }
public class EnclosureUI { public static EnclosureUI Instance; public void Open(AnimalEnclosure enclosure) { } }
public class ToolTarget : UnityEngine.MonoBehaviour
{
    protected ToolType requiredTool;
    protected virtual void Awake() { }
    public virtual bool UseTool(ToolType tool, int power) => false;
}
