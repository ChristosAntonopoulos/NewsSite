using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NewsSite.Server.Models;
using NewsSite.Server.Models.Auth;
using NewsSite.Server.Services.Interfaces;
using NewsSite.Server.Interfaces;
using Google.Apis.Auth;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace NewsSite.Server.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AuthService> _logger;
        private readonly JwtSettings _jwtSettings;
        private readonly GoogleAuthSettings _googleSettings;
        private readonly RedditAuthSettings _redditSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public AuthService(
            IUserRepository userRepository,
            IOptions<JwtSettings> jwtSettings,
            IOptions<GoogleAuthSettings> googleSettings,
            IOptions<RedditAuthSettings> redditSettings,
            IHttpClientFactory httpClientFactory,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _jwtSettings = jwtSettings.Value;
            _googleSettings = googleSettings.Value;
            _redditSettings = redditSettings.Value;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid email or password");
            }

            if (!await ValidatePasswordAsync(request.Password, user.PasswordHash))
            {
                throw new InvalidOperationException("Invalid email or password");
            }

            await _userRepository.UpdateLastLoginAsync(user.Id);
            var token = await GenerateJwtTokenAsync(user);

            return new AuthResponse
            {
                User = user,
                Token = token
            };
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var existingUser = await _userRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Email already exists");
            }

            var passwordHash = await HashPasswordAsync(request.Password);
            var user = new User
            {
                Email = request.Email,
                Name = request.Name,
                PasswordHash = passwordHash,
                Language = request.Language,
                Theme = "light",
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = DateTime.UtcNow
            };

            await _userRepository.CreateUserAsync(user);
            var token = await GenerateJwtTokenAsync(user);

            return new AuthResponse
            {
                User = user,
                Token = token
            };
        }

        public async Task<AuthResponse> GoogleLoginAsync(string token)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(token, new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _googleSettings.ClientId }
                });

                var user = await _userRepository.GetUserByGoogleIdAsync(payload.Subject);
                if (user == null)
                {
                    // Check if user exists with same email
                    user = await _userRepository.GetUserByEmailAsync(payload.Email);
                    if (user != null)
                    {
                        // Link Google account to existing user
                        await _userRepository.LinkGoogleAccountAsync(user.Id, payload.Subject);
                    }
                    else
                    {
                        // Create new user
                        user = new User
                        {
                            Email = payload.Email,
                            Name = payload.Name,
                            GoogleId = payload.Subject,
                            Language = "en",
                            Theme = "light",
                            CreatedAt = DateTime.UtcNow,
                            LastLoginAt = DateTime.UtcNow
                        };
                        await _userRepository.CreateUserAsync(user);
                    }
                }

                await _userRepository.UpdateLastLoginAsync(user.Id);
                var jwtToken = await GenerateJwtTokenAsync(user);

                return new AuthResponse
                {
                    User = user,
                    Token = jwtToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Google authentication");
                throw new InvalidOperationException("Invalid Google token");
            }
        }

        public async Task<AuthResponse> RedditLoginAsync(string code)
        {
            try
            {
                // Exchange code for access token
                var client = _httpClientFactory.CreateClient();
                var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://www.reddit.com/api/v1/access_token");
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_redditSettings.ClientId}:{_redditSettings.ClientSecret}"));
                tokenRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

                var content = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", _redditSettings.RedirectUri)
                });
                tokenRequest.Content = content;

                var tokenResponse = await client.SendAsync(tokenRequest);
                tokenResponse.EnsureSuccessStatusCode();
                var tokenJson = await tokenResponse.Content.ReadAsStringAsync();
                var tokenData = JsonSerializer.Deserialize<RedditTokenResponse>(tokenJson);

                // Get user info
                var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://oauth.reddit.com/api/v1/me");
                userRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tokenData.AccessToken);
                var userResponse = await client.SendAsync(userRequest);
                userResponse.EnsureSuccessStatusCode();
                var userJson = await userResponse.Content.ReadAsStringAsync();
                var redditUser = JsonSerializer.Deserialize<RedditUserInfo>(userJson);

                var user = await _userRepository.GetUserByRedditIdAsync(redditUser.Id);
                if (user == null)
                {
                    // Create new user
                    user = new User
                    {
                        Email = $"{redditUser.Name}@reddit.com", // Reddit doesn't provide email
                        Name = redditUser.Name,
                        RedditId = redditUser.Id,
                        Language = "en",
                        Theme = "light",
                        CreatedAt = DateTime.UtcNow,
                        LastLoginAt = DateTime.UtcNow
                    };
                    await _userRepository.CreateUserAsync(user);
                }

                await _userRepository.UpdateLastLoginAsync(user.Id);
                var jwtToken = await GenerateJwtTokenAsync(user);

                return new AuthResponse
                {
                    User = user,
                    Token = jwtToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Reddit authentication");
                throw new InvalidOperationException("Invalid Reddit code");
            }
        }

        public async Task<string> GenerateJwtTokenAsync(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim("language", user.Language),
                new Claim("theme", user.Theme)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(_jwtSettings.ExpirationDays);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<bool> ValidatePasswordAsync(string password, string passwordHash)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = await Task.Run(() => sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            var hash = Convert.ToBase64String(hashedBytes);
            return hash == passwordHash;
        }

        private async Task<string> HashPasswordAsync(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = await Task.Run(() => sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return Convert.ToBase64String(hashedBytes);
        }

        public async Task<User> GetUserByIdAsync(string id)
        {
            return await _userRepository.GetUserByIdAsync(id);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _userRepository.GetUserByEmailAsync(email);
        }
    }

    public class RedditTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; }

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }

    public class RedditUserInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }
    }
} 