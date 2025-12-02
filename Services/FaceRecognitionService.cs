using System.Text;
using System.Text.Json;

namespace HRMCyberse.Services
{
    public class FaceRecognitionService : IFaceRecognitionService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<FaceRecognitionService> _logger;
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public FaceRecognitionService(HttpClient httpClient, IConfiguration configuration, ILogger<FaceRecognitionService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _apiKey = configuration["ComPreFace:ApiKey"] ?? "d7358648-ba78-4b08-a8d5-da8a36fe53de";
            _baseUrl = configuration["ComPreFace:BaseUrl"] ?? "http://localhost:8000";
            
            _httpClient.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        }

        public async Task<string> AddFaceAsync(int userId, byte[] imageData)
        {
            try
            {
                _logger.LogInformation("AddFaceAsync called for user {UserId}, image size: {Size} bytes", userId, imageData.Length);
                _logger.LogInformation("ComPreFace URL: {Url}", $"{_baseUrl}/api/v1/recognition/faces");
                _logger.LogInformation("API Key: {Key}", _apiKey.Substring(0, 8) + "...");

                using var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(imageData), "file", $"user_{userId}.jpg");
                content.Add(new StringContent(userId.ToString()), "subject");

                _logger.LogInformation("Sending request to ComPreFace...");
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/recognition/faces", content);
                
                var result = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("ComPreFace response: Status={Status}, Body={Body}", 
                    response.StatusCode, result);
                
                if (!response.IsSuccessStatusCode)
                {
                    // Parse error response to provide better error messages
                    string errorMessage = "Không thể đăng ký khuôn mặt";
                    
                    try
                    {
                        var errorJson = JsonSerializer.Deserialize<ComPreFaceError>(result, new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        });
                        
                        if (errorJson?.Code == 28)
                        {
                            errorMessage = "Không tìm thấy khuôn mặt trong ảnh. Vui lòng:\n" +
                                         "- Chụp ảnh trong điều kiện ánh sáng tốt\n" +
                                         "- Đảm bảo khuôn mặt rõ ràng và nhìn thẳng vào camera\n" +
                                         "- Không che khuất khuôn mặt";
                        }
                        else if (errorJson?.Message != null)
                        {
                            errorMessage = errorJson.Message;
                        }
                    }
                    catch
                    {
                        // If parsing fails, use generic error
                    }
                    
                    throw new Exception(errorMessage);
                }

                _logger.LogInformation("Successfully added face for user {UserId}", userId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding face for user {UserId}", userId);
                throw;
            }
        }

        public async Task<FaceRecognitionResult> RecognizeFaceAsync(byte[] imageData)
        {
            try
            {
                _logger.LogInformation("RecognizeFaceAsync called, image size: {Size} bytes", imageData.Length);

                using var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(imageData), "file", "check.jpg");

                var response = await _httpClient.PostAsync($"{_baseUrl}/api/v1/recognition/recognize", content);
                var jsonResult = await response.Content.ReadAsStringAsync();
                
                _logger.LogInformation("ComPreFace recognize response: Status={Status}, Body={Body}", 
                    response.StatusCode, jsonResult);
                
                if (!response.IsSuccessStatusCode)
                {
                    // Parse error to provide specific message
                    string errorMessage = "Không thể nhận diện khuôn mặt";
                    
                    try
                    {
                        var errorJson = JsonSerializer.Deserialize<ComPreFaceError>(jsonResult, new JsonSerializerOptions 
                        { 
                            PropertyNameCaseInsensitive = true 
                        });
                        
                        if (errorJson?.Code == 28)
                        {
                            errorMessage = "Không tìm thấy khuôn mặt trong ảnh. Vui lòng chụp lại với:\n" +
                                         "- Ánh sáng tốt hơn\n" +
                                         "- Khuôn mặt rõ ràng và nhìn thẳng\n" +
                                         "- Không che khuất";
                        }
                        else if (errorJson?.Message != null)
                        {
                            errorMessage = errorJson.Message;
                        }
                    }
                    catch
                    {
                        // If parsing fails, use generic error
                    }
                    
                    return new FaceRecognitionResult
                    {
                        Success = false,
                        Message = errorMessage
                    };
                }

                var result = JsonSerializer.Deserialize<ComPreFaceResponse>(jsonResult, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });

                if (result?.Result == null || result.Result.Count == 0)
                {
                    _logger.LogWarning("No faces detected in recognition response");
                    return new FaceRecognitionResult
                    {
                        Success = false,
                        Message = "Không tìm thấy khuôn mặt trong ảnh. Vui lòng thử lại."
                    };
                }

                var bestMatch = result.Result[0];
                if (bestMatch.Subjects == null || bestMatch.Subjects.Count == 0)
                {
                    _logger.LogWarning("Face detected but no matching subjects found");
                    return new FaceRecognitionResult
                    {
                        Success = false,
                        Message = "Không nhận diện được người dùng. Vui lòng đăng ký khuôn mặt trước."
                    };
                }

                var subject = bestMatch.Subjects[0];
                _logger.LogInformation("Face recognized: UserId={UserId}, Similarity={Similarity}", 
                    subject.Subject, subject.Similarity);

                if (int.TryParse(subject.Subject, out int userId))
                {
                    return new FaceRecognitionResult
                    {
                        Success = true,
                        UserId = userId,
                        Confidence = subject.Similarity,
                        Message = "Nhận diện thành công"
                    };
                }

                return new FaceRecognitionResult
                {
                    Success = false,
                    Message = "Dữ liệu người dùng không hợp lệ"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recognizing face");
                return new FaceRecognitionResult
                {
                    Success = false,
                    Message = $"Lỗi nhận diện: {ex.Message}"
                };
            }
        }

        public async Task<bool> DeleteFaceAsync(int userId)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/v1/recognition/faces?subject={userId}");
                response.EnsureSuccessStatusCode();
                
                _logger.LogInformation("Deleted face for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting face for user {UserId}", userId);
                return false;
            }
        }

        private class ComPreFaceResponse
        {
            public List<FaceResult>? Result { get; set; }
        }

        private class FaceResult
        {
            public List<SubjectMatch>? Subjects { get; set; }
        }

        private class SubjectMatch
        {
            public string? Subject { get; set; }
            public double Similarity { get; set; }
        }

        private class ComPreFaceError
        {
            public string? Message { get; set; }
            public int Code { get; set; }
        }
    }
}
