using System;
using System.Net;
using System.Net.Http.Json;
using RaceDay.API.DTOs;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class AuthTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Register_NewUser_ReturnsCreated()
        {
            var dto = new RegisterDto
            {
                FullName = "Test User",
                Email = $"user_{Guid.NewGuid()}@example.com",
                Password = "Password123",
                Role = "Participant"
            };

            var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
            var content = await response.Content.ReadAsStringAsync();

            // If it's a 500 error, this will show us the stack trace
            if (!response.IsSuccessStatusCode) throw new Exception($"Register failed ({response.StatusCode}): {content}");

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Register_DuplicateEmail_ReturnsConflict()
        {
            var email = $"dupe_{Guid.NewGuid()}@test.com";
            var dto = new RegisterDto { FullName = "Test", Email = email, Password = "Password123", Role = "Participant" };

            // First registration - should succeed
            var firstResponse = await _client.PostAsJsonAsync("/api/auth/register", dto);
            Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

            // Second registration with the same email - should conflict
            var secondResponse = await _client.PostAsJsonAsync("/api/auth/register", dto);
            var secondContent = await secondResponse.Content.ReadAsStringAsync();

            if (secondResponse.StatusCode != HttpStatusCode.Conflict)
            {
                throw new Exception($"Expected Conflict, got {secondResponse.StatusCode}: {secondContent}");
            }

            Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);
        }
        

        [Fact]
        public async Task Login_ValidCredentials_ReturnsToken()
        {
            var email = $"login_{Guid.NewGuid()}@test.com";
            var registerDto = new RegisterDto { FullName = "Login User", Email = email, Password = "Password123", Role = "Participant" };
            await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            var loginDto = new LoginDto { Email = email, Password = "Password123" };

            var response = await _client.PostAsJsonAsync("/api/auth/login", loginDto);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode) throw new Exception($"Login failed ({response.StatusCode}): {content}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Contains("token", content);
        }
    }
}
