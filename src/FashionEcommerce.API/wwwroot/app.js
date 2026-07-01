// State management
let token = localStorage.getItem("token") || "";
let currentUser = JSON.parse(localStorage.getItem("currentUser")) || null;
let currentTab = "dashboard";
let currentProductPage = 1;

// API Helper
async function apiCall(endpoint, options = {}) {
    const headers = {
        "Content-Type": "application/json",
    };
    if (token) {
        headers["Authorization"] = `Bearer ${token}`;
    }

    const config = {
        ...options,
        headers: {
            ...headers,
            ...options.headers,
        },
    };

    try {
        const response = await fetch(endpoint, config);
        
        // Handle 204 No Content
        if (response.status === 204) {
            return true;
        }

        // Handle 401 Unauthorized
        if (response.status === 401) {
            handleLogout();
            throw new Error("Phiên làm việc hết hạn. Vui lòng đăng nhập lại.");
        }

        const data = await response.json();
        
        if (!response.ok) {
            throw new Error(data.message || data || "Có lỗi xảy ra khi gửi yêu cầu.");
        }
        
        return data;
    } catch (error) {
        console.error(`API Call error at ${endpoint}:`, error);
        throw error;
    }
}

// Format Currency
function formatVND(value) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
}

// Format Date Time
function formatDate(dateString) {
    if (!dateString) return "-";
    const date = new Date(dateString);
    return date.toLocaleString('vi-VN', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit'
    });
}

// Initialize Application
document.addEventListener("DOMContentLoaded", () => {
    // Check initial session
    if (token && currentUser) {
        showApp();
    } else {
        showLogin();
    }

    // Bind Event Listeners
    document.getElementById("login-form").addEventListener("submit", handleLogin);
    document.getElementById("logout-btn").addEventListener("click", handleLogout);
    
    // Sidebar tabs
    document.querySelectorAll(".menu-item").forEach(item => {
        item.addEventListener("click", (e) => {
            e.preventDefault();
            const tabName = item.getAttribute("data-tab");
            switchTab(tabName);
        });
    });

    // Realtime Clock
    setInterval(() => {
        const now = new Date();
        document.getElementById("current-time").innerText = now.toLocaleTimeString('vi-VN');
    }, 1000);

    // Product events
    document.getElementById("add-product-btn").addEventListener("click", () => showProductModal());
    document.getElementById("product-modal-close-btn").addEventListener("click", closeProductModal);
    document.getElementById("product-modal-cancel-btn").addEventListener("click", closeProductModal);
    document.getElementById("product-form").addEventListener("submit", saveProduct);
    document.getElementById("product-search-input").addEventListener("input", debounce(searchProducts, 400));

    // Inventory events
    document.getElementById("stock-modal-close-btn").addEventListener("click", closeStockModal);
    document.getElementById("stock-modal-cancel-btn").addEventListener("click", closeStockModal);
    document.getElementById("stock-form").addEventListener("submit", saveStock);
    document.getElementById("inventory-search-input").addEventListener("input", debounce(searchInventory, 400));

    // User events
    document.getElementById("user-search-input").addEventListener("input", debounce(searchUsers, 400));

    // Order & Shipment events
    document.getElementById("order-search-input").addEventListener("input", debounce(searchOrders, 400));
    document.getElementById("order-modal-close-btn").addEventListener("click", closeOrderModal);
    
    // Shipment actions
    document.getElementById("btn-show-create-shipment").addEventListener("click", showShipmentForm);
    document.getElementById("btn-cancel-shipment-edit").addEventListener("click", hideShipmentForm);
    document.getElementById("shipment-form").addEventListener("submit", saveShipment);
    document.getElementById("btn-edit-shipment").addEventListener("click", () => showShipmentForm(true));
    document.getElementById("btn-show-add-event").addEventListener("click", showEventForm);
    document.getElementById("btn-cancel-event-add").addEventListener("click", hideEventForm);
    document.getElementById("shipment-event-form").addEventListener("submit", saveShipmentEvent);
    document.getElementById("save-order-status-btn").addEventListener("click", updateOrderStatus);
});

// Debounce helper
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Switch UI panels
function showLogin() {
    document.getElementById("login-container").classList.remove("hidden");
    document.getElementById("app-container").classList.add("hidden");
}

function showApp() {
    document.getElementById("login-container").classList.add("hidden");
    document.getElementById("app-container").classList.remove("hidden");
    
    // Bind current user name
    document.getElementById("nav-user-name").innerText = `${currentUser.firstName} ${currentUser.lastName}`;
    document.getElementById("nav-user-role").innerText = currentUser.role === "Admin" ? "Quản trị viên" : "Nhân viên";
    document.getElementById("nav-avatar").innerText = currentUser.firstName[0].toUpperCase();

    // Default load dashboard
    switchTab("dashboard");
}

