// Minimal stand-ins for running the actual inventory/crafting C# outside Unity.
// These do not test Unity lifecycle, scene bindings, or UI behavior.
using System;

namespace UnityEngine
{
    public class MonoBehaviour { }
    public class ScriptableObject { }
    public class Sprite { }
    public class GameObject { }
    public class SerializeField : Attribute { }
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
    }
    public static class Debug
    {
        public static void Log(object message) { }
    }
}
