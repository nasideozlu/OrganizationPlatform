// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.querySelectorAll('[data-photo-slider]').forEach((slider) => {
  const slides = Array.from(slider.querySelectorAll('[data-slide-index]'));
  const current = slider.querySelector('[data-slide-current]');
  if (slides.length < 2) return;

  let activeIndex = 0;
  const showSlide = (index) => {
    activeIndex = (index + slides.length) % slides.length;
    slides.forEach((slide, slideIndex) => {
      const active = slideIndex === activeIndex;
      slide.classList.toggle('is-active', active);
      slide.setAttribute('aria-hidden', String(!active));
    });
    if (current) current.textContent = String(activeIndex + 1);
  };

  slider.querySelector('[data-slide-prev]')?.addEventListener('click', () => showSlide(activeIndex - 1));
  slider.querySelector('[data-slide-next]')?.addEventListener('click', () => showSlide(activeIndex + 1));
  showSlide(0);
});
