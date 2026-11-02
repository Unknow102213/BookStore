
document.addEventListener("DOMContentLoaded", function () {
    let activeModal = null;
    let toastTimer = null;

    // Tạo thông báo
    const toast = document.createElement("div");
    toast.className = "cart-toast";
    toast.setAttribute("role", "status");
    toast.setAttribute("aria-live", "polite");
    document.body.appendChild(toast);

    function showToast(message, isError = false) {
        clearTimeout(toastTimer);

        toast.className = "cart-toast";
        if (isError) {
            toast.classList.add("is-error");
        }

        const icon = document.createElement("i");
        icon.className = isError
            ? "fa-solid fa-circle-exclamation"
            : "fa-solid fa-circle-check";

        const text = document.createElement("span");
        text.textContent = message;

        toast.replaceChildren(icon, text);

        requestAnimationFrame(function () {
            toast.classList.add("is-visible");
        });

        toastTimer = setTimeout(function () {
            toast.classList.remove("is-visible");
        }, 3000);
    }

    // Cập nhật số lượng và thành tiền
    function updateQuantity(modal, value) {
        const input = modal.querySelector(".cart-modal-quantity-input");
        const subtotalElement = modal.querySelector("[data-subtotal]");
        const hiddenQuantity = modal.querySelector('input[name="quantity"]');

        if (!input) return;

        const max = Number(input.max) || 1;
        let quantity = Number.parseInt(value, 10);

        if (!Number.isFinite(quantity)) quantity = 1;

        quantity = Math.max(1, Math.min(max, quantity));
        input.value = quantity;

        if (hiddenQuantity) {
            hiddenQuantity.value = quantity;
        }

        if (subtotalElement) {
            const price = Number(subtotalElement.dataset.subtotal) || 0;

            subtotalElement.textContent =
                (price * quantity).toLocaleString("vi-VN") + " đ";
        }
    }

    // Mở modal
    document.addEventListener("click", function (event) {
        const openButton = event.target.closest("[data-open-cart-modal]");

        if (openButton) {
            event.preventDefault();

            const bookId = openButton.dataset.openCartModal;
            const modal = document.getElementById("cart-modal-" + bookId);

            if (!modal) return;

            if (activeModal) {
                closeModal(activeModal);
            }

            activeModal = modal;
            modal.classList.add("is-open");
            modal.setAttribute("aria-hidden", "false");
            document.body.style.overflow = "hidden";

            updateQuantity(modal, 1);
            return;
        }

        // Đóng modal
        const closeButton = event.target.closest("[data-close-cart-modal]");

        if (closeButton) {
            closeModal(closeButton.closest(".cart-modal"));
            return;
        }

        // Bấm nút tăng hoặc giảm số lượng
        const quantityButton = event.target.closest("[data-quantity-change]");

        if (quantityButton) {
            const modal = quantityButton.closest(".cart-modal");
            const input = modal?.querySelector(".cart-modal-quantity-input");

            if (!modal || !input) return;

            const current = Number(input.value) || 1;
            const change = Number(quantityButton.dataset.quantityChange);

            updateQuantity(modal, current + change);
        }
    });

    function closeModal(modal) {
        if (!modal) return;

        modal.classList.remove("is-open");
        modal.setAttribute("aria-hidden", "true");
        document.body.style.overflow = "";

        if (activeModal === modal) {
            activeModal = null;
        }
    }

    // Đóng khi bấm ra ngoài hộp thoại
    document.addEventListener("click", function (event) {
        if (event.target.classList.contains("cart-modal-backdrop")) {
            closeModal(event.target.closest(".cart-modal"));
        }
    });

    // Xử lý khi nhập số lượng bằng bàn phím
    document.addEventListener("input", function (event) {
        if (!event.target.matches(".cart-modal-quantity-input")) return;

        const modal = event.target.closest(".cart-modal");
        if (modal) {
            updateQuantity(modal, event.target.value);
        }
    });

    // Gửi yêu cầu thêm giỏ hàng
    document.addEventListener("submit", async function (event) {
        const form = event.target.closest(".cart-modal-form");
        if (!form) return;

        event.preventDefault();

        const modal = form.closest(".cart-modal");
        const submitButton = form.querySelector(".cart-modal-submit");
        const input = modal?.querySelector(".cart-modal-quantity-input");
        const errorElement = modal?.querySelector(".cart-modal-error");

        if (!modal || !input) return;

        updateQuantity(modal, input.value);

        if (submitButton) {
            submitButton.disabled = true;
        }

        if (errorElement) {
            errorElement.textContent = "";
        }

        try {
            const response = await fetch(form.action, {
                method: "POST",
                body: new FormData(form),
                headers: {
                    "X-Requested-With": "XMLHttpRequest"
                },
                credentials: "same-origin"
            });

            const result = await response.json();

            if (result.redirectUrl) {
                window.location.href = result.redirectUrl;
                return;
            }

            if (!response.ok || !result.success) {
                const message = result.message ||
                    "Không thể thêm sách vào giỏ hàng.";

                if (errorElement) {
                    errorElement.textContent = message;
                }

                showToast(message, true);
                return;
            }

            // Cập nhật số lượng trên biểu tượng giỏ hàng
            const cartCount = document.getElementById("header-cart-count");

            if (cartCount && result.cartCount !== undefined) {
                cartCount.textContent = result.cartCount;
                cartCount.style.display = Number(result.cartCount) > 0 ? "inline-flex" : "none";
            }

            closeModal(modal);
            showToast(result.message || "Đã thêm sách vào giỏ hàng.");
        } catch (error) {
            showToast("Có lỗi xảy ra. Vui lòng thử lại.", true);
        } finally {
            if (submitButton) {
                submitButton.disabled = false;
            }
        }
    });

    // Đóng modal bằng phím Escape
    document.addEventListener("keydown", function (event) {
        if (event.key === "Escape" && activeModal) {
            closeModal(activeModal);
        }
    });
});
