using UnityEngine;
using UnityEngine.UI;

namespace KANYE.UI
{
    public class CrispLabel : MonoBehaviour
    {
        public int designSize = 16;

        private void Start()
        {
            Apply();
        }

        private void Apply()
        {
            var label = GetComponent<Text>();
            if (label == null)
                return;

            if (label.font == null)
                label.font = UIFont.DefaultFont;

            label.fontSize = designSize;
            transform.localScale = Vector3.one;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;

            if (label.font != null && label.font.material != null && label.font.material.mainTexture != null)
                label.font.material.mainTexture.filterMode = FilterMode.Bilinear;
        }
    }
}