// Login logic
async function handleLogin(e) {
    e.preventDefault();
    const email = document.getElementById("login-email").value.trim();
    const password = document.getElementById("login-password").value;
    const errorEl = document.getElementById("login-error");
    errorEl.innerText = "";

    try {
        const data = await apiCall("/api/Auth/login", {
            method: "POST",
            body: JSON.stringify({ email, password })
        });

        if (data.role === "Customer") {
            throw new Error("Tài khoản khách hàng không có quyền truy cập trang quản trị.");
        }

        token = data.token;
        currentUser = {
            id: data.userId,
            firstName: data.firstName,
            lastName: data.lastName,
            email: data.email,
            role: data.role
        };

        localStorage.setItem("token", token);
        localStorage.setItem("currentUser", JSON.stringify(currentUser));
        
        showApp();
    } catch (err) {
        errorEl.innerText = err.message || "Tên đăng nhập hoặc mật khẩu không chính xác.";
    }
}

// Logout logic
function handleLogout() {
    token = "";
    currentUser = null;
    localStorage.removeItem("token");
    localStorage.removeItem("currentUser");
    showLogin();
}

// Tab router switcher
function switchTab(tabName) {
    currentTab = tabName;
    
    // Toggle active menu class
    document.querySelectorAll(".menu-item").forEach(item => {
        if (item.getAttribute("data-tab") === tabName) {
            item.classList.add("active");
        } else {
            item.classList.remove("active");
        }
    });

    // Toggle panel visibility
    document.querySelectorAll(".tab-panel").forEach(panel => {
        if (panel.getAttribute("id") === `tab-${tabName}`) {
            panel.classList.add("active-panel");
        } else {
            panel.classList.remove("active-panel");
        }
    });

    // Update Title & Load data
    const titleEl = document.getElementById("current-tab-title");
    switch (tabName) {
        case "dashboard":
            titleEl.innerText = "Tổng Quan Hệ Thống";
            loadDashboard();
            break;
        case "products":
            titleEl.innerText = "Quản Lý Sản Phẩm";
            loadProducts(1);
            break;
        case "inventory":
            titleEl.innerText = "Kiểm Soát Kho Hàng";
            loadInventory();
            break;
        case "customers":
            titleEl.innerText = "Quản Lý Khách Hàng & Nhân Viên";
            loadCustomers();
            break;
        case "orders":
            titleEl.innerText = "Quản Lý Đơn Hàng & Giao Vận";
            loadOrders();
            break;
    }
}

// ----------------------------------------------------
// DASHBOARD LOGIC
// ----------------------------------------------------
async function loadDashboard() {
    try {
        const data = await apiCall("/api/Dashboard/overview");
        
        // Render stats card
        document.getElementById("dash-revenue").innerText = formatVND(data.summary.totalRevenue || 0);
        document.getElementById("dash-orders").innerText = data.summary.totalOrders || 0;
        document.getElementById("dash-products").innerText = data.summary.totalProducts || 0;
        document.getElementById("dash-users").innerText = data.summary.totalCustomers || 0;

        // Render recent orders
        const ordersList = document.getElementById("dash-recent-orders-list");
        if (data.recentOrders && data.recentOrders.length > 0) {
            ordersList.innerHTML = data.recentOrders.map(order => `
                <tr>
                    <td><strong>#${order.orderNumber}</strong></td>
                    <td>${order.customerName}</td>
                    <td>${formatVND(order.totalPrice)}</td>
                    <td>${getOrderStatusBadge(order.status)}</td>
                    <td>${formatDate(order.createdAt)}</td>
                </tr>
            `).join("");
        } else {
            ordersList.innerHTML = `<tr><td colspan="5" class="text-center text-muted">Không có đơn hàng gần đây.</td></tr>`;
        }

        // Render stock alerts
        const stockList = document.getElementById("dash-low-stock-list");
        if (data.stockAlerts && data.stockAlerts.length > 0) {
            stockList.innerHTML = data.stockAlerts.map(alert => `
                <div class="low-stock-item">
                    <div>
                        <div class="title">${alert.productName}</div>
                        <div class="sku">SKU: ${alert.sku}</div>
                    </div>
                    <div class="stock">Còn ${alert.availableQuantity} sp</div>
                </div>
            `).join("");
        } else {
            stockList.innerHTML = `<p class="text-center text-muted">Tồn kho ổn định. Không có cảnh báo tồn thấp.</p>`;
        }
    } catch (err) {
        console.error("Dashboard error", err);
    }
}

// ----------------------------------------------------
// PRODUCT MANAGEMENT LOGIC
// ----------------------------------------------------
async function loadProducts(page = 1) {
    currentProductPage = page;
    const tbody = document.getElementById("product-table-body");
    tbody.innerHTML = `<tr><td colspan="8" class="text-center">Đang tải sản phẩm...</td></tr>`;

    try {
        const res = await apiCall(`/api/Products?page=${page}&pageSize=8`);
        
        if (res.items && res.items.length > 0) {
            tbody.innerHTML = res.items.map(p => `
                <tr>
                    <td><img src="${p.imageUrl || 'https://placehold.co/100'}" class="product-thumb" alt="${p.name}"></td>
                    <td><strong>${p.name}</strong><br><small class="text-muted">${p.brand || 'No brand'}</small></td>
                    <td><code class="text-blue">${p.sku}</code></td>
                    <td>${p.brand || '-'}</td>
                    <td>${p.color || '-'}/${p.size || '-'}</td>
                    <td>${formatVND(p.price)}</td>
                    <td><span class="badge ${p.isActive ? 'bg-green' : 'bg-red'}">${p.isActive ? 'Bán' : 'Ngưng'}</span></td>
                    <td>
                        <button class="btn btn-secondary btn-sm" onclick="editProduct(${p.id})"><i class="fa-solid fa-edit"></i> Sửa</button>
                        <button class="btn btn-danger btn-sm" onclick="deleteProduct(${p.id})"><i class="fa-solid fa-trash"></i> Xóa</button>
                    </td>
                </tr>
            `).join("");

            renderPagination("product-pagination", res.page, res.totalPages, loadProducts);
        } else {
            tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted">Không tìm thấy sản phẩm nào.</td></tr>`;
            document.getElementById("product-pagination").innerHTML = "";
        }
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-red">Lỗi tải dữ liệu: ${err.message}</td></tr>`;
    }
}

