using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using FrontendBlazor.Model.DTOs;


public class AuthStateService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorageService;
    private readonly NavigationManager _navigationManager;

    public UserDto CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;

    public AuthStateService(HttpClient httpClient, ILocalStorageService localStorageService, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _localStorageService = localStorageService;
        _navigationManager = navigationManager;
    }

    public async Task Initialize()
    {
        var token = await _localStorageService.GetItemAsync<string>("token");
        if (token != null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            DecodeAndSetUserFromToken(token);
        }
    }

    public async Task<bool> Login(string username, string password)
    {
        
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new { Username = username, Password = password });
        
        

        
        if (response.IsSuccessStatusCode)
        {
            var token = await response.Content.ReadAsStringAsync();
            await _localStorageService.SetItemAsync("token", token);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            DecodeAndSetUserFromToken(token);

            if (CurrentUser.IsAdmin)
            {
                _navigationManager.NavigateTo("/admin-dashboard");
            }
            else
            {
                _navigationManager.NavigateTo("/user-dashboard");
            }
            return true;
        }
        return false;
    }

    public void Logout()
    {
        _localStorageService.RemoveItemAsync("token");
        _httpClient.DefaultRequestHeaders.Authorization = null;
        CurrentUser = null;
        _navigationManager.NavigateTo("/login");
    }

    private void DecodeAndSetUserFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var userId = jwtToken.Claims.First(claim => claim.Type == "nameid").Value;
        var username = jwtToken.Claims.First(claim => claim.Type == "unique_name").Value;
        var isAdmin = jwtToken.Claims.First(claim => claim.Type == "IsAdmin").Value == "true";

        CurrentUser = new UserDto
        {
            Id = int.Parse(userId),
            UserName = username,
            IsAdmin = isAdmin
        };
    }
}
