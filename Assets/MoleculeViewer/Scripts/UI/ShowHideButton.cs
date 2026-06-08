using System.Collections.Generic;
using UnityEngine;

namespace MoleculeViewer
{
    public class HideShowObjects : MonoBehaviour
    {
        public List<GameObject> show;
        public List<GameObject> hide;

        public void HideAndShow()
        {
            foreach (var o in hide)
                o.SetActive(false);

            foreach (var o in show)
                o.SetActive(true);
        }
    }
}
