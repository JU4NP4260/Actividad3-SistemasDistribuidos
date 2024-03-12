using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.Events;

public class HttpsManager : MonoBehaviour
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

    void Start()
    {
        Token = PlayerPrefs.GetString("token");
        if (string.IsNullOrEmpty(Token))
        {
            Debug.Log("there is no token");
        }
        else
        {
            Username = PlayerPrefs.GetString("username");
            StartCoroutine(GetProfile());
        }
    }

    private void Update()
    {
        scoreDisplay.text = currentScore.ToString();
    }

    public void sendRegister()
    {
        AuthenticationData data = new AuthenticationData();

        data.username = inputUsername.text;
        data.password = inputPassword.text;
        StartCoroutine(Register(JsonUtility.ToJson(data)));
    }

    public void sendLogin()
    {
        AuthenticationData data = new AuthenticationData();

        data.username = inputUsername.text;
        data.password = inputPassword.text;

        StartCoroutine(Login(JsonUtility.ToJson(data)));
    }

    IEnumerator Register(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/api/usuarios", json);
        request.method = "POST";
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                Debug.Log("Sucessful register");
                StartCoroutine(Login(json));
            }
            else
            {
                Debug.Log(request.responseCode + "|" + request.error);
            }
        }
    }
    IEnumerator Login(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/api/auth/login", json);
        request.method = "POST";
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthenticationData data = JsonUtility.FromJson<AuthenticationData>(request.downloadHandler.text);
                Token = data.token;
                Username = data.usuario.username;
                PlayerPrefs.SetString("username", Username);
                PlayerPrefs.SetString("token", Token);
                Debug.Log(data.token);
                startGame.Invoke();
            }
            else
            {
                Debug.Log(request.responseCode + "|" + request.error);
            }
        }
    }

    public string Username { get; set; }

    public string Token { get; set; }

    IEnumerator GetProfile()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/api/usuarios/" + Username);
        //Debug.Log("Sending Request GetProfile");
        request.SetRequestHeader("x-token", Token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthenticationData data = JsonUtility.FromJson<AuthenticationData>(request.downloadHandler.text);
                Debug.Log(data.usuario.username + " has been verified \n Score:" + data.usuario.data.score);

                usernameDisplay.text = Username;
                downloadedScore = data.usuario.data.score;
                highScoreDisplay.text = downloadedScore.ToString();

                //Game Start Logic
                startGame.Invoke();
                
            }
            else
            {
                //Debug.Log(request.responseCode + "|" + request.error);
                Debug.Log("User has not been Authentified");
            }
        }
    }

    private IEnumerator SendData(string json)
    {
        UnityWebRequest request = UnityWebRequest.Put(url + "/api/usuarios", json);
        request.method = "PATCH";
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-token", Token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                AuthenticationData data = JsonUtility.FromJson<AuthenticationData>(request.downloadHandler.text);
                Debug.Log(data.usuario.username + " has been verified \n new Score: " + data.usuario.data.score);
            }
            else
            {
                Debug.Log("User has not been Authentified");
            }

        }
    }

    public void LogOut()
    {
        PlayerPrefs.SetString("username", null);
        PlayerPrefs.SetString("token", null);
    }



    //Events methods

    public void Event_SendData()
    {
        UsersJson currUser = new UsersJson();
        currUser.data = new DataUser();
        currUser.username = Username;
        currUser.data.score = downloadedScore;

        if(currentScore > currUser.data.score)
        {
            currUser.data.score = currentScore;

            StartCoroutine(SendData(JsonUtility.ToJson(currUser)));
        }
    }

    public void Event_Leaderboard()
    {
        StartCoroutine(SetLeaderBoard());
    }

    public void Event_GetUser()
    {
        StartCoroutine(GetProfile());
    }

    IEnumerator SetLeaderBoard()
    {
        UnityWebRequest request = UnityWebRequest.Get(url + "/api/usuarios");
        request.SetRequestHeader("x-token", Token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.Log(request.error);
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            if (request.responseCode == 200)
            {
                UsersList data = JsonUtility.FromJson<UsersList>(request.downloadHandler.text);

                //Debug.Log("List: " + usersList.users[1]);

                //UsersJson[] sortedUsers = new UsersJson[0];
                //for (int i = 0; i<sortedUsers.Length; i++)
                //{
                //    sortedUsers[i] = new UsersJson();
                //    Debug.Log(sortedUsers[i]);
                //}

                //sortedUsers
                var userList = data.users.OrderByDescending(u => u.data.score).Take(9).ToArray();
                CreateLeaderboard(userList);
            }
            else
            {
                //Debug.Log(request.responseCode + "|" + request.error);
                Debug.Log("User has not been Authentified");
            }
        }
    }

    public void CreateLeaderboard(UsersJson[] users)
    {
        //for (int i = 0; i < users.Length; i++)
        //{
        //    if ((users[i].username == null))
        //    {
        //        users[i].username = "No Name";
        //        users[i].data.score = 0;
        //        leaderboard_usernames[i].text = users[i].username;
        //        leaderboard_scores[i].text = users[i].data.score.ToString();
        //    }
        //    else
        //    {
        //        leaderboard_usernames[i].text = users[i].username;
        //        leaderboard_scores[i].text = users[i].data.score.ToString();
        //    }
        //}

        for(int i = 0; i < users.Length; i++)
        {

            leaderboard_usernames[i].text = users[i].username;
            leaderboard_scores[i].text = users[i].data.score.ToString();
        }
    }
}

[System.Serializable]
public class AuthenticationData
{
    public string username;
    public string password;
    public UsersJson usuario;
    public string token;
}
[System.Serializable]
public class UsersJson
{
    public string _id;
    public string username;
    public DataUser data;
}

[System.Serializable]
public class DataUser
{
    public int score;
}

[System.Serializable]
public class UsersList
{
    public UsersJson[] users;
}