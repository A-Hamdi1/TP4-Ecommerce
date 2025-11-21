/**
 * Simple Lightbox for Product Images
 */
class Lightbox {
    constructor() {
        this.init();
    }

    init() {
        // Create lightbox container
        if (!document.getElementById('lightbox-container')) {
            const container = document.createElement('div');
            container.id = 'lightbox-container';
            container.className = 'lightbox-container';
            container.innerHTML = `
                <div class="lightbox-overlay"></div>
                <div class="lightbox-content">
                    <button class="lightbox-close" aria-label="Close">
                        <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
                        </svg>
                    </button>
                    <img class="lightbox-image" src="" alt="Product image" />
                </div>
            `;
            document.body.appendChild(container);

            // Event listeners
            container.querySelector('.lightbox-overlay').addEventListener('click', () => this.close());
            container.querySelector('.lightbox-close').addEventListener('click', () => this.close());
            document.addEventListener('keydown', (e) => {
                if (e.key === 'Escape') this.close();
            });
        }
    }

    open(imageSrc, imageAlt = '') {
        const container = document.getElementById('lightbox-container');
        const img = container.querySelector('.lightbox-image');
        img.src = imageSrc;
        img.alt = imageAlt;
        container.classList.add('active');
        document.body.style.overflow = 'hidden';
    }

    close() {
        const container = document.getElementById('lightbox-container');
        container.classList.remove('active');
        document.body.style.overflow = '';
    }
}

// Initialize
const lightbox = new Lightbox();
window.lightbox = lightbox;

