using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// This script defines which sprite the 'Player" uses and its health.
/// </summary>

public class Player : MonoBehaviour
{
    public GameObject destructionFX;
    public HttpsManager httpsManager;

    public static Player instance; 

    private void Awake()
    {
        if (instance == null) 
            instance = this;
    }

    private void Start()
    {
        httpsManager = FindFirstObjectByType<HttpsManager>();
    }

    //method for damage proceccing by 'Player'
    public void GetDamage(int damage)   
    {
         StartCoroutine(Destruction());
    }    

    //'Player's' destruction procedure
    private IEnumerator Destruction()
    {
        httpsManager.Event_SendData();

        Instantiate(destructionFX, transform.position, Quaternion.identity); //generating destruction visual effect and destroying the 'Player' object

        Destroy(gameObject);
        yield return new WaitForSeconds(3);

        SceneManager.LoadScene(0);
    }
}
















