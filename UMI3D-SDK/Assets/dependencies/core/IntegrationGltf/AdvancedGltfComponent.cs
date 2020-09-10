using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using UnityGLTF.Loader;

namespace UnityGLTF
{
    /// <summary>
    /// Component to load a GLTF scene with
    /// </summary>
    public class AdvancedGLTFComponent : MonoBehaviour
    {
        public string GLTFUri = null;
        public bool Multithreaded = true;
        public bool UseStream = false;
        public bool DownloadFile = false;
        public string DownloadFileRoot = "test.gltf";
        public string DownloadPath = null;
        public bool AppendStreamingAssets = true;
        public bool PlayAnimationOnLoad = true;

        public Action onLoaded;

        public ImporterFactory Factory = null;

        public IEnumerable<Animation> Animations { get; private set; }

        [SerializeField]
        private bool loadOnStart = true;

        [SerializeField] private bool MaterialsOnly = false;

        public int RetryCount = 10;
        public float RetryTimeout = 2.0f;
        private int numRetries = 0;


        public int MaximumLod = 300;
        public int Timeout = 8;
        public GLTFSceneImporter.ColliderType Collider = GLTFSceneImporter.ColliderType.None;
        public GameObject LastLoadedScene { get; private set; } = null;

        [SerializeField]
        private Shader shaderOverride = null;

        /*
        private async void Start()
        {
            if (!loadOnStart) return;

            try
            {
                await Load();
            }
#if WINDOWS_UWP
			catch (Exception)
#else
            catch (HttpRequestException)
#endif
            {
                if (numRetries++ >= RetryCount)
                    throw;

                Debug.LogWarning("Load failed, retrying: "+ GLTFUri);
                await Task.Delay((int)(RetryTimeout * 1000));
                Start();
            }
        }*/

        public async void LoadAndRetry()
        {
            try
            {
                await Load();
            }
#if WINDOWS_UWP
			catch (Exception)
#else
            catch (HttpRequestException)
#endif
            {
                if (numRetries++ >= RetryCount)
                    throw;

                Debug.LogWarning("Load failed, retrying");
                await Task.Delay((int)(RetryTimeout * 1000));
                LoadAndRetry();
            }
        }

        public async Task Load()
        {
            float startTime = Time.time;
            var importOptions = new ImportOptions
            {
                AsyncCoroutineHelper = gameObject.GetComponent<AsyncCoroutineHelper>() ?? gameObject.AddComponent<AsyncCoroutineHelper>(),
            };

            GLTFSceneImporter sceneImporter = null;
            try
            {
                Factory = Factory ?? ScriptableObject.CreateInstance<DefaultImporterFactory>();

                if (UseStream)
                {
                    string fullPath;
                    if (AppendStreamingAssets)
                    {
                        // Path.Combine treats paths that start with the separator character
                        // as absolute paths, ignoring the first path passed in. This removes
                        // that character to properly handle a filename written with it.
                        fullPath = Path.Combine(Application.streamingAssetsPath, GLTFUri.TrimStart(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }));
                    }
                    else
                    {
                        fullPath = GLTFUri;
                    }
                    string directoryPath = URIHelper.GetDirectoryName(fullPath);
                    importOptions.DataLoader = new FileLoader(directoryPath);
                    sceneImporter = Factory.CreateSceneImporter(
                        Path.GetFileName(GLTFUri),
                        importOptions
                        );
                }
                else if (DownloadFile)
                {
                    DownloadPath = DownloadPath == null ? Application.dataPath+"/../" : DownloadPath;
                    importOptions.DataLoader = new FileDownloader(GLTFUri,Path.Combine(DownloadPath, DownloadFileRoot));
                    sceneImporter = Factory.CreateSceneImporter(
                        Path.GetFileName(GLTFUri),
                        importOptions
                        );
                }
                else
                {
                    string directoryPath = URIHelper.GetDirectoryName(GLTFUri);
                    AdvancedWebRequestLoader loader = new AdvancedWebRequestLoader(directoryPath);
                    loader.query = (new Uri(GLTFUri)).Query;
                    importOptions.DataLoader = loader;
                    sceneImporter = Factory.CreateSceneImporter(
                        URIHelper.GetFileFromUri(new Uri(GLTFUri)),
                        importOptions
                        );
                }

                sceneImporter.SceneParent = gameObject.transform;
                sceneImporter.Collider = Collider;
                sceneImporter.MaximumLod = MaximumLod;
                sceneImporter.Timeout = Timeout;
                sceneImporter.IsMultithreaded = Multithreaded;
                sceneImporter.CustomShaderName = shaderOverride ? shaderOverride.name : null;

                if (MaterialsOnly)
                {
                    var mat = await sceneImporter.LoadMaterialAsync(0);
                    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.SetParent(gameObject.transform);
                    var renderer = cube.GetComponent<Renderer>();
                    renderer.sharedMaterial = mat;
                }
                else
                {
                    await sceneImporter.LoadSceneAsync();
                }

                // Override the shaders on all materials if a shader is provided
                if (shaderOverride != null)
                {
                    Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        renderer.sharedMaterial.shader = shaderOverride;
                    }
                }

                print("model loaded with vertices: " + sceneImporter.Statistics.VertexCount.ToString() + ", triangles: " + sceneImporter.Statistics.TriangleCount.ToString() + "  in " + (Time.time - startTime).ToString() +" secondes." );
                LastLoadedScene = sceneImporter.LastLoadedScene;

                Animations = sceneImporter.LastLoadedScene.GetComponents<Animation>();

                if (PlayAnimationOnLoad && Animations.Any())
                {
                    Animations.FirstOrDefault().Play();
                }

            }
            finally
            {
                if (onLoaded != null)
                    onLoaded.Invoke();

                if (importOptions.DataLoader != null)
                {
                    sceneImporter?.Dispose();
                    sceneImporter = null;
                    importOptions.DataLoader = null;
                }
            }
        }
    }
}
