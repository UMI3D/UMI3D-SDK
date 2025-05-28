using umi3d.common;
using UnityEngine.Networking;

namespace umi3d.cdk
{
    public static class LoaderUtils
    {
        /// <summary>
        /// Modifies <paramref name="webRequest"/> to add it an authorization token.
        /// </summary>
        /// <param name="webRequest"></param>
        /// <param name="autorization"></param>
        public static void SetWebRequestCertificate(UnityWebRequest webRequest, string fileAuthorization)
        {
            if (string.IsNullOrEmpty(fileAuthorization))
                return;

            webRequest.SetRequestHeader(UMI3DNetworkingKeys.Authorization, fileAuthorization);
        }
    }
}
