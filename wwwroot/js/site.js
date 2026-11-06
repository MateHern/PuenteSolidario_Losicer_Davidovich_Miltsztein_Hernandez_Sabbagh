// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', () => {
  const toggleButton = document.querySelector('[data-nav-toggle]');
  const mobileNav = document.querySelector('[data-mobile-nav]');
  const yearTarget = document.querySelector('[data-current-year]');

  if (yearTarget) {
    yearTarget.textContent = new Date().getFullYear();
  }

  if (toggleButton && mobileNav) {
    toggleButton.addEventListener('click', () => {
      const isExpanded = toggleButton.getAttribute('aria-expanded') === 'true';
      toggleButton.setAttribute('aria-expanded', String(!isExpanded));
      mobileNav.classList.toggle('hidden');
    });
  }
});
