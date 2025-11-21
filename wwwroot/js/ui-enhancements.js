/**
 * UI Enhancements Script
 * Adds modern interactions and animations
 */

(function() {
    'use strict';

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }

    function init() {
        enhanceNavigation();
        enhanceProductCards();
        enhanceForms();
        enhanceButtons();
        enhanceCart();
        enhanceDropdowns();
    }

    // ========== NAVIGATION ENHANCEMENTS ==========
    function enhanceNavigation() {
        // Replace hamburger icon
        const navToggle = document.getElementById('nav-toggle');
        if (navToggle && !navToggle.querySelector('.hamburger')) {
            navToggle.innerHTML = `
                <div class="hamburger">
                    <span></span>
                    <span></span>
                    <span></span>
                </div>
            `;
            
            navToggle.addEventListener('click', function() {
                const hamburger = this.querySelector('.hamburger');
                if (hamburger) {
                    hamburger.classList.toggle('active');
                }
            });
        }

        // Add active state to current page link
        const currentPath = window.location.pathname.toLowerCase();
        document.querySelectorAll('nav a[asp-controller]').forEach(link => {
            const href = link.getAttribute('href');
            if (href && currentPath.includes(href.toLowerCase())) {
                link.classList.add('nav-link-active');
            }
        });
    }

    // ========== PRODUCT CARDS ENHANCEMENTS ==========
    function enhanceProductCards() {
        // Add product-card class to product cards
        document.querySelectorAll('.bg-white.rounded-xl.shadow-sm').forEach(card => {
            if (card.querySelector('img[src*="images"]')) {
                card.classList.add('product-card');
                
                // Wrap image
                const img = card.querySelector('img');
                if (img && !img.parentElement.classList.contains('product-card-image-wrapper')) {
                    const wrapper = document.createElement('div');
                    wrapper.className = 'product-card-image-wrapper';
                    img.parentNode.insertBefore(wrapper, img);
                    wrapper.appendChild(img);
                }

                // Enhance badges
                card.querySelectorAll('span').forEach(span => {
                    if (span.textContent.includes('DT') || span.textContent.includes('€')) {
                        span.classList.add('product-badge', 'product-badge-price');
                    } else if (span.textContent.includes('Stock:')) {
                        span.classList.add('product-badge', 'product-badge-stock');
                        const stockMatch = span.textContent.match(/\d+/);
                        if (stockMatch) {
                            const stock = parseInt(stockMatch[0]);
                            if (stock === 0) {
                                span.classList.add('out-of-stock');
                            } else if (stock < 10) {
                                span.classList.add('low-stock');
                            }
                        }
                    }
                });
            }
        });
    }

    // ========== FORM ENHANCEMENTS ==========
    function enhanceForms() {
        // Add loading state to submit buttons
        document.querySelectorAll('form').forEach(form => {
            const submitBtn = form.querySelector('button[type="submit"]');
            if (submitBtn) {
                form.addEventListener('submit', function(e) {
                    if (form.checkValidity()) {
                        submitBtn.classList.add('btn-loading');
                        submitBtn.disabled = true;
                    }
                });
            }

            // Real-time validation feedback
            form.querySelectorAll('input, textarea, select').forEach(input => {
                input.addEventListener('blur', function() {
                    validateField(this);
                });

                input.addEventListener('input', function() {
                    if (this.classList.contains('form-input-error')) {
                        validateField(this);
                    }
                });
            });
        });
    }

    function validateField(field) {
        if (field.checkValidity()) {
            field.classList.remove('form-input-error');
        } else {
            field.classList.add('form-input-error');
        }
    }

    // ========== BUTTON ENHANCEMENTS ==========
    function enhanceButtons() {
        // Add hover effects to buttons
        document.querySelectorAll('a[class*="bg-indigo"], button[class*="bg-indigo"]').forEach(btn => {
            if (!btn.classList.contains('btn-hover-lift')) {
                btn.classList.add('btn-hover-lift');
            }
        });
    }

    // ========== CART ENHANCEMENTS ==========
    function enhanceCart() {
        // Enhance quantity controls
        document.querySelectorAll('.PlusProducts, .MinProducts').forEach(btn => {
            btn.addEventListener('click', function(e) {
                e.preventDefault();
                // Add visual feedback
                this.style.transform = 'scale(0.95)';
                setTimeout(() => {
                    this.style.transform = '';
                }, 150);
            });
        });

        // Animate cart item removal
        document.querySelectorAll('.RemoveLink').forEach(link => {
            link.addEventListener('click', function(e) {
                const row = this.closest('tr, .cart-item');
                if (row) {
                    row.classList.add('cart-item-removing');
                }
            });
        });
    }

    // ========== DROPDOWN ENHANCEMENTS ==========
    function enhanceDropdowns() {
        // Add smooth transitions to dropdowns
        document.querySelectorAll('.dropdown-menu').forEach(menu => {
            menu.style.transition = 'opacity 0.2s, transform 0.2s';
        });
    }

    // ========== UTILITY: Animate numbers ==========
    function animateNumber(element, target, duration = 1000) {
        const start = parseFloat(element.textContent) || 0;
        const increment = (target - start) / (duration / 16);
        let current = start;

        const timer = setInterval(() => {
            current += increment;
            if ((increment > 0 && current >= target) || (increment < 0 && current <= target)) {
                current = target;
                clearInterval(timer);
            }
            element.textContent = current.toFixed(2);
        }, 16);
    }

    // Expose utility functions
    window.UIEnhancements = {
        animateNumber: animateNumber
    };
})();

