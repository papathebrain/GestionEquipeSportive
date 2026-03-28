// ================================================
// GESTION ÉQUIPES SPORTIVES — Scripts personnalisés
// ================================================

document.addEventListener('DOMContentLoaded', function () {

    // Auto-dismiss des alertes après 5 secondes
    const alerts = document.querySelectorAll('.alert.alert-success, .alert.alert-info');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = bootstrap.Alert.getOrCreateInstance(alert);
            if (bsAlert) bsAlert.close();
        }, 5000);
    });

    // Afficher les résumés de validation
    const validationSummary = document.querySelector('[data-valmsg-summary]');
    if (validationSummary) {
        const errors = validationSummary.querySelectorAll('li');
        if (errors.length > 0) {
            validationSummary.style.display = 'block';
        }
    }

    // Animation des cards au scroll
    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
            }
        });
    }, { threshold: 0.1 });

    document.querySelectorAll('.card').forEach(card => {
        observer.observe(card);
    });

    // Confirmation de suppression générique
    document.querySelectorAll('[data-confirm-delete]').forEach(btn => {
        btn.addEventListener('click', function (e) {
            if (!confirm(this.dataset.confirmDelete || 'Êtes-vous sûr de vouloir supprimer cet élément ?')) {
                e.preventDefault();
            }
        });
    });

    // Initialisation des tooltips Bootstrap
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipTriggerList.forEach(el => new bootstrap.Tooltip(el));

    // Preview d'image générique
    document.querySelectorAll('input[type="file"][data-preview]').forEach(input => {
        input.addEventListener('change', function () {
            const previewId = this.dataset.preview;
            const preview = document.getElementById(previewId);
            if (preview && this.files && this.files[0]) {
                const reader = new FileReader();
                reader.onload = e => { preview.src = e.target.result; };
                reader.readAsDataURL(this.files[0]);
            }
        });
    });

});

// Utilitaire: synchroniser color picker et input texte
function syncColorInput(colorInputId, textInputId) {
    const colorInput = document.getElementById(colorInputId);
    const textInput = document.getElementById(textInputId);
    if (!colorInput || !textInput) return;

    colorInput.addEventListener('input', () => {
        textInput.value = colorInput.value;
    });

    textInput.addEventListener('input', () => {
        if (/^#[0-9A-Fa-f]{6}$/.test(textInput.value)) {
            colorInput.value = textInput.value;
        }
    });
}
