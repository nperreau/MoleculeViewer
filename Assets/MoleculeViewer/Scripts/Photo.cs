using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using SimpleFileBrowser;
using UnityEngine;

namespace MoleculeViewer
{
    public class Photo : MonoBehaviour
    {
        public List<GameObject> objectsToHide;

        public void TakePhoto()
        {
            FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".png"));
            FileBrowser.SetDefaultFilter(".png");

            FileBrowser.ShowSaveDialog(PerformTakePhoto, null, pickMode: FileBrowser.PickMode.Files);
        }

        private void PerformTakePhoto([NotNull] string[] paths) => StartCoroutine(PhotoRoutine(paths.First()));

        private IEnumerator PhotoRoutine([NotNull] string path)
        {
            try
            {
                foreach (GameObject go in objectsToHide.Where(go => go))
                    go.SetActive(false);

                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot(path);
            }
            finally
            {
                foreach (GameObject go in objectsToHide.Where(go => go))
                    go.SetActive(true);
            }
        }
    }
}
