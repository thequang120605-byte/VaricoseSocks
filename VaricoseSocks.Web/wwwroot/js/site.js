// =========================================================================
// VARICOSE SOCKS - CLIENT SIDE MOTION & MICRO-INTERACTION ENGINE
// =========================================================================

document.addEventListener('DOMContentLoaded', () => {
    initSmartNavbar();
    init3DCardTilt();
    initSpotlightSearchShortcut();
    initStickyMobileBar();
    initPressureSimulator();
    initSizingSlider();
    initCounters();
});

// =========================================================================
// 1. SMART DYNAMIC FLOATING GLASS NAVBAR
// =========================================================================
function initSmartNavbar() {
    const navbar = document.querySelector('.glass-nav');
    if (!navbar) return;

    let lastScrollY = window.scrollY;

    window.addEventListener('scroll', () => {
        const currentScrollY = window.scrollY;

        if (currentScrollY > 60) {
            navbar.classList.add('nav-shrink');
        } else {
            navbar.classList.remove('nav-shrink');
        }

        // Hide when scrolling down, show when scrolling up
        if (currentScrollY > 300 && currentScrollY > lastScrollY) {
            navbar.style.transform = 'translateY(-100%)';
        } else {
            navbar.style.transform = 'translateY(0)';
        }

        lastScrollY = currentScrollY;
    }, { passive: true });
}

// =========================================================================
// 2. PARABOLIC FLY-TO-CART ANIMATION
// =========================================================================
function flyToCart(imgSource, event) {
    let img = null;

    if (typeof imgSource === 'string') {
        img = document.querySelector(imgSource);
    } else if (imgSource instanceof HTMLElement) {
        img = imgSource.tagName === 'IMG' ? imgSource : imgSource.querySelector('img');
    }

    const cartBadge = document.getElementById('cartCountBadge') || document.querySelector('.bi-cart3');
    if (!img || !cartBadge) return;

    const imgRect = img.getBoundingClientRect();
    const cartRect = cartBadge.getBoundingClientRect();

    const clone = img.cloneNode(true);
    clone.classList.add('flying-sock-ghost');
    clone.style.width = '64px';
    clone.style.height = '64px';
    clone.style.objectFit = 'contain';
    clone.style.left = `${imgRect.left + (imgRect.width / 2) - 32}px`;
    clone.style.top = `${imgRect.top + (imgRect.height / 2) - 32}px`;

    document.body.appendChild(clone);

    // Compute delta coordinates
    const deltaX = (cartRect.left + (cartRect.width / 2)) - (imgRect.left + (imgRect.width / 2));
    const deltaY = (cartRect.top + (cartRect.height / 2)) - (imgRect.top + (imgRect.height / 2));

    requestAnimationFrame(() => {
        clone.style.transform = `translate(${deltaX}px, ${deltaY}px) scale(0.2) rotate(360deg)`;
        clone.style.opacity = '0.4';
    });

    setTimeout(() => {
        clone.remove();
        // Trigger badge bounce
        const badge = document.getElementById('cartCountBadge');
        if (badge) {
            badge.classList.remove('cart-bounce');
            void badge.offsetWidth; // re-flow
            badge.classList.add('cart-bounce');
        }
    }, 800);
}

// =========================================================================
// 3. STACKED TOAST NOTIFICATIONS (SONNER STYLE)
// =========================================================================
function showModernToast(title, message, type = 'success') {
    let container = document.getElementById('toastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'toastContainer';
        document.body.appendChild(container);
    }

    const toast = document.createElement('div');
    toast.className = `med-toast med-toast-${type}`;

    let iconHtml = '<i class="bi bi-check-circle-fill fs-5 text-success"></i>';
    if (type === 'info') iconHtml = '<i class="bi bi-info-circle-fill fs-5 text-info"></i>';
    if (type === 'warning') iconHtml = '<i class="bi bi-exclamation-triangle-fill fs-5 text-warning"></i>';

    toast.innerHTML = `
        <div class="toast-icon">${iconHtml}</div>
        <div class="flex-grow-1">
            <div class="fw-bold small">${title}</div>
            <div class="text-white-50" style="font-size: 0.78rem;">${message}</div>
        </div>
        <button type="button" class="btn-close btn-close-white btn-sm" onclick="this.parentElement.remove()"></button>
    `;

    container.appendChild(toast);

    requestAnimationFrame(() => {
        toast.classList.add('show');
    });

    setTimeout(() => {
        toast.classList.remove('show');
        setTimeout(() => toast.remove(), 400);
    }, 4000);
}

