using UnityEngine;
using UnityEngine.SceneManagement;

namespace HardCodeLab.TutorialMaster.Demos.InventoryDemo
{
    public class SceneRestarter : MonoBehaviour
    {
        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}