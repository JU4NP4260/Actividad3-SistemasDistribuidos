using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.Events;
using Firebase.Auth;

public class FirebaseAuth_manager : MonoBehaviour
{
    [Header("Player Auth parameters")]
    [SerializeField] private TMP_InputField inputUsername;
    [SerializeField] private TMP_InputField inputPassword;

    [Header("Player info display")]
    [SerializeField] private TextMeshProUGUI usernameDisplay;
    [SerializeField] private TextMeshProUGUI highScoreDisplay;
    [SerializeField] private TextMeshProUGUI scoreDisplay;

    [Header("Leaderboard")]
    [SerializeField] private List<TextMeshProUGUI> leaderboard_usernames;
    [SerializeField] private List<TextMeshProUGUI> leaderboard_scores;


    public int currentScore;
    public int downloadedScore;

    public UnityEvent startGame = new UnityEvent();

    private string url = "https://sid-restapi.onrender.com";

    //void Start()
    //{
    //    Token = PlayerPrefs.GetString("token");
    //    if (string.IsNullOrEmpty(Token))
    //    {
    //        Debug.Log("there is no token");
    //    }
    //    else
    //    {
    //        Username = PlayerPrefs.GetString("username");
    //        StartCoroutine(GetProfile());
    //    }
    //}

    private void Update()
    {
        scoreDisplay.text = currentScore.ToString();
    }

    private IEnumerator RegisterUser()
    {
        var auth = FirebaseAuth.DefaultInstance;
        var singupTask = auth.CreateUserWithEmailAndPasswordAsync(inputUsername.text, inputPassword.text);

        yield return new WaitUntil(() => singupTask.IsCompleted);

        if (singupTask.IsCanceled)
        {
            Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
        }
        else if (singupTask.IsFaulted)
        {
            Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + singupTask.Exception);
        }
        else
        {
            //Firebase fue creado
            Firebase.Auth.AuthResult result = singupTask.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

        }
    }


    public void RegisterUser_Btn()
    {
        StartCoroutine(RegisterUser());
    }
}

//[System.Serializable]
//public class AuthenticationData
//{
//    public string username;
//    public string password;
//    public UsersJson usuario;
//    public string token;
//    public UsersList[] usuarios;
//}

//[System.Serializable]
//public class UsersList
//{
//    public UsersJson[] usuarios;
//}

//[System.Serializable]
//public class UsersJson
//{
//    public string _id;
//    public string username;
//    public DataUser data;
//}

//[System.Serializable]
//public class DataUser
//{
//    public int score;
//}

