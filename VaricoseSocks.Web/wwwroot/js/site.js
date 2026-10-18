// =========================================================================
// VARICOSE SOCKS - CLIENT SIDE JAVASCRIPT & AI CHATBOT INTEGRATION
// =========================================================================

// 1. CHATBOT AI ASSISTANT FUNCTIONS
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

    // Hiển thị biểu tượng đang trả lời
    const loadingId = 'loading-' + Date.now();
    appendLoadingMessage(loadingId);

    // Trích xuất số đo nếu người dùng nhập số kèm cm (Vd: "22cm")
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
            headers: {
                'Content-Type': 'application/json'
            },
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
            appendChatMessage('bot', 'Cảm ơn bạn! Để được tư vấn chi tiết nhất theo đúng chỉ số y khoa, bạn có thể liên hệ trực tiếp hotline bác sĩ 1900 6868 hoặc thử chọn lại triệu chứng nhé.');
        }
    } catch (err) {
        removeLoadingMessage(loadingId);
        appendChatMessage('bot', 'Chào bạn! Bác sĩ AI khuyên bạn nên sử dụng vớ y khoa chuẩn áp lực CCL2 (20-30 mmHg) nếu có triệu chứng nặng chân, nổi gân xanh, hoặc CCL1 nếu chỉ mỏi chân văn phòng.');
    }
}

function appendChatMessage(sender, text) {
    const container = document.getElementById('chatbotMessages');
    if (!container) return;

    const row = document.createElement('div');
    row.className = sender === 'user' ? 'd-flex justify-content-end mb-3' : 'd-flex mb-3';

    const bubble = document.createElement('div');
    bubble.className = sender === 'user' 
        ? 'bg-primary text-white p-3 rounded-3 shadow-sm small' 
        : 'bg-white text-dark p-3 rounded-3 shadow-sm border small';
    bubble.style.maxWidth = '85%';

    if (sender === 'user') {
        bubble.innerText = text;
    } else {
        bubble.innerHTML = `<div class="fw-bold text-primary mb-1"><i class="bi bi-shield-plus me-1"></i> Bác sĩ AI Varicose</div>${text}`;
    }

    row.appendChild(bubble);
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
        <div class="bg-white text-muted p-2 px-3 rounded-3 shadow-sm border small">
            <span class="spinner-border spinner-border-sm text-primary me-2" role="status"></span> Đang phân tích triệu chứng y khoa...
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

    const row = document.createElement('div');
    row.className = 'd-flex mb-3';

    let prodsHtml = '';
    if (data.recommendedProducts && data.recommendedProducts.length > 0) {
        prodsHtml += '<div class="mt-2 border-top pt-2"><div class="fw-bold text-dark mb-1">Sản phẩm khuyến nghị:</div>';
        data.recommendedProducts.forEach(p => {
            const price = (p.discountPrice || p.price).toLocaleString('vi-VN');
            prodsHtml += `
                <div class="d-flex align-items-center justify-content-between bg-light p-2 rounded mb-1 border">
                    <div style="font-size: 0.8rem;" class="fw-semibold text-truncate me-2" style="max-width: 170px;">${p.name}</div>
                    <div class="text-danger fw-bold text-nowrap" style="font-size: 0.8rem;">${price} ₫</div>
                    <a href="/Product/Detail/${p.id}" class="btn btn-sm btn-outline-primary py-0 px-2 ms-2" style="font-size: 0.75rem;">Xem</a>
                </div>`;
        });
        prodsHtml += '</div>';
    }

    let sizeHtml = '';
    if (data.recommendedSize) {
        sizeHtml = `<div class="alert alert-success py-1 px-2 my-2 small mb-1">
            <strong>Size khuyến nghị:</strong> <span class="badge bg-success fs-6">Size ${data.recommendedSize}</span>
        </div>`;
    }

    let compHtml = '';
    if (data.recommendedCompressionText) {
        compHtml = `<div class="badge bg-primary mb-1">${data.recommendedCompressionText}</div>`;
    }

    row.innerHTML = `
        <div class="bg-white text-dark p-3 rounded-3 shadow-sm border small" style="max-width: 88%;">
            <div class="fw-bold text-primary mb-1"><i class="bi bi-shield-plus me-1"></i> Bác sĩ AI Varicose</div>
            ${compHtml}
            <div>${data.replyMessage}</div>
            ${sizeHtml}
            ${prodsHtml}
        </div>`;

    container.appendChild(row);
    container.scrollTop = container.scrollHeight;
}

// 2. MODAL SIZE CALCULATOR
function openSizeModal() {
    const modal = new bootstrap.Modal(document.getElementById('sizeGuideModal'));
    modal.show();
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

// 3. CART OPERATIONS
async function addToCartLive(productId, size, color, qty) {
    try {
        const formData = new FormData();
        formData.append('productId', productId);
        formData.append('size', size || 'M');
        formData.append('color', color || 'Màu Da (Beige)');
        formData.append('quantity', qty || 1);

        const resp = await fetch('/Cart/AddToCart', {
            method: 'POST',
            body: formData,
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        if (resp.ok) {
            const data = await resp.json();
            const badge = document.getElementById('cartCountBadge');
            if (badge) badge.innerText = data.totalQuantity;
            alert('Đã thêm vớ y khoa vào giỏ hàng thành công!');
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
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
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
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            }
        });

        if (resp.ok) {
            window.location.reload();
        }
    } catch {
        window.location.reload();
    }
}

// Cập nhật số lượng ban đầu của Cart Badge
document.addEventListener('DOMContentLoaded', async () => {
    try {
        const resp = await fetch('/Cart/Index', { headers: { 'X-Requested-With': 'XMLHttpRequest' } });
    } catch { }
});