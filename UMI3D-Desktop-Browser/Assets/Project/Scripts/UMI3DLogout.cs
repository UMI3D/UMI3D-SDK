using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UMI3DLogout : MonoBehaviour
{
    public string LoginScene;




    public void Logout() {
        umi3d.cdk.UMI3DBrowser.CloseEnvironment();
        SceneManager.LoadScene(LoginScene, LoadSceneMode.Single);
    }
}
