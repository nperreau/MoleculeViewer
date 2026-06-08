using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using SimpleFileBrowser;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace MoleculeViewer
{
    public class Photo : MonoBehaviour
    {
        public List<GameObject> objectsToHide;

        public bool useTransparentBackground = false;

        [field: SerializeField]
        private Camera camera;

        public Camera Camera
        {
            get => camera ? camera : Camera.main;
            set => camera = value;
        }

        public static Photo Current { get; private set; }

        private void Awake()
        {
            if (Current)
                Debug.LogWarning($"There is more than one {GetType().Name} in the scene!");

            Current = this;
        }

        public void TakePhoto()
        {
            HideObjects();

            FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".png"));
            FileBrowser.SetDefaultFilter(".png");

            FileBrowser.ShowSaveDialog(PerformTakePhoto, ShowObjects, pickMode: FileBrowser.PickMode.Files, initialFilename:$"{MoleculeNameLoader.Current.CurrentMoleculeName}_{System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss", CultureInfo.InvariantCulture)}.png");
        }

        private void PerformTakePhoto([NotNull] string[] paths) => StartCoroutine(PhotoRoutine(paths.First()));

        private void HideObjects()
        {
            foreach (GameObject go in objectsToHide.Where(go => go))
                go.SetActive(false);
        }

        private void ShowObjects()
        {
            foreach (GameObject go in objectsToHide.Where(go => go))
                go.SetActive(true);
        }

        private IEnumerator PhotoRoutine([NotNull] string path)
        {
            try
            {
                if (useTransparentBackground)
                    throw new NotImplementedException();

                // 1. Wait for the end of the frame to ensure everything is stable
                yield return new WaitForEndOfFrame();

                int width = Screen.currentResolution.width;
                int height = Screen.currentResolution.height;

                // 2. Create high-precision RenderTexture for Unity 6 HDR pipeline
                RenderTexture rt = new RenderTexture(width, height, 32, RenderTextureFormat.ARGBHalf);

                // 3. UNITY 6 RENDER REQUEST: Instantiate the request properly from UniversalRenderPipeline
                UniversalRenderPipeline.SingleCameraRequest requestData = new UniversalRenderPipeline.SingleCameraRequest
                {
                    destination = rt
                };

                // Check if the current pipeline supports it and submit
                if (RenderPipeline.SupportsRenderRequest(Camera, requestData))
                    RenderPipeline.SubmitRenderRequest(Camera, requestData);
                else
                    Debug.LogError("Render Request not supported by the active pipeline.");

                // 4. Read the pixels out of the render texture
                RenderTexture.active = rt;
                Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGBAHalf, false);
                screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                screenShot.Apply();

                // --- NEW: TONEMAPPING & COLOR SPACE CORRECTION ---
                Color[] pixels = screenShot.GetPixels();
                for (int i = 0; i < pixels.Length; i++)
                {
                    // 1. Convert the color data from Linear (HDR pipeline space) to Gamma (Screen space)
                    // This instantly restores the punchy intensity of the Bloom/Glow!
                    Color gammaColor = pixels[i].gamma;

                    // 2. Combine the corrected colors with the original alpha transparency channel
                    pixels[i] = new Color(gammaColor.r, gammaColor.g, gammaColor.b, pixels[i].a);
                }
                screenShot.SetPixels(pixels);
                screenShot.Apply();

                // 5. Clean up the camera state immediately
                RenderTexture.active = null;
                Destroy(rt);

                // 6. Encode and save
                byte[] bytes = screenShot.EncodeToPNG();
                File.WriteAllBytes(path, bytes);

                Debug.Log($"Beautiful post-processed transparent screenshot saved via Unity 6 Render Request at: {path}");
                Destroy(screenShot);
            }
            finally
            {
                ShowObjects();
            }
        }
    }
}