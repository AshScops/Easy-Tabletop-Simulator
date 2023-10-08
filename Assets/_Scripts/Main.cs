using UnityEngine;
using UnityEngine.SceneManagement;

namespace Tabletop
{
    public class Main : MonoBehaviour
    {
        public bool showOptions = false;

        private void OnGUI() 
        {
            GUILayout.BeginArea(new Rect(0, 200, Screen.width, Screen.height));
            //if (GUI.Button(new Rect(20, 40, 100, 20), "���ֽ̳�"))
            //{
            //    SceneManager.LoadScene("Practice");
            //}

            if (!showOptions && GUI.Button(new Rect(20, 60, 100, 20), "����"))
            {
                showOptions = true;
            }

            if (showOptions && GUI.Button(new Rect(20, 80, 100, 20), "�����˻�����"))
            {
                showOptions = false;
                SceneManager.LoadScene("Practice");
            }
            if (showOptions && GUI.Button(new Rect(20, 100, 100, 20), "�������˶���"))
            {
                showOptions = false;
                SceneManager.LoadScene("Offline");
            }
            if (showOptions && GUI.Button(new Rect(20, 120, 100, 20), "����"))
            {
                showOptions = false;
            }
            GUILayout.EndArea();
        }

        
    }
}