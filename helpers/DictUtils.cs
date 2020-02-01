using System.Collections.Generic;
using Godot.Collections;

namespace Refactorio.helpers
{
    public static class DictUtils
    {
        public static T GetFromDict<K, T>(IDictionary<K, T> dict, K key, T @default)
        {
            return dict.TryGetValue(key, out var result) ? result : @default;
        }
    }
}