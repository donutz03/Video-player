class VideoEffects {
    constructor(videoElement, canvasElement) {
        this.video = videoElement;
        this.canvas = canvasElement;
        this.ctx = this.canvas.getContext('2d');
        this.currentEffect = 0;
        this.isPlaying = false;

        this.setupCanvas();
        this.setupEventListeners();
        this.setupControls();
    }

    setupCanvas() {
        this.video.addEventListener('loadedmetadata', () => {
            this.canvas.width = this.video.videoWidth;
            this.canvas.height = this.video.videoHeight;
            this.video.play().catch(e => console.log('Error playing video:', e));
        });
    }

    setupEventListeners() {
        this.video.addEventListener('play', () => {
            this.isPlaying = true;
            this.render();
        });

        this.video.addEventListener('pause', () => {
            this.isPlaying = false;
        });

        this.video.addEventListener('ended', () => {
            this.isPlaying = false;
        });

        this.canvas.addEventListener('click', () => {
            if (this.video.paused) {
                this.video.play();
            } else {
                this.video.pause();
            }
        });
    }

    setupControls() {
        this.canvas.style.cursor = 'pointer';

        this.video.addEventListener('canplay', () => {
            if (this.isPlaying) {
                this.video.play().catch(e => console.log('Error playing video:', e));
            }
        });
    }

    setEffect(effectId) {
        this.currentEffect = effectId;
    }

    render() {
        if (!this.isPlaying) return;

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
            
        }

        requestAnimationFrame(() => this.render());
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