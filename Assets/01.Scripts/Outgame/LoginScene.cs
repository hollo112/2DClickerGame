using Cysharp.Threading.Tasks;
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
    
    private void Start()
    {
        AddButtonEvents();
        Refresh();
    }

    private void AddButtonEvents()
    {
        _gotoRegisterButton.onClick.AddListener(GotoRegister);
        _loginButton.onClick.AddListener(() => Login().Forget());
        _gotoLoginButton.onClick.AddListener(GotoLogin);
        _registerButton.onClick.AddListener(() => Register().Forget());
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

    public void OnEmailTextChanged(string email)
    {
        var emailSpec = new AccountEmailSpecification();
        if (!emailSpec.IsStatisfiedBy(email))
        {
            _messageTextUI.text = emailSpec.ErrorMessage;
            _loginButton.enabled = false;
            return;
        }
        
        _messageTextUI.text = "완벽한 이메일입니다.";
        _loginButton.enabled = true;
    }
    
    private async UniTask Login()
    {
        // 로그인
        string email = _idInputField.text;
        string password = _passwordInputField.text;

        var result = await AccountManager.Instance.TryLogin(email, password);
        if (result.Success)
        {
            SceneManager.LoadScene("GameScene");
        }
        else
        {
            _messageTextUI.text = result.ErrorMessage;
        }
    }

    private async UniTask Register()
    {
        string email = _idInputField.text;
        string password = _passwordInputField.text;
        string password2 = _passwordConfirmInputField.text;

        if (string.IsNullOrEmpty(password2) || password != password2)
        {
            _messageTextUI.text = "패스워드를 확인해주세요.";
            return;
        }

        var result = await AccountManager.Instance.TryRegister(email, password);
        if (result.Success)
        {
            GotoLogin();
        }
        else
        {
            _messageTextUI.text = result.ErrorMessage;
        }

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
    
}