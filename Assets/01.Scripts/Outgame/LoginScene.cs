using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginScene : MonoBehaviour
{
    // 로그인씬 (로그인/회원가입) -> 게임씬

    private enum SceneMode
    {
        Login,
        Register
    }
    
    private SceneMode _mode = SceneMode.Login;
    
    // 비밀번호 확인 오브젝트
    [SerializeField] private GameObject _passwordCofirmObject;
    [SerializeField] private Button _gotoRegisterButton;
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _gotoLoginButton;
    [SerializeField] private Button _registerButton;

    [SerializeField] private TextMeshProUGUI _messageTextUI;
    
    [SerializeField] private TMP_InputField _idInputField;
    [SerializeField] private TMP_InputField _passwordInputField;
    [SerializeField] private TMP_InputField _passwordConfirmInputField;
    
    private const string LastLoginIdKey = "LastLoginId";
    
    private void Start()
    {
        LoadLastLoginId();
        AddButtonEvents();
        Refresh();
    }

    private void AddButtonEvents()
    {
        _gotoRegisterButton.onClick.AddListener(GotoRegister);
        _loginButton.onClick.AddListener(Login);
        _gotoLoginButton.onClick.AddListener(GotoLogin);
        _registerButton.onClick.AddListener(Register);
    }

    private void Refresh()
    {
        // 2차 비밀번호 오브젝트는 회원가입 모드일때만 노출
        _passwordCofirmObject.SetActive(_mode == SceneMode.Register);
        _gotoRegisterButton.gameObject.SetActive(_mode == SceneMode.Login);
        _loginButton.gameObject.SetActive(_mode == SceneMode.Login);
        _gotoLoginButton.gameObject.SetActive(_mode == SceneMode.Register);
        _registerButton.gameObject.SetActive(_mode == SceneMode.Register);
    }

    private void Login()
    {
        // 로그인
        // 1. 아이디 입력을 확인한다.
        string id = _idInputField.text;
        if (string.IsNullOrEmpty(id))
        {
            _messageTextUI.text = "아이디를 입력해주세요.";
            return;
        }
        
        // 2. 비밀번호 입력을 확인한다.
        string password = _passwordInputField.text;
        if (string.IsNullOrEmpty(password))
        {
            _messageTextUI.text = "패스워드를 입력해주세요.";
            return;
        }
        
        // 3. 실제 저장된 아이디-비밀번호 계정이 있는지 확인한다.
        // 3-1. 아이디가 있는지 확인한다.
        if (!PlayerPrefs.HasKey(id))
        {
            _messageTextUI.text = "아이디/비밀번호를 확인해주세요.";
            return;
        }
        
        string myPassword = PlayerPrefs.GetString(id);
        if (myPassword != password)
        {
            _messageTextUI.text = "아이디/비밀번호를 확인해주세요.";
            return;
        }
        
        PlayerPrefs.SetString(LastLoginIdKey, id);
        PlayerPrefs.Save();
        
        // 4. 있다면 씬 이동
        SceneManager.LoadScene("LoadingScene");
    }

    private void Register()
    {
        // 로그인
        // 1. 아이디 입력을 확인한다.
        string id = _idInputField.text;
        if (string.IsNullOrEmpty(id))
        {
            _messageTextUI.text = "아이디를 입력해주세요.";
            return;
        }
        
        if (!IsValidEmail(id))
        {
            _messageTextUI.text = "이메일 형식이 아닙니다.";
            return;
        }
        
        // 2. 비밀번호 입력을 확인한다.
        string password = _passwordInputField.text;
        if (string.IsNullOrEmpty(password))
        {
            _messageTextUI.text = "패스워드를 입력해주세요.";
            return;
        }
        
        if (!IsValidPassword(password))
        {
            _messageTextUI.text =
                "비밀번호는 7~20자이며 대문자, 소문자, 숫자, 특수문자를 포함해야 합니다.";
            return;
        }
        
        // 2. 2ck 비밀번호 입력을 확인한다.
        string password2 = _passwordConfirmInputField.text;
        if (string.IsNullOrEmpty(password2) || password != password2)
        {
            _messageTextUI.text = "패스워드를 확인해주세요.";
            return;
        }
        
        // 4. 실제 저장된 아이디-비밀번호 계정이 있는지 확인한다.
        // 4-1. 아이디가 있는지 확인한다.
        if (PlayerPrefs.HasKey(id))
        {
            _messageTextUI.text = "중복된 아이디입니다.";
            return;
        }

        PlayerPrefs.SetString(id, password);

        GotoLogin();
    }

    private void GotoLogin()
    {
        _mode = SceneMode.Login;
        Refresh();
    }

    private void GotoRegister()
    {
        _mode = SceneMode.Register;
        Refresh();
    }
    
    private void LoadLastLoginId()
    {
        if (PlayerPrefs.HasKey(LastLoginIdKey))
        {
            _idInputField.text = PlayerPrefs.GetString(LastLoginIdKey);
        }
    }
    
    private bool IsValidEmail(string email)
    {
        string pattern = @"^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}$";
        return Regex.IsMatch(email, pattern);
    }

    private bool IsValidPassword(string password)
    {
        string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s])[A-Za-z\d^\w\s]{7,20}$";
        return Regex.IsMatch(password, pattern);
    }
}