async function searchProducts() {
    const term = document.getElementById("product-search-input").value.trim();
    if (!term) {
        loadProducts(1);
        return;
    }

    const tbody = document.getElementById("product-table-body");
    tbody.innerHTML = `<tr><td colspan="8" class="text-center">Đang tìm kiếm...</td></tr>`;

    try {
        const res = await apiCall(`/api/Products/search?searchTerm=${encodeURIComponent(term)}&page=1&pageSize=20`);
        const items = res.items || res.data?.items || res;
        
        if (items && items.length > 0) {
            tbody.innerHTML = items.map(p => `
                <tr>
                    <td><img src="${p.imageUrl || 'https://placehold.co/100'}" class="product-thumb" alt="${p.name}"></td>
                    <td><strong>${p.name}</strong><br><small class="text-muted">${p.brand || 'No brand'}</small></td>
                    <td><code class="text-blue">${p.sku}</code></td>
                    <td>${p.brand || '-'}</td>
                    <td>${p.color || '-'}/${p.size || '-'}</td>
                    <td>${formatVND(p.price)}</td>
                    <td><span class="badge ${p.isActive ? 'bg-green' : 'bg-red'}">${p.isActive ? 'Bán' : 'Ngưng'}</span></td>
                    <td>
                        <button class="btn btn-secondary btn-sm" onclick="editProduct(${p.id})"><i class="fa-solid fa-edit"></i> Sửa</button>
                        <button class="btn btn-danger btn-sm" onclick="deleteProduct(${p.id})"><i class="fa-solid fa-trash"></i> Xóa</button>
                    </td>
                </tr>
            `).join("");
            document.getElementById("product-pagination").innerHTML = "";
        } else {
            tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted">Không tìm thấy kết quả phù hợp.</td></tr>`;
        }
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-red">Lỗi tìm kiếm: ${err.message}</td></tr>`;
    }
}

function renderPagination(containerId, currentPage, totalPages, onPageClick) {
    const el = document.getElementById(containerId);
    let html = "";

    // Prev Button
    html += `<button class="page-btn" ${currentPage === 1 ? 'disabled' : ''} onclick="window.paginationClick('${containerId}', ${currentPage - 1})"><i class="fa-solid fa-chevron-left"></i></button>`;

    // Current & Total Info
    for (let i = 1; i <= totalPages; i++) {
        if (i === 1 || i === totalPages || (i >= currentPage - 1 && i <= currentPage + 1)) {
            html += `<button class="page-btn ${i === currentPage ? 'active' : ''}" onclick="window.paginationClick('${containerId}', ${i})">${i}</button>`;
        } else if (i === currentPage - 2 || i === currentPage + 2) {
            html += `<span style="padding: 0.25rem 0.5rem">...</span>`;
        }
    }

    // Next Button
    html += `<button class="page-btn" ${currentPage === totalPages ? 'disabled' : ''} onclick="window.paginationClick('${containerId}', ${currentPage + 1})"><i class="fa-solid fa-chevron-right"></i></button>`;

    el.innerHTML = html;
    window[`click_${containerId}`] = onPageClick;
}

window.paginationClick = (containerId, page) => {
    if (window[`click_${containerId}`]) {
        window[`click_${containerId}`](page);
    }
};

// Open product modal
function showProductModal(product = null) {
    const modal = document.getElementById("product-modal");
    const title = document.getElementById("product-modal-title");
    
    // Clear values
    document.getElementById("prod-id").value = "";
    document.getElementById("prod-name").value = "";
    document.getElementById("prod-sku").value = "";
    document.getElementById("prod-price").value = "";
    document.getElementById("prod-discount").value = "";
    document.getElementById("prod-category").value = "1";
    document.getElementById("prod-brand").value = "";
    document.getElementById("prod-color").value = "";
    document.getElementById("prod-size").value = "";
    document.getElementById("prod-material").value = "";
    document.getElementById("prod-image").value = "";
    document.getElementById("prod-desc").value = "";

    if (product) {
        title.innerText = "Cập Nhật Sản Phẩm";
        document.getElementById("prod-id").value = product.id;
        document.getElementById("prod-name").value = product.name;
        document.getElementById("prod-sku").value = product.sku;
        document.getElementById("prod-price").value = Math.round(product.price);
        document.getElementById("prod-discount").value = product.discountPrice ? Math.round(product.discountPrice) : "";
        document.getElementById("prod-category").value = product.categoryId;
        document.getElementById("prod-brand").value = product.brand || "";
        document.getElementById("prod-color").value = product.color || "";
        document.getElementById("prod-size").value = product.size || "";
        document.getElementById("prod-material").value = product.material || "";
        document.getElementById("prod-image").value = product.imageUrl || "";
        document.getElementById("prod-desc").value = product.description || "";
    } else {
        title.innerText = "Thêm Sản Phẩm Mới";
    }

    modal.classList.remove("hidden");
}

