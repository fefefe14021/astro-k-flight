using System;
using UnityEngine;

namespace KANYE.UI
{
    public class MenuPanelSwitcher : MonoBehaviour
    {
        [Serializable]
        public class PanelEntry
        {
            public string id;
            public GameObject panel;
        }

        [SerializeField] private PanelEntry[] panels;
        [SerializeField] private string startPanelId = "main";

        private void Start()
        {
            if (!string.IsNullOrEmpty(startPanelId))
                Show(startPanelId);
        }

        public void Show(string panelId)
        {
            if (panels == null)
                return;

            for (var i = 0; i < panels.Length; i++)
            {
                if (panels[i].panel != null)
                    panels[i].panel.SetActive(panels[i].id == panelId);
            }
        }
    }
}
