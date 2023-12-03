using UnityEngine;

/// <summary>
/// 高亮的3030, 指示器:3020, msak:3010, 正常:3000
/// </summary>
public class ModelRenderQueueHelper : MonoBehaviour
{
    private MaterialItem[] m_MaterialItems;
    private void OnEnable()
    {
        SkinnedMeshRenderer[] skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        m_MaterialItems = new MaterialItem[skinnedMeshRenderers.Length + spriteRenderers.Length];
        if (m_MaterialItems.Length > 0)
        {
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                MaterialItem materialItem = new MaterialItem(skinnedMeshRenderers[i].material);
                m_MaterialItems[i] = materialItem;
            }
            int si = skinnedMeshRenderers.Length > 0 ? m_MaterialItems.Length - 1 : 0;
            for (int i = si; i < spriteRenderers.Length + si; i++)
            {
                MaterialItem materialItem = new MaterialItem(spriteRenderers[i - si].material);
                m_MaterialItems[i] = materialItem;
            }
        }
    }

    public void ToRenderQueue(int toRenderQueue)
    {
        if (m_MaterialItems != null && m_MaterialItems.Length > 0)
        {
            foreach (var item in m_MaterialItems)
            {
                item.ToRenderQueue(toRenderQueue);
            }
        }
    }

    public void BackRenderQueue()
    {
        if (m_MaterialItems != null && m_MaterialItems.Length > 0)
        {
            foreach (var item in m_MaterialItems)
            {
                item.BackRenderQueue();
            }
        }
    }

    private class MaterialItem
    {
        public int oriRenderQueue;
        public Material material;
        public MaterialItem(Material material)
        {
            this.material = material;
            oriRenderQueue = material.renderQueue;
        }

        public void ToRenderQueue(int toRenderQueue)
        {
            material.renderQueue = toRenderQueue;
        }

        public void BackRenderQueue()
        {
            material.renderQueue = oriRenderQueue;
        }
    }
}
