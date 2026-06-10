using UnityEngine;
using UnityEngine.EventSystems;

public static class UIEventSystemHelper
{
    public static void Ensure()
    {
        EventSystem[] all = Object.FindObjectsOfType<EventSystem>();

        // 清理多余的 EventSystem
        if (all.Length > 1)
        {
            for (int i = 1; i < all.Length; i++)
            {
                RemoveEventSystem(all[i]);
            }
        }

        // 只留一个 EventSystem 时，清理它身上的多余 StandaloneInputModule
        if (all.Length >= 1)
        {
            CleanDuplicateInputModules(all[0]);
            return;
        }

        // 一个都没有 → 创建
        GameObject go = new GameObject("EventSystem");
        go.AddComponent<EventSystem>();
        go.AddComponent<StandaloneInputModule>();
        Debug.Log("[UIEventSystemHelper] Created EventSystem.");
    }

    private static void CleanDuplicateInputModules(EventSystem es)
    {
        StandaloneInputModule[] modules = es.GetComponents<StandaloneInputModule>();
        if (modules.Length > 1)
        {
            for (int i = 1; i < modules.Length; i++)
            {
                Object.Destroy(modules[i]);
                Debug.Log(string.Format("[UIEventSystemHelper] Removed duplicate StandaloneInputModule from {0}", es.gameObject.name));
            }
        }
    }

    private static void RemoveEventSystem(EventSystem es)
    {
        // 先删它身上的所有 StandaloneInputModule
        StandaloneInputModule[] modules = es.GetComponents<StandaloneInputModule>();
        foreach (var m in modules)
            Object.Destroy(m);

        // 再删 EventSystem 组件
        Object.Destroy(es);
        Debug.Log(string.Format("[UIEventSystemHelper] Removed EventSystem from {0}", es.gameObject.name));
    }
}