function closeProductModal() {
    document.getElementById("product-modal").classList.add("hidden");
}

async function editProduct(id) {
    try {
        const prod = await apiCall(`/api/Products/${id}`);
        showProductModal(prod);
    } catch (err) {
        alert("Không thể lấy dữ liệu sản phẩm: " + err.message);
    }
}

async function saveProduct(e) {
    e.preventDefault();
    const id = document.getElementById("prod-id").value;
    const name = document.getElementById("prod-name").value.trim();
    const sku = document.getElementById("prod-sku").value.trim();
    const price = parseFloat(document.getElementById("prod-price").value);
    const discountInput = document.getElementById("prod-discount").value;
    const discountPrice = discountInput ? parseFloat(discountInput) : null;
    const categoryId = parseInt(document.getElementById("prod-category").value);
    const brand = document.getElementById("prod-brand").value.trim();
    const color = document.getElementById("prod-color").value.trim();
    const size = document.getElementById("prod-size").value.trim();
    const material = document.getElementById("prod-material").value.trim();
    const imageUrl = document.getElementById("prod-image").value.trim();
    const description = document.getElementById("prod-desc").value.trim();

    const payload = {
        id: id ? parseInt(id) : 0,
        name,
        sku,
        price,
        discountPrice,
        categoryId,
        brand,
        color,
        size,
        material,
        imageUrl,
        description,
        isActive: true
    };

    try {
        if (id) {
            // Update
            await apiCall(`/api/Products/${id}`, {
                method: "PUT",
                body: JSON.stringify(payload)
            });
            alert("Cập nhật sản phẩm thành công!");
        } else {
            // Create
            await apiCall("/api/Products", {
                method: "POST",
                body: JSON.stringify(payload)
            });
            alert("Thêm sản phẩm mới thành công!");
        }
        closeProductModal();
        loadProducts(currentProductPage);
    } catch (err) {
        alert("Lỗi khi lưu sản phẩm: " + err.message);
    }
}

async function deleteProduct(id) {
    if (!confirm("Bạn có chắc chắn muốn xóa sản phẩm này?")) return;

    try {
        await apiCall(`/api/Products/${id}`, { method: "DELETE" });
        alert("Xóa sản phẩm thành công!");
        loadProducts(currentProductPage);
    } catch (err) {
        alert("Lỗi khi xóa sản phẩm: " + err.message);
    }
}

// ----------------------------------------------------
// INVENTORY CONTROL LOGIC
// ----------------------------------------------------
async function loadInventory() {
    const tbody = document.getElementById("inventory-table-body");
    tbody.innerHTML = `<tr><td colspan="8" class="text-center">Đang tải dữ liệu kho hàng...</td></tr>`;

    try {
        const inventories = await apiCall("/api/Inventories");
        
        if (inventories && inventories.length > 0) {
            tbody.innerHTML = inventories.map(inv => {
                const available = inv.quantity - inv.reservedQuantity;
                const isLow = available < 15;
                const rowClass = isLow ? 'style="background-color: rgba(245, 158, 11, 0.05)"' : '';
                const availableBadge = isLow 
                    ? `<span class="badge bg-orange">${available} (Thấp)</span>`
                    : `<span class="badge bg-green">${available}</span>`;

                return `
                    <tr ${rowClass}>
                        <td><strong>${inv.product?.name || 'Sản phẩm #' + inv.productId}</strong></td>
                        <td><code>${inv.product?.sku || '-'}</code></td>
                        <td>${inv.quantity}</td>
                        <td>${inv.reservedQuantity}</td>
                        <td>${availableBadge}</td>
                        <td>${inv.location || '-'}</td>
                        <td><small class="text-muted">${inv.notes || '-'}</small></td>
                        <td>
                            <button class="btn btn-primary btn-sm" onclick="editStock(${inv.id})">
                                <i class="fa-solid fa-pen-to-square"></i> Cập nhật kho
                            </button>
                        </td>
                    </tr>
                `;
            }).join("");
        } else {
            tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted">Kho hàng trống.</td></tr>`;
        }
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-red">Lỗi tải dữ liệu kho: ${err.message}</td></tr>`;
    }
}

async function searchInventory() {
    const term = document.getElementById("inventory-search-input").value.trim().toLowerCase();
    const rows = document.querySelectorAll("#inventory-table-body tr");
    
    rows.forEach(row => {
        const text = row.textContent.toLowerCase();
        if (text.includes(term)) {
            row.style.display = "";
        } else {
            row.style.display = "none";
        }
    });
}

