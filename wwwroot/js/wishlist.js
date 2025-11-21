// Gestion de la wishlist avec JavaScript vanilla
(function() {
    // Variable globale pour le nom d'utilisateur
    let currentUserName = window.currentUserName || '';

    // Fonction pour mettre à jour l'apparence du bouton
    function updateButtonAppearance(button, isInWishlist) {
        const currentText = button.textContent.trim();
        
        if (isInWishlist) {
            // Produit dans la wishlist - cœur plein rouge
            if (currentText.includes('Liste de souhaits')) {
                button.textContent = '❤️ Liste de souhaits';
            } else {
                button.textContent = '❤️';
            }
            button.classList.add('text-red-600');
            button.classList.remove('text-gray-400');
        } else {
            // Produit pas dans la wishlist - cœur vide
            if (currentText.includes('Liste de souhaits')) {
                button.textContent = '🤍 Liste de souhaits';
            } else {
                button.textContent = '🤍';
            }
            button.classList.remove('text-red-600');
            button.classList.add('text-gray-400');
        }
    }

    // Initialiser les boutons wishlist au chargement
    function initWishlistButtons() {
        document.querySelectorAll('.toggle-wishlist').forEach(function(button) {
            const productId = button.getAttribute('data-product-id');
            
            if (!productId) return;
            
            // Vérifier le statut initial de la wishlist
            fetch('/Wishlist/GetWishlistStatus/' + productId)
                .then(response => {
                    if (!response.ok) {
                        throw new Error('Erreur réseau');
                    }
                    return response.json();
                })
                .then(data => {
                    updateButtonAppearance(button, data.isInWishlist);
                })
                .catch(error => {
                    console.error('Erreur lors de la vérification du statut:', error);
                    // En cas d'erreur, on laisse le bouton tel quel
                });
        });
    }

    // Gérer le clic sur les boutons wishlist
    document.addEventListener('click', function(e) {
        const button = e.target.closest('.toggle-wishlist');
        
        if (!button) return;
        
        // Ignorer les boutons sur la page Wishlist/Index (gérés par le script inline)
        if (window.location.pathname.includes('/Wishlist/Index') || window.location.pathname === '/Wishlist') {
            return;
        }
        
        e.preventDefault();
        e.stopPropagation();
        
        const productId = button.getAttribute('data-product-id');
        
        if (!productId) return;
        
        // Vérifier si l'utilisateur est authentifié
        if (!currentUserName) {
            window.location.href = '/Account/Login';
            return;
        }
        
        // Récupérer le token anti-forgery
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenInput ? tokenInput.value : '';
        
        // Désactiver le bouton pendant la requête
        button.disabled = true;
        const originalContent = button.innerHTML;
        const originalText = button.textContent;
        button.innerHTML = '⏳';
        
        // Faire la requête AJAX
        fetch('/Wishlist/ToggleWishlist/' + productId, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include'
        })
        .then(response => {
            if (!response.ok) {
                throw new Error('Erreur réseau');
            }
            return response.json();
        })
        .then(data => {
            button.disabled = false;
            
            if (!data.success) {
                throw new Error(data.error || 'Une erreur est survenue');
            }
            
            // Mettre à jour l'apparence du bouton
            updateButtonAppearance(button, data.isInWishlist);
            
            // Mettre à jour le compteur dans la navbar si présent
            const wishlistLinks = document.querySelectorAll('a[href*="Wishlist"]');
            wishlistLinks.forEach(wishlistLink => {
                if (data.count !== undefined) {
                    let navCounter = wishlistLink.querySelector('span.bg-red-500, span[class*="bg-red"]');
                    if (data.count > 0) {
                        if (!navCounter) {
                            navCounter = document.createElement('span');
                            navCounter.className = 'ml-1 bg-red-500 text-white rounded-full text-xs px-2 py-0.5';
                            wishlistLink.appendChild(navCounter);
                        }
                        navCounter.textContent = data.count;
                        navCounter.style.display = 'inline';
                    } else {
                        if (navCounter) {
                            navCounter.style.display = 'none';
                        }
                    }
                }
            });
        })
        .catch(error => {
            console.error('Erreur:', error);
            button.disabled = false;
            button.innerHTML = originalContent;
            button.textContent = originalText;
            alert('Une erreur est survenue. Veuillez réessayer.');
        });
    });

    // Initialiser les boutons au chargement de la page
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initWishlistButtons);
    } else {
        initWishlistButtons();
    }
})();
