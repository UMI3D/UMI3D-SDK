
using UnityEngine;
using UnityEngine.SceneManagement;

public class Home : MonoBehaviour {

    public void LoadScene(int number)
    {
        SceneManager.LoadScene(number);
    }

}