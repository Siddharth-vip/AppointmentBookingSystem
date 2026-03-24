using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;
using Appointment.UI.Models;
using Microsoft.AspNetCore.Http;

namespace Appointment.UI.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        private HttpRequestMessage CreateAuthorizedRequest(HttpMethod method, string endpoint, HttpContent? content = null)
        {
            var request = new HttpRequestMessage(method, endpoint);
            var token = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");

            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            if (content != null)
            {
                request.Content = content;
            }

            return request;
        }

        // ===============================
        // LOGIN USER
        // ===============================
        public async Task<LoginResponse?> LoginUserAsync(string email, string password)
        {
            var response = await _httpClient.PostAsync(
                $"Auth/login?email={email}&password={password}", null);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<LoginResponse>(json,
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

            var response = await _httpClient.PostAsync("auth/register", content);

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
            var response = await _httpClient.GetAsync($"TimeSlot/doctor/{doctorId}");

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
        public async Task<BookingApiResult> BookAppointmentAsync(Appointment.UI.Models.Appointment appointment)
        {
            var json = JsonSerializer.Serialize(appointment, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = CreateAuthorizedRequest(HttpMethod.Post, "appointment/book", content);
            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            string message = response.IsSuccessStatusCode
                ? "Appointment booked successfully!"
                : "Unable to book appointment.";

            bool smsSent = false;

            if (!string.IsNullOrWhiteSpace(body))
            {
                using var doc = JsonDocument.Parse(body);

                if (doc.RootElement.TryGetProperty("message", out var msgProp))
                {
                    message = msgProp.GetString() ?? message;
                }

                if (doc.RootElement.TryGetProperty("smsSent", out var smsProp) && smsProp.ValueKind == JsonValueKind.True)
                {
                    smsSent = true;
                }
            }

            return new BookingApiResult
            {
                Success = response.IsSuccessStatusCode,
                Message = message,
                SmsSent = smsSent
            };
        }

        // ===============================
        // 🔥 DOCTOR LOGIN (FIXED POSITION)
        // ===============================
        public async Task<Doctor?> LoginDoctorAsync(string email, string password)
        {
            var response = await _httpClient.PostAsync(
                $"doctor/login?email={email}&password={password}", null);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<Doctor>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }

        public async Task<bool> CreateDoctorSlotAsync(int doctorId, DateTime slotDate, TimeSpan startTime, TimeSpan endTime)
        {
            var payload = new
            {
                DoctorId = doctorId,
                SlotDate = slotDate.Date,
                StartTime = startTime,
                EndTime = endTime,
                IsBooked = false
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = CreateAuthorizedRequest(HttpMethod.Post, "timeslot/create", content);
            var response = await _httpClient.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        public async Task<BookingApiResult> CreateAdminStandardSlotsAsync(int doctorId, DateTime slotDate)
        {
            var payload = new
            {
                DoctorId = doctorId,
                SlotDate = slotDate.Date
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var request = CreateAuthorizedRequest(HttpMethod.Post, "admin/create-standard-slots", content);
            var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            string message = response.IsSuccessStatusCode
                ? "Standard slots generated successfully."
                : "Unable to generate slots.";

            if (!string.IsNullOrWhiteSpace(body))
            {
                using var doc = JsonDocument.Parse(body);
                if (doc.RootElement.TryGetProperty("message", out var msgProp))
                {
                    message = msgProp.GetString() ?? message;
                }

                if (doc.RootElement.TryGetProperty("createdCount", out var countProp) && countProp.TryGetInt32(out int count))
                {
                    message = $"{message} Created slots: {count}.";
                }
            }

            return new BookingApiResult
            {
                Success = response.IsSuccessStatusCode,
                Message = message,
                SmsSent = false
            };
        }

        public async Task<bool> DeleteDoctorSlotAsync(int slotId, int doctorId)
        {
            var response = await _httpClient.DeleteAsync($"timeslot/{slotId}?doctorId={doctorId}");
            return response.IsSuccessStatusCode;
        }

        public async Task<List<DoctorAppointmentDetail>> GetDoctorAppointmentsAsync(int doctorId)
        {
            var response = await _httpClient.GetAsync($"appointment/doctor/{doctorId}");

            if (!response.IsSuccessStatusCode)
                return new List<DoctorAppointmentDetail>();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<DoctorAppointmentDetail>>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<DoctorAppointmentDetail>();
        }
    }
}