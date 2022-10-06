using UnitySystemFramework.Utility;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnitySystemFramework.Inputs
{
    public class KeyboardGenerator : MonoBehaviour
    {
        [Serializable]
        public class KeyRow
        {
            public KeyInfo[] Keys;
        }

        [Serializable]
        public class KeyInfo
        {
            public string Name;
            public bool Enabled = true;
            public float Width = 80;
            public string Key;
            public KeyCode Code;
            public float SpaceAfter;
        }

        [Serializable]
        public class KeyBinding
        {
            public string Label;
            public KeyCode Code;
        }

        public class KeyItem
        {
            public KeyInfo Key;
            public GameObject GameObject;
            public Button Button;
            public Text KeyText;
            public Text LabelText;
        }

        public GameObject RowPrefab;
        public GameObject KeyPrefab;
        public GameObject SpacerPrefab;
        public Vector2 Scale = Vector2.one;
        public KeyRow[] Rows;
        public KeyBinding[] Bindings;

#if UNITY_EDITOR
        [ContextMenu("Generate")]
        void GenerateKeys()
        {
            var keyLookup = new Dictionary<KeyCode, KeyItem>();
            for (var rowIdx = 0; rowIdx < Rows.Length; rowIdx++)
            {
                var row = Rows[rowIdx];
                var rowGO = Instantiate(RowPrefab, transform);
                rowGO.name = "Row" + (rowIdx + 1);
                var rowRect = rowGO.GetComponent<RectTransform>();
                rowRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, rowRect.rect.width * Scale.x);
                rowRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, rowRect.rect.height * Scale.y);
                foreach (var key in row.Keys)
                {
                    var item = new KeyItem();
                    item.GameObject = Instantiate(KeyPrefab, rowGO.transform);
                    item.GameObject.name = key.Code.ToString();
                    var itemRect = item.GameObject.GetComponent<RectTransform>();
                    itemRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, key.Width * Scale.x);
                    itemRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, itemRect.rect.height * Scale.y);
                    item.Button = item.GameObject.GetComponent<Button>();
                    item.Button.interactable = key.Enabled;

                    item.KeyText = item.GameObject.FindComponent<Text>("Key");
                    item.LabelText = item.GameObject.FindComponent<Text>("Label");
                    item.LabelText.text = "";

                    item.KeyText.text = key.Key;
                    keyLookup.Add(key.Code, item);

                    if (key.SpaceAfter > 0)
                    {
                        var spacer = Instantiate(SpacerPrefab, rowGO.transform);
                        var spacerRect = spacer.GetComponent<RectTransform>();
                        spacerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, key.SpaceAfter * Scale.x);
                        spacerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, spacerRect.rect.height * Scale.y);
                    }
                }
            }

            foreach (var binding in Bindings)
            {
                if (keyLookup.TryGetValue(binding.Code, out var item))
                {
                    item.LabelText.text = binding.Label;
                }
            }
        }
#endif
    }
}
