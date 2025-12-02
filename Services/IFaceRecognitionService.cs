namespace HRMCyberse.Services
{
    public interface IFaceRecognitionService
    {
        Task<string> AddFaceAsync(int userId, byte[] imageData);
        Task<FaceRecognitionResult> RecognizeFaceAsync(byte[] imageData);
        Task<bool> DeleteFaceAsync(int userId);
    }

    public class FaceRecognitionResult
    {
        public bool Success { get; set; }
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public double Confidence { get; set; }
        public string? Message { get; set; }
    }
}
