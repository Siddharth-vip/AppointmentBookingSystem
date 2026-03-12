using System.Text;
using System.Text.Json;
using Appointment.UI.Models;

namespace Appointment.UI.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;

        public ApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // ===============================
        // LOGIN USER
        // ===============================
        public async Task<User?> LoginUserAsync(string email, string password)
        {
            var data = new
            {
                Email = email,
                Password = password
            };

            var json = JsonSerializer.Serialize(data);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("user/login", content);

            if (!response.IsSuccessStatusCode)
                return null;

            var result = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<User>(result,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        // ===============================
        // REGISTER USER
        // ===============================
        public async Task<bool> RegisterUserAsync(User user)
        {
            var json = JsonSerializer.Serialize(user);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("user/register", content);

            return response.IsSuccessStatusCode;
        }

        // ===============================
        // GET USER BY EMAIL
        // ===============================
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var response = await _httpClient.GetAsync($"user/email/{email}");

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<User>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        // ===============================
        // GET ALL DOCTORS
        // ===============================
        public async Task<List<Doctor>> GetDoctorsAsync()
        {
            var response = await _httpClient.GetAsync("doctor/all");

            if (!response.IsSuccessStatusCode)
                return new List<Doctor>();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<Doctor>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Doctor>();
        }

        // ===============================
        // GET AVAILABLE DOCTORS
        // ===============================
        public async Task<List<Doctor>> GetAvailableDoctorsAsync(DateTime date, TimeSpan time)
{
    string formattedDate = date.ToString("yyyy-MM-dd");
    string formattedTime = time.ToString(@"hh\:mm\:ss");

    var response = await _httpClient.GetAsync(
        $"doctor/available?date={formattedDate}&time={formattedTime}");

    if (!response.IsSuccessStatusCode)
        return new List<Doctor>();

    var json = await response.Content.ReadAsStringAsync();

    return JsonSerializer.Deserialize<List<Doctor>>(json,
        new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? new List<Doctor>();
}

        // ===============================
        // GET TIMESLOTS
        // ===============================
        public async Task<List<TimeSlot>> GetTimeSlotsAsync(int doctorId)
        {
            var response = await _httpClient.GetAsync($"timeslot/{doctorId}");

            if (!response.IsSuccessStatusCode)
                return new List<TimeSlot>();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<TimeSlot>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<TimeSlot>();
        }

        // ===============================
        // BOOK APPOINTMENT
        // ===============================
        public async Task<bool> BookAppointmentAsync(Appointment.UI.Models.Appointment appointment)
        {
            var json = JsonSerializer.Serialize(appointment);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("appointment/book", content);

            return response.IsSuccessStatusCode;
        }
    }
}