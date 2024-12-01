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
    public event Action? OnStateChanged; // Correct event name
    
  public UserDto CurrentUser { get; private set; }
    public bool IsAuthenticated => CurrentUser != null;

    public AuthStateService(HttpClient httpClient, ILocalStorageService localStorageService, NavigationManager navigationManager)
    {
        _httpClient = httpClient;
        _localStorageService = localStorageService;
        _navigationManager = navigationManager;
    }
    private void NotifyAuthStateChanged()
    {
OnStateChanged?.Invoke(); // Trigger the OnStateChanged event
    }

    public async Task Initialize()
    {
        var token = await _localStorageService.GetItemAsync<string>("token");
        if (token != null)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            DecodeAndSetUserFromToken(token);
            NotifyAuthStateChanged(); // Notify state change

        }
    }

    public async Task<bool> Login(string username, string password)
    {
      //  Console.WriteLine("AuthStateService.Login() - Hit");
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", new { Username = username, Password = password });
      //  Console.WriteLine("AuthStateService.Login() - response: " + response.ToString());
        
        if (response.IsSuccessStatusCode)
        {

            var responseJson = await response.Content.ReadFromJsonAsync<TokenResponse>();
            var token = responseJson?.Token?.Trim();
          //  Console.WriteLine("AuthStateService.Login() - token recieved. " + token);

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("AuthStateService.Login() - Token is null or empty.");
                return false;
            }

           // Console.WriteLine("AuthStateService.Login() - Token received: " + token);

            await _localStorageService.SetItemAsync("token", token);
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            DecodeAndSetUserFromToken(token);
            
            SetStateData(CurrentUser);
            NotifyAuthStateChanged(); // Notify state change

            
            return true;
        }
        return false;
    }

  

    public void Logout()
    {
        _localStorageService.RemoveItemAsync("token");
        _httpClient.DefaultRequestHeaders.Authorization = null;
        CurrentUser = null;
        NotifyAuthStateChanged(); // Notify state change
        _navigationManager.NavigateTo("/Home");
    }

    private void DecodeAndSetUserFromToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var userId = jwtToken.Claims.First(claim => claim.Type == "UserId").Value;
        var username = jwtToken.Claims.First(claim => claim.Type == "Username").Value;
        var isAdmin = jwtToken.Claims.First(claim => claim.Type == "IsAdmin").Value == "true";
        var email = jwtToken.Claims.First(claim => claim.Type == "Email").Value;
        var fullName = jwtToken.Claims.First(claim => claim.Type == "FullName").Value;
        var apartment = jwtToken.Claims.First(claim => claim.Type == "Apartment").Value;
        
        
        
        // Extract LivesIn claims
        var livesInList = jwtToken.Claims
            .Where(claim => claim.Type == "ComplexName" || claim.Type == "Street" || claim.Type == "City" || claim.Type == "ZipCode")
            .GroupBy(claim => claim.Value)
            .Select(group =>
            {
                var complexName = group.FirstOrDefault(c => c.Type == "ComplexName")?.Value;
                var street = group.FirstOrDefault(c => c.Type == "Street")?.Value;
                var city = group.FirstOrDefault(c => c.Type == "City")?.Value;
                var zipCode = group.FirstOrDefault(c => c.Type == "ZipCode")?.Value;

                return new LivesInDto
                {
                    ComplexName = complexName,
                    Street = street,
                    City = city,
                    ZipCode = zipCode
                };
            })
            .ToList();
        
        
        /*
        foreach (var claim in jwtToken.Claims)
        {
            Console.WriteLine($"Claim Type: {claim.Type}, Value: {claim.Value}");
        }
*/
        CurrentUser = new UserDto
        {
            Id = int.Parse(userId),
            UserName = username,
            Email = email,
            FullName = fullName,
            IsAdmin = isAdmin,
            LivesIn = livesInList
        };
    }

    public async Task<string>  Test()
    {
        var response1 = await _httpClient.GetAsync("api/test");

        return response1.ToString();
    }
    
    
    private void SetStateData(UserDto currentUser)
    {
        
        
        
    }
    
    
}
public class TokenResponse
{
    public string Token { get; set; }
}
