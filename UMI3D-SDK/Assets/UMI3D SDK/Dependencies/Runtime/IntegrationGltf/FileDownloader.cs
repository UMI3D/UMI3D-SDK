using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace UnityGLTF.Loader
{
    class MyWebClient : WebClient
    {
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            return request;
        }
    }

    public class FileDownloader : IDataLoader
    {
        private readonly string filesroot;
        private readonly Uri baseAddress;
        public string query;

        public FileDownloader(string uri, string downloadedFilesroot)
        {
            filesroot = downloadedFilesroot;
            baseAddress = new Uri(uri);
        }

        public async Task<Stream> LoadStreamAsync(string gltfFilePath)
        {
            MyWebClient client = new MyWebClient();
            Uri uri = new Uri(baseAddress, gltfFilePath + query);
            string file = Path.Combine(filesroot, URIHelper.GetFileFromUri(uri));
            client.DownloadFileAsync(uri, file);
            return File.OpenRead(file);
        }
    }
}