// =========================================================================
// 4. 3D CARD TILT MICRO-INTERACTION
// =========================================================================
function init3DCardTilt() {
    const cards = document.querySelectorAll('.product-card');
    cards.forEach(card => {
        card.addEventListener('mousemove', e => {
            const rect = card.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            const centerX = rect.width / 2;
            const centerY = rect.height / 2;

            const rotateX = ((y - centerY) / centerY) * -6;
            const rotateY = ((x - centerX) / centerX) * 6;

            card.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateY(-8px)`;
        });

        card.addEventListener('mouseleave', () => {
            card.style.transform = 'perspective(1000px) rotateX(0deg) rotateY(0deg) translateY(0)';
        });
    });
}

// =========================================================================
// 5. INTERACTIVE CCL PRESSURE SIMULATOR
// =========================================================================
function initPressureSimulator() {
    const buttons = document.querySelectorAll('.pressure-pill-btn');
    const pressureValueElem = document.getElementById('simPressureValue');
    const pressureDescElem = document.getElementById('simPressureDesc');
    const veinValveElem = document.getElementById('simVeinValve');

    if (!buttons.length || !pressureValueElem) return;

    buttons.forEach(btn => {
        btn.addEventListener('click', () => {
            buttons.forEach(b => b.classList.remove('active'));
            btn.classList.add('active');

            const ccl = btn.getAttribute('data-ccl');
            if (ccl === '1') {
                pressureValueElem.innerText = '15 - 20 mmHg (CCL1)';
                pressureDescElem.innerText = 'Áp lực êm dịu, phòng ngừa mỏi chân, giãn tĩnh mạch mạng nhện, phù nhẹ.';
                if (veinValveElem) veinValveElem.style.transform = 'scale(1.05)';
            } else if (ccl === '2') {
                pressureValueElem.innerText = '20 - 30 mmHg (CCL2)';
                pressureDescElem.innerText = 'Chuẩn y tế điều trị suy giãn tĩnh mạch giai đoạn C2-C4, nổi gân xanh, sau can thiệp xơ hóa.';
                if (veinValveElem) veinValveElem.style.transform = 'scale(1.15)';
            } else if (ccl === '3') {
                pressureValueElem.innerText = '30 - 40 mmHg (CCL3)';
                pressureDescElem.innerText = 'Áp lực cực mạnh cho huyết khối tĩnh mạch sâu (DVT), phù bạch mạch nặng, theo chỉ định bác sĩ.';
                if (veinValveElem) veinValveElem.style.transform = 'scale(1.25)';
            }
        });
    });
}

// =========================================================================
// 6. INTERACTIVE SIZING SLIDER RULER
// =========================================================================
function initSizingSlider() {
    const slider = document.getElementById('liveAnkleSlider');
    const display = document.getElementById('liveAnkleDisplay');
    const badge = document.getElementById('liveSizeBadge');
    const advice = document.getElementById('liveSizeAdvice');

    if (!slider || !display) return;

    const updateSize = (val) => {
        display.innerText = `${val} cm`;
        let recSize = 'S';
        let desc = 'Ôm sát bắp chân thon gọn, áp lực chuẩn 100% tại mắt cá.';

        if (val < 21) {
            recSize = 'S';
            desc = 'Dành cho cổ chân nhỏ (18 - 21 cm). Hiệu quả nén tối ưu.';
        } else if (val < 25) {
            recSize = 'M';
            desc = 'Dành cho cổ chân trung bình (21 - 25 cm). Kích cỡ phổ biến nhất.';
        } else if (val < 29) {
            recSize = 'L';
            desc = 'Dành cho cổ chân đầy đặn (25 - 29 cm). Co giãn êm ái, không bó thắt.';
        } else {
            recSize = 'XL';
            desc = 'Dành cho cổ chân lớn hoặc sưng phù (29 - 33 cm).';
        }

        if (badge) badge.innerText = `Size ${recSize}`;
        if (advice) advice.innerText = desc;

        // Also sync on product detail if available
        const sizeBtns = document.querySelectorAll('#sizeSelector .btn-size');
        sizeBtns.forEach(b => {
            if (b.innerText.trim() === recSize) {
                b.classList.add('active');
            } else {
                b.classList.remove('active');
            }
        });
    };

    slider.addEventListener('input', e => updateSize(e.target.value));
}

// =========================================================================
// 7. CONFETTI CELEBRATION TRIGGER
// =========================================================================
function triggerConfetti() {
    if (typeof confetti === 'function') {
        confetti({
            particleCount: 80,
            spread: 70,
            origin: { y: 0.6 }
        });
    }
}

// =========================================================================
// 8. SPOTLIGHT SEARCH (CTRL + K / CMD + K)
// =========================================================================
function initSpotlightSearchShortcut() {
    window.addEventListener('keydown', e => {
        if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
            e.preventDefault();
            toggleSpotlightSearch();
        }
    });
}

function toggleSpotlightSearch() {
    const modalEl = document.getElementById('spotlightSearchModal');
    if (modalEl) {
        const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modal.show();
        setTimeout(() => {
            const input = document.getElementById('spotlightInput');
            if (input) input.focus();
        }, 200);
    }
}

// =========================================================================
// 9. STICKY MOBILE BUY BAR
// =========================================================================
function initStickyMobileBar() {
    const bar = document.querySelector('.sticky-mobile-buy-bar');
    const mainBuyBtn = document.getElementById('mainBuyBtn');
    if (!bar || !mainBuyBtn) return;

    window.addEventListener('scroll', () => {
        const rect = mainBuyBtn.getBoundingClientRect();
        if (rect.bottom < 0) {
            bar.classList.add('visible');
        } else {
            bar.classList.remove('visible');
        }
    }, { passive: true });
}

// =========================================================================
// 10. COUNTER ANIMATION (0 to Value)
// =========================================================================
function initCounters() {
    const counters = document.querySelectorAll('.counter-animate');
    counters.forEach(counter => {
        const target = +counter.getAttribute('data-target') || 0;
        let count = 0;
        const speed = target / 30;

        const update = () => {
            count += speed;
            if (count < target) {
                counter.innerText = Math.floor(count).toLocaleString('vi-VN');
                requestAnimationFrame(update);
            } else {
                counter.innerText = target.toLocaleString('vi-VN');
            }
        };
        update();
    });
}

// =========================================================================
// 11. ENHANCED CART LIVE ACTIONS
// =========================================================================
async function addToCartLive(productId, size, color, qty, triggerElement) {
    try {
        if (triggerElement) {
            flyToCart(triggerElement);
        }

        const formData = new FormData();
        formData.append('productId', productId);
        formData.append('size', size || 'M');
        formData.append('color', color || 'Màu Da (Beige)');
        formData.append('quantity', qty || 1);

        const resp = await fetch('/Cart/AddToCart', {
            method: 'POST',
            body: formData,
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        if (resp.ok) {
            const data = await resp.json();
            const badge = document.getElementById('cartCountBadge');
            if (badge) badge.innerText = data.totalQuantity;

            showModernToast('Đã thêm vào giỏ hàng!', `Vớ y khoa (Size ${size || 'M'}) đã sẵn sàng trong giỏ của bạn.`, 'success');
        } else {
            window.location.href = '/Cart';
        }
    } catch {
        window.location.href = '/Cart';
    }
}

async function updateCartQty(cartItemId, newQty) {
    if (newQty < 1) {
        if (!confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?')) return;
    }

    try {
        const formData = new FormData();
        formData.append('cartItemId', cartItemId);
        formData.append('quantity', newQty);

        const resp = await fetch('/Cart/UpdateQuantity', {
            method: 'POST',
            body: formData,
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        if (resp.ok) {
            window.location.reload();
        }
    } catch {
        window.location.reload();
    }
}

async function removeCartItem(cartItemId) {
    if (!confirm('Bạn có chắc muốn xóa sản phẩm này khỏi giỏ hàng?')) return;

    try {
        const formData = new FormData();
        formData.append('cartItemId', cartItemId);

        const resp = await fetch('/Cart/RemoveItem', {
            method: 'POST',
            body: formData,
            headers: { 'X-Requested-With': 'XMLHttpRequest' }
        });

        if (resp.ok) {
            window.location.reload();
        }
    } catch {
        window.location.reload();
    }
}

// =========================================================================
// 12. CHATBOT AI ASSISTANT FUNCTIONS
// =========================================================================
function toggleChatbot() {
    const chatWin = document.getElementById('chatbotWindow');
    if (chatWin) {
        chatWin.classList.toggle('d-none');
        if (!chatWin.classList.contains('d-none')) {
            const input = document.getElementById('chatInput');
            if (input) input.focus();
        }
    }
}

function openChatbot() {
    const chatWin = document.getElementById('chatbotWindow');
    if (chatWin && chatWin.classList.contains('d-none')) {
        chatWin.classList.remove('d-none');
    }
    const input = document.getElementById('chatInput');
    if (input) input.focus();
}

function sendQuickMsg(text) {
    const input = document.getElementById('chatInput');
    if (input) {
        input.value = text;
        handleChatSubmit(new Event('submit'));
    }
}

function promptSizeTool() {
    const input = document.getElementById('chatInput');
    if (input) {
        input.value = 'Tôi có số đo cổ chân là 22cm, bắp chân 35cm, tôi mặc size gì?';
        handleChatSubmit(new Event('submit'));
    }
}

async function handleChatSubmit(e) {
    if (e && e.preventDefault) e.preventDefault();
    const input = document.getElementById('chatInput');
    if (!input) return;
    const msg = input.value.trim();
    if (!msg) return;

    appendChatMessage('user', msg);
    input.value = '';

    const loadingId = 'loading-' + Date.now();
    appendLoadingMessage(loadingId);

    let ankleCm = null;
    let calfCm = null;
    const matches = msg.match(/(\d+(\.\d+)?)\s*(cm)?/g);
    if (matches && matches.length > 0) {
        const numbers = matches.map(m => parseFloat(m)).filter(n => !isNaN(n));
        if (numbers.length > 0) ankleCm = numbers[0];
        if (numbers.length > 1) calfCm = numbers[1];
    }

    try {
        const response = await fetch('/Chatbot/Ask', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                message: msg,
                ankleCm: ankleCm,
                calfCm: calfCm,
                isPregnant: msg.toLowerCase().includes('bầu') || msg.toLowerCase().includes('thai'),
                hasVaricose: msg.toLowerCase().includes('gân') || msg.toLowerCase().includes('phù'),
                hasSwelling: msg.toLowerCase().includes('sưng') || msg.toLowerCase().includes('nặng')
            })
        });

        removeLoadingMessage(loadingId);

        if (response.ok) {
            const data = await response.json();
            renderBotResponse(data);
        } else {
            appendChatMessage('bot', 'Cảm ơn bạn! Để được tư vấn chi tiết theo đúng chỉ số y khoa, bạn có thể liên hệ hotline 1900 6868 hoặc thử chọn lại số đo cổ chân nhé.');
        }
    } catch {
        removeLoadingMessage(loadingId);
        appendChatMessage('bot', 'Chào bạn! Bác sĩ AI khuyên bạn nên sử dụng vớ y khoa chuẩn áp lực CCL2 (20-30 mmHg) nếu có triệu chứng nặng chân, nổi gân xanh, hoặc CCL1 nếu chỉ mỏi chân văn phòng.');
    }
}

function appendChatMessage(sender, text) {
    const container = document.getElementById('chatbotMessages');
    if (!container) return;

    const row = document.createElement('div');
    row.className = `d-flex mb-3 ${sender === 'user' ? 'justify-content-end' : ''}`;

    if (sender === 'user') {
        row.innerHTML = `<div class="bg-primary text-white p-3 rounded-4 shadow-sm small" style="max-width: 85%; border-bottom-right-radius: 4px;">${text}</div>`;
    } else {
        row.innerHTML = `<div class="bg-white p-3 rounded-4 shadow-sm border small text-dark" style="max-width: 85%; border-bottom-left-radius: 4px;">
            <div class="fw-bold text-primary mb-1"><i class="bi bi-shield-plus me-1"></i> Bác sĩ AI Varicose</div>
            ${text}
        </div>`;
    }

    container.appendChild(row);
    container.scrollTop = container.scrollHeight;
}

function appendLoadingMessage(id) {
    const container = document.getElementById('chatbotMessages');
    if (!container) return;

    const row = document.createElement('div');
    row.id = id;
    row.className = 'd-flex mb-3';
    row.innerHTML = `
        <div class="bg-white p-2 px-3 rounded-4 shadow-sm border small text-muted d-flex align-items-center gap-2">
            <div class="spinner-grow spinner-grow-sm text-primary" role="status"></div>
            <span>Bác sĩ AI đang phân tích dữ liệu...</span>
        </div>`;
    container.appendChild(row);
    container.scrollTop = container.scrollHeight;
}

function removeLoadingMessage(id) {
    const elem = document.getElementById(id);
    if (elem) elem.remove();
}

function renderBotResponse(data) {
    const container = document.getElementById('chatbotMessages');
    if (!container) return;

    let prodsHtml = '';
    if (data.recommendedProducts && data.recommendedProducts.length > 0) {
        prodsHtml = '<div class="mt-2 pt-2 border-top"><div class="fw-bold text-dark mb-1">Sản phẩm khuyến nghị cho bạn:</div>';
        data.recommendedProducts.forEach(p => {
            prodsHtml += `
                <div class="d-flex align-items-center gap-2 mb-2 p-1 bg-light rounded-3">
                    <img src="${p.imageUrl}" style="width: 38px; height: 38px; object-fit: contain;" class="rounded border bg-white" />
                    <div class="flex-grow-1 text-truncate">
                        <div class="fw-bold small text-truncate">${p.name}</div>
                        <div class="text-danger small fw-bold">${p.price.toLocaleString('vi-VN')} ₫</div>
                    </div>
                    <a href="/Product/Detail/${p.id}" class="btn btn-sm btn-outline-primary py-0 px-2" style="font-size: 0.75rem;">Xem</a>
                </div>`;
        });
        prodsHtml += '</div>';
    }

    const row = document.createElement('div');
    row.className = 'd-flex mb-3';
    row.innerHTML = `
        <div class="bg-white p-3 rounded-4 shadow-sm border small text-dark" style="max-width: 90%; border-bottom-left-radius: 4px;">
            <div class="fw-bold text-primary mb-1"><i class="bi bi-shield-plus me-1"></i> Bác sĩ AI Varicose</div>
            <div>${data.answer}</div>
            ${prodsHtml}
        </div>`;

    container.appendChild(row);
    container.scrollTop = container.scrollHeight;
}

// Modal size calculator
function openSizeModal() {
    const modalEl = document.getElementById('sizeGuideModal');
    if (modalEl) {
        const modal = bootstrap.Modal.getOrCreateInstance(modalEl);
        modal.show();
    }
}

async function calculateModalSize() {
    const input = document.getElementById('modalAnkleInput');
    const resElem = document.getElementById('modalSizeResult');
    if (!input || !resElem) return;

    const val = parseFloat(input.value);
    if (isNaN(val) || val <= 0) {
        resElem.innerText = 'Vui lòng nhập số đo vòng cổ chân hợp lệ (cm).';
        return;
    }

    try {
        const resp = await fetch(`/Chatbot/CalculateSize?ankleCm=${val}`);
        const data = await resp.json();
        resElem.innerHTML = `<span class="badge bg-success fs-6 me-2">Size ${data.recommendedSize}</span> ${data.explanation}`;
    } catch {
        resElem.innerText = 'Vui lòng kiểm tra lại số đo.';
    }
}