async function editStock(id) {
    try {
        const inv = await apiCall(`/api/Inventories/${id}`);
        
        document.getElementById("stock-product-id").value = inv.productId;
        document.getElementById("stock-inventory-id").value = inv.id;
        document.getElementById("stock-product-name").value = inv.product?.name || `Sản phẩm #${inv.productId}`;
        document.getElementById("stock-quantity").value = inv.quantity;
        document.getElementById("stock-warehouse").value = inv.warehouseId || 1;
        document.getElementById("stock-location").value = inv.location || "";
        document.getElementById("stock-notes").value = inv.notes || "";

        document.getElementById("stock-modal").classList.remove("hidden");
    } catch (err) {
        alert("Lỗi tải thông tin tồn kho: " + err.message);
    }
}

function closeStockModal() {
    document.getElementById("stock-modal").classList.add("hidden");
}

async function saveStock(e) {
    e.preventDefault();
    const id = parseInt(document.getElementById("stock-inventory-id").value);
    const productId = parseInt(document.getElementById("stock-product-id").value);
    const quantity = parseInt(document.getElementById("stock-quantity").value);
    const warehouseId = parseInt(document.getElementById("stock-warehouse").value);
    const location = document.getElementById("stock-location").value.trim();
    const notes = document.getElementById("stock-notes").value.trim();

    const payload = {
        id,
        productId,
        quantity,
        warehouseId,
        location,
        notes
    };

    try {
        await apiCall(`/api/Inventories/${id}`, {
            method: "PUT",
            body: JSON.stringify(payload)
        });
        alert("Cập nhật kho hàng thành công!");
        closeStockModal();
        loadInventory();
    } catch (err) {
        alert("Lỗi cập nhật kho hàng: " + err.message);
    }
}

// ----------------------------------------------------
// CUSTOMER MANAGEMENT LOGIC
// ----------------------------------------------------
async function loadCustomers() {
    const tbody = document.getElementById("user-table-body");
    tbody.innerHTML = `<tr><td colspan="8" class="text-center">Đang tải danh sách người dùng...</td></tr>`;

    try {
        const users = await apiCall("/api/Users");
        
        if (users && users.length > 0) {
            tbody.innerHTML = users.map(user => {
                const selfId = currentUser.id.toString();
                const isSelf = user.id.toString() === selfId;
                
                // Block/Unlock switch
                const statusSwitch = isSelf 
                    ? `<span class="text-muted"><i class="fa-solid fa-user-shield"></i> Trực tuyến</span>`
                    : `
                        <label class="switch">
                            <input type="checkbox" ${user.isActive ? 'checked' : ''} onchange="toggleUserStatus(${user.id}, this.checked)">
                            <span class="slider"></span>
                        </label>
                    `;

                // Role Dropdown select
                const roleOptions = [
                    { id: 1, name: "Admin" },
                    { id: 2, name: "Customer" },
                    { id: 3, name: "Staff" },
                    { id: 4, name: "Manager" }
                ];
                
                const roleDropdown = isSelf 
                    ? `<span class="badge bg-purple">${user.role}</span>`
                    : `
                        <select onchange="changeUserRole(${user.id}, this.value)" style="padding: 0.25rem 0.5rem; background: var(--bg-darker); border: 1px solid var(--border-color); color: white; border-radius: 4px; font-size: 0.75rem;">
                            ${roleOptions.map(opt => `<option value="${opt.id}" ${user.role === opt.name ? 'selected' : ''}>${opt.name}</option>`).join("")}
                        </select>
                    `;

                return `
                    <tr>
                        <td><strong>${user.firstName} ${user.lastName}</strong> ${isSelf ? '<small class="text-blue">(Bạn)</small>' : ''}</td>
                        <td>${user.email}</td>
                        <td>${user.phoneNumber || '-'}</td>
                        <td>${roleDropdown}</td>
                        <td><span class="badge ${user.isActive ? 'bg-green' : 'bg-red'}">${user.isActive ? 'Hoạt động' : 'Đã khóa'}</span></td>
                        <td>${formatDate(user.createdAt)}</td>
                        <td>${formatDate(user.lastLoginAt)}</td>
                        <td>${statusSwitch}</td>
                    </tr>
                `;
            }).join("");
        } else {
            tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted">Không tìm thấy người dùng.</td></tr>`;
        }
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-red">Lỗi tải danh sách người dùng: ${err.message}</td></tr>`;
    }
}

async function toggleUserStatus(id, isActive) {
    try {
        await apiCall(`/api/Users/${id}/status`, {
            method: "PUT",
            body: JSON.stringify({ isActive })
        });
        loadCustomers();
    } catch (err) {
        alert("Lỗi cập nhật trạng thái người dùng: " + err.message);
        loadCustomers();
    }
}

async function changeUserRole(id, roleId) {
    try {
        await apiCall(`/api/Users/${id}/role`, {
            method: "PUT",
            body: JSON.stringify({ roleId: parseInt(roleId) })
        });
        alert("Thay đổi vai trò người dùng thành công!");
        loadCustomers();
    } catch (err) {
        alert("Lỗi khi thay đổi vai trò: " + err.message);
        loadCustomers();
    }
}

