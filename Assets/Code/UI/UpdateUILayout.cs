using UnitySystemFramework.Core;
using UnityEngine;
using UnityEngine.UI;

public class UpdateUILayout : MonoBehaviour
{
    public RectTransform LayoutTransform;

    private void OnEnable()
    {
        Game.CurrentGame.QueueUpdate(() =>
        {
            if (LayoutTransform)
                LayoutRebuilder.ForceRebuildLayoutImmediate(LayoutTransform);

            return false;
        });
    }
}
