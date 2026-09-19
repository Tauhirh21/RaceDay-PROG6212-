using System;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using RaceDay.API.DTOs;
using Xunit;

namespace RaceDay.Api.Tests
{
    public class EventTests : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public EventTests(CustomWebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        // Improved helper method that throws clear errors instead of failing on JSON parsing
        private async Task<string> GetToken(string email, string role)
        {
            var uniqueEmail = $"{email}_{Guid.NewGuid()}@test.com";

            var register = new RegisterDto { FullName = "Test " + role, Email = uniqueEmail, Password = "Password123", Role = role };
            var regResponse = await _client.PostAsJsonAsync("/api/auth/register", register);
            if (!regResponse.IsSuccessStatusCode)
            {
                var regError = await regResponse.Content.ReadAsStringAsync();
                throw new Exception($"Registration failed ({regResponse.StatusCode}): {regError}");
            }

            var login = new LoginDto { Email = uniqueEmail, Password = "Password123" };
            var response = await _client.PostAsJsonAsync("/api/auth/login", login);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception($"Login failed ({response.StatusCode}): {content}");
            }

            using var doc = JsonDocument.Parse(content);
            if (doc.RootElement.TryGetProperty("token", out var tokenElement))
            {
                return tokenElement.GetString()!;
            }

            throw new Exception($"Token not found in response: {content}");
        }

        [Fact]
        public async Task CreateEvent_AsOrganiser_ReturnsCreated()
        {
            // Arrange
            var token = await GetToken("organiser", "Organiser");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dto = new CreateEventDto
            {
                Name = "Test Run",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Cape Town",
                Province = "WC",
                EventType = "Run"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/events", dto);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_AsParticipant_ReturnsForbidden()
        {
            // Arrange
            var token = await GetToken("participant", "Participant");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var dto = new CreateEventDto
            {
                Name = "Test Run",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Cape Town",
                Province = "WC",
                EventType = "Run"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/events", dto);

            // Assert - Role rejected
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [Fact]
        public async Task CreateEvent_WithoutToken_ReturnsUnauthorized()
        {
            // Arrange - Make sure no token is attached
            _client.DefaultRequestHeaders.Authorization = null;

            var dto = new CreateEventDto
            {
                Name = "Test Run",
                EventDate = DateTime.UtcNow.AddDays(30),
                Location = "Cape Town",
                Province = "WC",
                EventType = "Run"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/events", dto);

            // Assert - Not authenticated
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task GetAllEvents_AsParticipant_ReturnsOk()
        {
            // Arrange
            var token = await GetToken("participant", "Participant");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            var response = await _client.GetAsync("/api/events");

            // Assert - Both roles can view events
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