async function searchUsers() {
    const term = document.getElementById("user-search-input").value.trim();
    const tbody = document.getElementById("user-table-body");
    tbody.innerHTML = `<tr><td colspan="8" class="text-center">Đang tải...</td></tr>`;

    try {
        const users = await apiCall(`/api/Users?search=${encodeURIComponent(term)}`);
        
        if (users && users.length > 0) {
            tbody.innerHTML = users.map(user => {
                const selfId = currentUser.id.toString();
                const isSelf = user.id.toString() === selfId;
                const statusSwitch = isSelf 
                    ? `<span class="text-muted"><i class="fa-solid fa-user-shield"></i> Trực tuyến</span>`
                    : `
                        <label class="switch">
                            <input type="checkbox" ${user.isActive ? 'checked' : ''} onchange="toggleUserStatus(${user.id}, this.checked)">
                            <span class="slider"></span>
                        </label>
                    `;

                const roleOptions = [
                    { id: 1, name: "Admin" },
                    { id: 2, name: "Customer" },
                    { id: 3, name: "Staff" },
                    { id: 4, name: "Manager" }
                ];
                
                const roleDropdown = isSelf 
                    ? `<span class="badge bg-purple">${user.role}</span>`
                    : `
                        <select onchange="changeUserRole(${user.id}, this.value)" style="padding: 0.25rem 0.5rem; background: var(--bg-darker); border: 1px solid var(--border-color); color: white; border-radius: 4px; font-size: 0.75rem;">
                            ${roleOptions.map(opt => `<option value="${opt.id}" ${user.role === opt.name ? 'selected' : ''}>${opt.name}</option>`).join("")}
                        </select>
                    `;

                return `
                    <tr>
                        <td><strong>${user.firstName} ${user.lastName}</strong> ${isSelf ? '<small class="text-blue">(Bạn)</small>' : ''}</td>
                        <td>${user.email}</td>
                        <td>${user.phoneNumber || '-'}</td>
                        <td>${roleDropdown}</td>
                        <td><span class="badge ${user.isActive ? 'bg-green' : 'bg-red'}">${user.isActive ? 'Hoạt động' : 'Đã khóa'}</span></td>
                        <td>${formatDate(user.createdAt)}</td>
                        <td>${formatDate(user.lastLoginAt)}</td>
                        <td>${statusSwitch}</td>
                    </tr>
                `;
            }).join("");
        } else {
            tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted">Không tìm thấy kết quả.</td></tr>`;
        }
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-red">Lỗi: ${err.message}</td></tr>`;
    }
}

// ----------------------------------------------------
// ORDER & SHIPMENT LOGIC
// ----------------------------------------------------
let activeOrderId = null;
let activeShipmentId = null;

async function loadOrders() {
    const tbody = document.getElementById("order-table-body");
    tbody.innerHTML = `<tr><td colspan="8" class="text-center">Đang tải danh sách đơn hàng...</td></tr>`;

    try {
        const orders = await apiCall("/api/Orders");
        
        if (orders && orders.length > 0) {
            tbody.innerHTML = orders.map(order => {
                let shipmentBtn = `<span class="text-muted">Chưa tạo vận đơn</span>`;
                if (order.trackingNumber) {
                    shipmentBtn = `<span class="badge bg-blue" title="${order.trackingNumber}"><i class="fa-solid fa-truck"></i> ${order.trackingNumber}</span>`;
                }

                return `
                    <tr>
                        <td><strong>#${order.orderNumber}</strong></td>
                        <td>${order.user ? (order.user.firstName + ' ' + order.user.lastName) : 'Khách hàng'}</td>
                        <td>${order.phoneNumber || '-'}</td>
                        <td>${formatVND(order.totalPrice)}</td>
                        <td>${getOrderStatusBadge(order.status)}</td>
                        <td>${formatDate(order.createdAt)}</td>
                        <td>${shipmentBtn}</td>
                        <td>
                            <button class="btn btn-primary btn-sm" onclick="viewOrderDetails(${order.id})">
                                Chi tiết & Giao vận <i class="fa-solid fa-circle-info"></i>
                            </button>
                        </td>
                    </tr>
                `;
            }).join("");
        } else {
            tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted">Không có đơn hàng nào.</td></tr>`;
        }
    } catch (err) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-red">Lỗi tải danh sách đơn hàng: ${err.message}</td></tr>`;
    }
}

async function searchOrders() {
    const term = document.getElementById("order-search-input").value.trim().toLowerCase();
    const rows = document.querySelectorAll("#order-table-body tr");
    
    rows.forEach(row => {
        const orderNumberCell = row.cells[0]?.textContent.toLowerCase();
        const customerCell = row.cells[1]?.textContent.toLowerCase();
        if (orderNumberCell.includes(term) || customerCell.includes(term)) {
            row.style.display = "";
        } else {
            row.style.display = "none";
        }
    });
}

function getOrderStatusBadge(status) {
    switch (status) {
        case 0: return `<span class="badge bg-secondary">Chờ duyệt</span>`;
        case 1: return `<span class="badge bg-orange">Đang xử lý</span>`;
        case 2: return `<span class="badge bg-blue">Đã gửi hàng</span>`;
        case 3: return `<span class="badge bg-green">Đã giao</span>`;
        case 4: return `<span class="badge bg-red">Đã hủy</span>`;
        case 5: return `<span class="badge bg-purple">Đã trả hàng</span>`;
        default: return `<span class="badge bg-secondary">Không rõ</span>`;
    }
}

