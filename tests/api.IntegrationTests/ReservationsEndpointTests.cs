using System.Net;
using System.Net.Http.Json;
using Contracts;
using Models;

namespace api.IntegrationTests;

public class ReservationsEndpointTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ReservationsEndpointTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }


    [Fact]
    public async Task Post_valid_reservation_returns_201()
    {
        var booking = new ReservationRequest
        {
            RoomNumber = "101",
            GuestEmail = "guest@mjail.com",
            Start = DateTime.Today.AddDays(1),
            End = DateTime.Today.AddDays(3)
        };

        var response = await _client.PostAsJsonAsync("/api/reservations", booking);

        Assert.True(
            response.StatusCode == HttpStatusCode.Created,
            $"Expected 201 but got {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}"
        );

        var reservation = await response.Content.ReadFromJsonAsync<Reservation>();
        Assert.NotNull(reservation);
        Assert.NotEqual(Guid.Empty, reservation.Id);
        Assert.Equal("101", reservation.RoomNumber);
        Assert.Equal("guest@mjail.com", reservation.GuestEmail);
    }

    [Fact]
    public async Task Get_reservations_returns_200()
    {
        var response = await _client.GetAsync("/api/reservations");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var reservations = await response.Content.ReadFromJsonAsync<List<Reservation>>();
        Assert.NotNull(reservations);
    }

    [Fact]
    public async Task Get_nonexistent_reservation_returns_404()
    {
        var fakeId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/reservations/{fakeId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("NotFound", error.Title);
        Assert.Equal("Reservation", error.ResourceType);
    }


    [Fact]
    public async Task Post_reservation_with_nonexistent_room_returns_404()
    {
        var booking = new ReservationRequest
        {
            RoomNumber = "999",
            GuestEmail = "guest@mjail.com",
            Start = DateTime.Today.AddDays(1),
            End = DateTime.Today.AddDays(3)
        };

        var response = await _client.PostAsJsonAsync("/api/reservations", booking);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("NotFound", error.Title);
        Assert.Equal("Room", error.ResourceType);
        Assert.Equal("999", error.ResourceId);
    }

    [Fact]
    public async Task Post_reservation_with_invalid_data_returns_400_with_errors()
    {
        var invalidBooking = new ReservationRequest
        {
            RoomNumber = "000",
            GuestEmail = "not-an-email",
            Start = DateTime.Today.AddDays(-1),
            End = DateTime.Today.AddDays(-1)
        };

        var response = await _client.PostAsJsonAsync("/api/reservations", invalidBooking);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error);
        Assert.Equal("BadRequest", error.Title);
        Assert.NotNull(error.Errors);
        Assert.True(error.Errors.ContainsKey("RoomNumber"));
        Assert.True(error.Errors.ContainsKey("GuestEmail"));
        Assert.True(error.Errors.ContainsKey("Start"));
    }

    [Fact]
    public async Task Post_reservation_with_invalid_room_number_returns_room_error()
    {
        var booking = new ReservationRequest
        {
            RoomNumber = "abc",
            GuestEmail = "guest@mjail.com",
            Start = DateTime.Today.AddDays(1),
            End = DateTime.Today.AddDays(3)
        };

        var response = await _client.PostAsJsonAsync("/api/reservations", booking);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error?.Errors);
        Assert.True(error.Errors.ContainsKey("RoomNumber"));
        Assert.False(error.Errors.ContainsKey("GuestEmail"));
    }

    [Fact]
    public async Task Post_reservation_with_invalid_email_returns_email_error()
    {
        var booking = new ReservationRequest
        {
            RoomNumber = "101",
            GuestEmail = "nodomain",
            Start = DateTime.Today.AddDays(1),
            End = DateTime.Today.AddDays(3)
        };

        var response = await _client.PostAsJsonAsync("/api/reservations", booking);
        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.NotNull(error?.Errors);
        Assert.True(error.Errors.ContainsKey("GuestEmail"));
        Assert.False(error.Errors.ContainsKey("RoomNumber"));
    }
}
