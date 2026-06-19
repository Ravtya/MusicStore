(function () {
    'use strict';

    const form = document.getElementById('parametersForm');
    const pageInput = document.getElementById('pageInput');
    const slider = document.getElementById('likesSlider');
    const likesValue = document.getElementById('likesValue');
    const averageLikesInput = document.getElementById('averageLikesInput');

    document.addEventListener('DOMContentLoaded', () => {
        document.querySelector('[name="locale"]').addEventListener('change', submitFromFirstPage);
        document.getElementById('seedInput').addEventListener('change', submitFromFirstPage);

        if (slider) {
            syncLikes(slider.value);
            slider.addEventListener('input', e => syncLikes(e.target.value));
            slider.addEventListener('change', submitFromFirstPage);
        }

        const toggleBtn = document.getElementById('toggleViewBtn');
        if (toggleBtn) {
            toggleBtn.addEventListener('click', () => {
                const input = document.getElementById('infiniteScrollInput');
                input.value = input.value !== 'true';
                submitFromFirstPage();
            });
        }

        initInfiniteScroll();
        initLazyCovers();
    });

    function submitFromFirstPage() {
        pageInput.value = 1;
        form.submit();
    }

    function syncLikes(value) {
        const formatted = parseFloat(value).toFixed(1);
        likesValue.textContent = formatted;
        averageLikesInput.value = formatted;
    }

    function initLazyCovers() {
        const body = document.getElementById('trackTableBody');
        if (!body) return;

        body.addEventListener('shown.bs.collapse', e => {
            const img = e.target.querySelector('img.cover-img[data-cover-url]:not([src])');
            if (img) img.src = img.dataset.coverUrl;
        });
    }

    function initInfiniteScroll() {
        const sentinel = document.getElementById('scrollSentinel');
        if (!sentinel) return;

        let currentPage = parseInt(sentinel.dataset.currentPage, 10) || 1;
        let isLoading = false;

        new IntersectionObserver(entries => {
            if (!entries[0].isIntersecting || isLoading) return;

            isLoading = true;
            currentPage++;

            fetch(sentinel.dataset.baseUrl + currentPage)
                .then(r => r.text())
                .then(html => {
                    if (!html.trim()) {
                        sentinel.remove();
                        return;
                    }
                    document.getElementById('trackTableBody').insertAdjacentHTML('beforeend', html);
                    isLoading = false;
                })
                .catch(() => {
                    isLoading = false;
                });
        }, {rootMargin: '200px'}).observe(sentinel);
    }
})();
