using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.Events;

public class HttpsManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputUsername;
    [SerializeField] private TMP_InputField inputPassword;

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
                Debug.Log("Registro Exitoso");
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
                Debug.Log("El usuario " + data.usuario.username + " se encuentra autenticado y su puntaje es " + data.usuario.data.score);
                //Game Start Logic
                startGame.Invoke();

                UsuarioJson[] usuarios = new UsuarioJson[10];
                UsuarioJson[] usuariosOrganizados = usuarios.OrderByDescending(user => user.data.score).Take(7).ToArray();
            }
            else
            {
                //Debug.Log(request.responseCode + "|" + request.error);
                Debug.Log("Usuario no autenticado");
            }
        }
    }
}
[System.Serializable]
public class AuthenticationData
{
    public string username;
    public string password;
    public UsuarioJson usuario;
    public string token;
}
[System.Serializable]
public class UsuarioJson
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