function getShipmentStatusText(status) {
    switch (status) {
        case 0: return "Đang khởi tạo (Created)";
        case 1: return "Đang đóng gói (Packing)";
        case 2: return "Sẵn sàng gửi đi (Ready To Ship)";
        case 3: return "Đang vận chuyển (In Transit)";
        case 4: return "Đang giao hàng (Out For Delivery)";
        case 5: return "Đã giao hàng thành công (Delivered)";
        case 6: return "Giao hàng thất bại (Failed)";
        case 7: return "Đã trả lại kho (Returned)";
        case 8: return "Đã hủy vận đơn (Cancelled)";
        default: return "Không rõ";
    }
}

async function viewOrderDetails(id) {
    activeOrderId = id;
    activeShipmentId = null;
    
    // Clear views
    document.getElementById("order-modal-title").innerText = `Đang tải đơn hàng #${id}...`;
    document.getElementById("ord-items-list").innerHTML = "Đang tải...";
    
    hideShipmentForm();
    hideEventForm();

    try {
        const order = await apiCall(`/api/Orders/${id}`);
        document.getElementById("order-modal-title").innerText = `Chi Tiết Đơn Hàng #ORD-${order.orderNumber}`;
        
        // Buyer Info
        document.getElementById("ord-cust-name").innerText = order.user ? `${order.user.firstName} ${order.user.lastName}` : "Khách vãng lai";
        document.getElementById("ord-cust-phone").innerText = order.phoneNumber || "-";
        document.getElementById("ord-cust-address").innerText = order.shippingAddress || "-";
        document.getElementById("ord-cust-city").innerText = [order.city, order.state, order.country].filter(Boolean).join(", ") || "-";
        document.getElementById("ord-date").innerText = formatDate(order.createdAt);
        document.getElementById("ord-notes").innerText = order.notes || "Không có ghi chú.";

        // Order Status Select
        document.getElementById("update-order-status-select").value = order.status;

        // Items
        if (order.items && order.items.length > 0) {
            document.getElementById("ord-items-list").innerHTML = order.items.map(item => `
                <div class="order-item-row">
                    <div class="order-item-info">
                        <span class="order-item-name">${item.product?.name || 'Sản phẩm #' + item.productId}</span>
                        <span class="order-item-meta">Size: ${item.size || 'Mặc định'}, Màu: ${item.color || 'Mặc định'}</span>
                    </div>
                    <div><strong>${item.quantity}</strong> x ${formatVND(item.unitPrice)}</div>
                </div>
            `).join("");
        } else {
            document.getElementById("ord-items-list").innerHTML = '<div class="text-center text-muted">Không có mặt hàng nào.</div>';
        }

        // Summary
        document.getElementById("ord-subtotal").innerText = formatVND(order.subTotal);
        document.getElementById("ord-shipping-cost").innerText = formatVND(order.shippingCost);
        document.getElementById("ord-discount").innerText = `-${formatVND(order.discountAmount)}`;
        document.getElementById("ord-total").innerText = formatVND(order.totalPrice);

        // Load shipment info
        await loadShipmentInfo(id);

        document.getElementById("order-modal").classList.remove("hidden");
    } catch (err) {
        alert("Lỗi tải thông tin đơn hàng: " + err.message);
    }
}

function closeOrderModal() {
    document.getElementById("order-modal").classList.add("hidden");
    loadOrders();
}

// Load Shipment Info & Events
async function loadShipmentInfo(orderId) {
    const noShipment = document.getElementById("no-shipment-message");
    const shipmentInfo = document.getElementById("shipment-info-container");
    
    noShipment.classList.add("hidden");
    shipmentInfo.classList.add("hidden");

    try {
        const shipment = await apiCall(`/api/Shipments/order/${orderId}`);
        activeShipmentId = shipment.id;

        // Populate info UI
        document.getElementById("info-ship-carrier").innerText = shipment.carrierName;
        document.getElementById("info-ship-tracking").innerText = shipment.trackingNumber || "Chưa có";
        document.getElementById("info-ship-fee").innerText = formatVND(shipment.shippingFee);
        document.getElementById("info-ship-est-delivery").innerText = formatDate(shipment.estimatedDeliveryDate);
        
        const statusBadge = document.getElementById("info-ship-status");
        statusBadge.innerText = getShipmentStatusText(shipment.status);
        statusBadge.className = `badge ${getShipmentStatusBadgeClass(shipment.status)}`;

        // Render events timeline
        const timeline = document.getElementById("shipment-timeline");
        if (shipment.events && shipment.events.length > 0) {
            timeline.innerHTML = shipment.events.map((ev, idx) => `
                <div class="timeline-event ${idx === 0 ? 'active-event' : ''}">
                    <div class="event-time">${formatDate(ev.occurredAt)}</div>
                    <div class="event-details">${getShipmentStatusText(ev.status)}</div>
                    <div class="event-meta">
                        ${ev.location ? `<i class="fa-solid fa-map-pin"></i> ${ev.location}` : ''} 
                        ${ev.note ? `| <i class="fa-solid fa-memo"></i> Ghi chú: ${ev.note}` : ''}
                    </div>
                </div>
            `).join("");
        } else {
            timeline.innerHTML = '<p class="text-muted text-center">Chưa có cập nhật hành trình nào.</p>';
        }

        shipmentInfo.classList.remove("hidden");
    } catch (err) {
        // 404 means no shipment exists yet
        noShipment.classList.remove("hidden");
    }
}

