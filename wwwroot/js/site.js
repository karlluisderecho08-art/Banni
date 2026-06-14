// PawsInn Pet Hotel - site.js

// Auto-dismiss alerts after 5 seconds
document.addEventListener('DOMContentLoaded', () => {
    const alerts = document.querySelectorAll('.alert');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.transition = 'opacity 0.5s';
            alert.style.opacity = '0';
            setTimeout(() => alert.remove(), 500);
        }, 5000);
    });

    // Highlight active nav link
    const path = window.location.pathname.toLowerCase();
    document.querySelectorAll('.nav-links a').forEach(link => {
        if (link.getAttribute('href')?.toLowerCase() === path) {
            link.style.background = 'rgba(255,255,255,0.2)';
            link.style.color = 'white';
        }
    });
});
