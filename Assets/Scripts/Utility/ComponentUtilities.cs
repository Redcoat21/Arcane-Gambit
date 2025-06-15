using System;
using UnityEngine;

namespace Utility
{
    public static class ComponentUtilities
    {
        public static void SafeAssign<T>(T component, Action<T> assignAction, string componentName, GameObject context) where T : class
        {
            if (component != null)
            {
                assignAction(component);
            }
            else
            {
                Debug.LogWarning($"{componentName} is not assigned for {context.name}", context);
            }
        }
    }
}
