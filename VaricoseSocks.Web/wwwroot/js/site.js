function getCart() {
    return JSON.parse(localStorage.getItem('cart') || '[]');
}

function saveCart(cart) {
    localStorage.setItem('cart', JSON.stringify(cart));
    updateCartBadge();
}

function updateCartBadge() {
    const cart = getCart();
    const totalCount = cart.reduce((sum, item) => sum + item.quantity, 0);
    const badge = document.getElementById('cartCountBadge');
    if (badge) {
        badge.innerText = totalCount;
    }
}

function addToCart(productId, name, price) {
    const qtyInput = document.getElementById('buyQty');
    const quantity = qtyInput ? parseInt(qtyInput.value) || 1 : 1;
    
    let cart = getCart();
    const existing = cart.find(x => x.id === productId);
    if (existing) {
        existing.quantity += quantity;
    } else {
        cart.push({ id: productId, name: name, price: price, quantity: quantity });
    }
    saveCart(cart);
    alert('Đã thêm sản phẩm vào giỏ hàng!');
}

function openMiniCart() {
    const cart = getCart();
    if (cart.length === 0) {
        alert('Giỏ hàng hiện đang trống.');
        return;
    }
    let msg = 'Giỏ hàng của bạn:\n';
    let total = 0;
    cart.forEach(i => {
        msg += `- ${i.name} x ${i.quantity}: ${(i.price * i.quantity).toLocaleString()} ₫\n`;
        total += i.price * i.quantity;
    });
    msg += `\nTổng tiền: ${total.toLocaleString()} ₫`;
    alert(msg);
}

document.addEventListener('DOMContentLoaded', updateCartBadge);