function getShipmentStatusBadgeClass(status) {
    switch (status) {
        case 0: return "bg-secondary";
        case 1:
        case 2: return "bg-purple";
        case 3:
        case 4: return "bg-orange";
        case 5: return "bg-green";
        case 6:
        case 8: return "bg-red";
        case 7: return "bg-secondary";
        default: return "bg-secondary";
    }
}

// Actions inside Shipment UI
function showShipmentForm(isEdit = false) {
    document.getElementById("no-shipment-message").classList.add("hidden");
    document.getElementById("shipment-info-container").classList.add("hidden");
    document.getElementById("shipment-form-container").classList.remove("hidden");

    if (isEdit && activeShipmentId) {
        // Populate inputs with current values
        document.getElementById("ship-carrier").value = document.getElementById("info-ship-carrier").innerText;
        const tracking = document.getElementById("info-ship-tracking").innerText;
        document.getElementById("ship-tracking").value = tracking === "Chưa có" ? "" : tracking;
        
        const feeStr = document.getElementById("info-ship-fee").innerText.replace(/[^0-9]/g, '');
        document.getElementById("ship-fee").value = parseInt(feeStr) || 0;
        
        // Est date
        document.getElementById("ship-est-delivery").value = "";
    } else {
        document.getElementById("ship-carrier").value = "";
        document.getElementById("ship-tracking").value = "";
        document.getElementById("ship-fee").value = "0";
        document.getElementById("ship-est-delivery").value = "";
        document.getElementById("ship-notes").value = "";
    }
}

function hideShipmentForm() {
    document.getElementById("shipment-form-container").classList.add("hidden");
    if (activeShipmentId) {
        document.getElementById("shipment-info-container").classList.remove("hidden");
    } else {
        document.getElementById("no-shipment-message").classList.remove("hidden");
    }
}

async function saveShipment(e) {
    e.preventDefault();
    const carrierName = document.getElementById("ship-carrier").value.trim();
    const trackingNumber = document.getElementById("ship-tracking").value.trim();
    const shippingFee = parseFloat(document.getElementById("ship-fee").value) || 0;
    const estDelivery = document.getElementById("ship-est-delivery").value;
    const notes = document.getElementById("ship-notes").value.trim();

    const payload = {
        orderId: activeOrderId,
        carrierName,
        trackingNumber: trackingNumber || null,
        shippingFee,
        estimatedDeliveryDate: estDelivery ? new Date(estDelivery).toISOString() : null,
        notes: notes || null
    };

    try {
        if (activeShipmentId) {
            // Update existing shipment
            await apiCall(`/api/Shipments/${activeShipmentId}`, {
                method: "PUT",
                body: JSON.stringify(payload)
            });
            alert("Cập nhật vận đơn thành công!");
        } else {
            // Create new shipment
            await apiCall("/api/Shipments", {
                method: "POST",
                body: JSON.stringify(payload)
            });
            alert("Tạo vận đơn giao hàng thành công!");
        }
        hideShipmentForm();
        loadShipmentInfo(activeOrderId);
    } catch (err) {
        alert("Lỗi lưu vận đơn: " + err.message);
    }
}

// Event Tracking History triggers
function showEventForm() {
    document.getElementById("shipment-event-form-container").classList.remove("hidden");
}

function hideEventForm() {
    document.getElementById("shipment-event-form-container").classList.add("hidden");
}

async function saveShipmentEvent(e) {
    e.preventDefault();
    const status = parseInt(document.getElementById("event-status").value);
    const location = document.getElementById("event-location").value.trim();
    const note = document.getElementById("event-note").value.trim();

    const payload = {
        status,
        location: location || null,
        note: note || null,
        occurredAt: new Date().toISOString()
    };

    try {
        await apiCall(`/api/Shipments/${activeShipmentId}/events`, {
            method: "POST",
            body: JSON.stringify(payload)
        });
        alert("Cập nhật hành trình thành công!");
        hideEventForm();
        loadShipmentInfo(activeOrderId);
    } catch (err) {
        alert("Lỗi cập nhật hành trình: " + err.message);
    }
}

// Update Order status directly
async function updateOrderStatus() {
    const status = parseInt(document.getElementById("update-order-status-select").value);
    try {
        await apiCall(`/api/Orders/${activeOrderId}/status`, {
            method: "PUT",
            body: JSON.stringify({ status })
        });
        alert("Cập nhật trạng thái đơn hàng thành công!");
        viewOrderDetails(activeOrderId);
    } catch (err) {
        alert("Lỗi cập nhật trạng thái đơn hàng: " + err.message);
    }
}
