using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
  public void Iniciar()
  {
    UnityEngine.SceneManagement.SceneManager.LoadScene("MenuOnline");
  }
    public void Sair()
    {
        Application.Quit();
    }

}
