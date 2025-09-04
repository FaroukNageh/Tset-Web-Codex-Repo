using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TestCodex.Pages.Account;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _factory;

    public LoginModel(IHttpClientFactory factory)
        => _factory = factory;

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? Token { get; set; }

    public class InputModel
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public async Task OnPostAsync()
    {
        var client = _factory.CreateClient();
        var response = await client.RequestPasswordTokenAsync(new PasswordTokenRequest
        {
            Address = "/connect/token",
            UserName = Input.Username,
            Password = Input.Password,
            Scope = "openid"
        });

        if (!string.IsNullOrEmpty(response.AccessToken))
        {
            Token = response.AccessToken;
        }
    }
}
