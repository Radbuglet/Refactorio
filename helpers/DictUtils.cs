using System.Collections.Generic;
using Godot.Collections;

namespace Refactorio.helpers
{
    public static class DictUtils
    {
        public static void Update<K, T>(this IDictionary<K, T> dict, K key, T value)
        {
            dict.Remove(key);
            dict.Add(key, value);
        }
        public static T GetFromDict<K, T>(IDictionary<K, T> dict, K key, T @default)
        {
            return dict.TryGetValue(key, out var result) ? result : @default;
        }
    }
}