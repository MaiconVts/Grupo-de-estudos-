// Aguarda o documento HTML carregar completamente antes de executar o código.
// Isso previne erros de tentar manipular elementos que ainda não existem.
document.addEventListener("DOMContentLoaded", function () {
  
// Funcionalidade de hover para os dropdowns
            const dropdowns = document.querySelectorAll('.main-nav .dropdown');
            dropdowns.forEach(function (dropdown) {
                dropdown.addEventListener('mouseenter', () => bootstrap.Dropdown.getOrCreateInstance(dropdown.querySelector('[data-bs-toggle="dropdown"]')).show());
                dropdown.addEventListener('mouseleave', () => bootstrap.Dropdown.getOrCreateInstance(dropdown.querySelector('[data-bs-toggle="dropdown"]')).hide());
            });

            // Efeito de transição no header ao rolar a página inteira
            const header = document.querySelector('.hero-section header');
            window.addEventListener('scroll', function () {
                if (window.scrollY > 50) {
                    header.classList.add('scrolled');
                } else {
                    header.classList.remove('scrolled');
                }
            });
        });


  // --- PARTE 2: EFEITO DE TRANSIÇÃO NO HEADER AO ROLAR A PÁGINA ---
  // Seleciona o elemento <header> que está dentro de .hero-section.
  const header = document.querySelector(".hero-section header");

  // Adiciona um "ouvinte" para o evento de rolagem da JANELA INTEIRA.
  // Esta é a abordagem mais comum para headers fixos.
  window.addEventListener("scroll", function () {
    // Se a rolagem vertical da página for maior que 50 pixels...
    if (window.scrollY > 50) {
      // Adiciona a classe 'scrolled' ao header, ativando o efeito CSS.
      header.classList.add("scrolled");
    } else {
      // Se a rolagem for menor que 50 pixels, remove a classe.
      header.classList.remove("scrolled");
    }
  });
});
