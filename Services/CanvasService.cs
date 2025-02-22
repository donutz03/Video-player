using VideoPlayer_EasierCS.Models;

namespace VideoPlayer_EasierCS.Services;
// Services/CanvasService.cs
public class CanvasService
{
    public string GenerateCanvasScript(Video currentVideo)
    {
        return @"
            function initializeCanvas() {
                const video = document.getElementById('sourceVideo');
                const canvas = document.getElementById('videoCanvas');
                const ctx = canvas.getContext('2d');
                
                function drawVideo() {
                    if (!video.paused && !video.ended) {
                        ctx.drawImage(video, 0, 0, canvas.width, canvas.height);
                        drawControls();
                    }
                    requestAnimationFrame(drawVideo);
                }

                function drawControls() {
                    ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
                    ctx.fillRect(0, canvas.height - 40, canvas.width, 40);
                    
                    const progress = video.currentTime / video.duration;
                    ctx.fillStyle = 'rgba(255, 255, 255, 0.5)';
                    ctx.fillRect(10, canvas.height - 30, (canvas.width - 20) * progress, 10);
                }

                video.addEventListener('play', drawVideo);
                
                canvas.width = video.videoWidth || 640;
                canvas.height = video.videoHeight || 360;
            }
            
            document.addEventListener('DOMContentLoaded', initializeCanvas);";
    }
}