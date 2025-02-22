class VideoEffects {
    constructor(videoElement, canvasElement) {
        this.video = videoElement;
        this.canvas = canvasElement;
        this.ctx = this.canvas.getContext('2d');
        this.currentEffect = 0;
        this.isPlaying = false;
        this.showControls = false;

        this.progressBarHeight = 30;
        this.controlButtonWidth = 40;

        this.setupEventListeners();
        this.draw();
    }

    setupEventListeners() {
        this.canvas.addEventListener('mouseenter', () => {
            this.showControls = true;
        });

        this.canvas.addEventListener('mouseleave', () => {
            this.showControls = false;
        });

        this.canvas.addEventListener('click', (e) => {
            const rect = this.canvas.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            const canvasHeight = this.canvas.height;

            if (y > canvasHeight - this.progressBarHeight) {
                const clickProgress = x / this.canvas.width;
                this.video.currentTime = clickProgress * this.video.duration;
            }
            else {
                if (this.video.paused) {
                    this.video.play();
                    this.isPlaying = true;
                } else {
                    this.video.pause();
                    this.isPlaying = false;
                }
            }
        });

        this.video.addEventListener('play', () => {
            this.isPlaying = true;
        });

        this.video.addEventListener('pause', () => {
            this.isPlaying = false;
        });
    }

    draw() {
        this.ctx.drawImage(this.video, 0, 0, this.canvas.width, this.canvas.height);

        if (this.currentEffect > 0) {
            const imageData = this.ctx.getImageData(0, 0, this.canvas.width, this.canvas.height);
            const pixels = imageData.data;

            switch (this.currentEffect) {
                case 1:
                    this.applyGrayscale(pixels);
                    break;
                case 2:
                    this.applySepia(pixels);
                    break;
                case 3:
                    this.applyInvert(pixels);
                    break;
            }

            this.ctx.putImageData(imageData, 0, 0);
        }

        if (this.showControls) {
            this.drawControls();
        }

        requestAnimationFrame(() => this.draw());
    }

    drawControls() {
        const width = this.canvas.width;
        const height = this.canvas.height;

        this.ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
        this.ctx.fillRect(0, height - this.progressBarHeight, width, this.progressBarHeight);

        if (this.video.duration) {
            const progress = (this.video.currentTime / this.video.duration) * width;
            this.ctx.fillStyle = 'rgba(255, 255, 255, 0.3)';
            this.ctx.fillRect(0, height - this.progressBarHeight, progress, this.progressBarHeight);
        }

        this.ctx.fillStyle = 'white';
        this.ctx.font = '14px Arial';
        this.ctx.textAlign = 'right';
        const timeText = `${Math.floor(this.video.currentTime)}s / ${Math.floor(this.video.duration)}s`;
        this.ctx.fillText(timeText, width - 10, height - this.progressBarHeight/2);

        this.ctx.fillStyle = 'white';
        if (this.video.paused) {
            this.ctx.beginPath();
            this.ctx.moveTo(10, height - this.progressBarHeight/2 - 10);
            this.ctx.lineTo(30, height - this.progressBarHeight/2);
            this.ctx.lineTo(10, height - this.progressBarHeight/2 + 10);
            this.ctx.fill();
        } else {
            this.ctx.fillRect(10, height - this.progressBarHeight/2 - 10, 5, 20);
            this.ctx.fillRect(20, height - this.progressBarHeight/2 - 10, 5, 20);
        }
    }

    setEffect(effectId) {
        this.currentEffect = effectId;
    }

    applyGrayscale(pixels) {
        for (let i = 0; i < pixels.length; i += 4) {
            const avg = (pixels[i] + pixels[i + 1] + pixels[i + 2]) / 3;
            pixels[i] = pixels[i + 1] = pixels[i + 2] = avg;
        }
    }

    applySepia(pixels) {
        for (let i = 0; i < pixels.length; i += 4) {
            const r = pixels[i];
            const g = pixels[i + 1];
            const b = pixels[i + 2];

            pixels[i] = Math.min(255, (r * 0.393) + (g * 0.769) + (b * 0.189));
            pixels[i + 1] = Math.min(255, (r * 0.349) + (g * 0.686) + (b * 0.168));
            pixels[i + 2] = Math.min(255, (r * 0.272) + (g * 0.534) + (b * 0.131));
        }
    }

    applyInvert(pixels) {
        for (let i = 0; i < pixels.length; i += 4) {
            pixels[i] = 255 - pixels[i];
            pixels[i + 1] = 255 - pixels[i + 1];
            pixels[i + 2] = 255 - pixels[i + 2];
        }
    }
}