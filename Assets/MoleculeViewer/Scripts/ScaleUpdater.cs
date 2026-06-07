using System;
using UnityEngine;
using UnityEngine.UI;

namespace MoleculeViewer
{
    public class ScaleUpdater : MonoBehaviour
    {
        public RDKit3DGenerator Generator;
        public Slider Slider;

        private void Start()
        {
            UpdateScale();
        }

        public void UpdateScale()
        {
            Generator.SetAtomScale(Slider.value);
        }
    }
}
