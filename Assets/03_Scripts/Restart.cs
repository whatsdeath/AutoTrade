using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart : MonoBehaviour
{
    private void Start()
    {
        StartCoroutine(RestartDelay());
    }

    private IEnumerator RestartDelay()
    {
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene("AutoTrade", LoadSceneMode.Single);
    }
}
