# 📊 Database Schema Documentation

## Entity Relationship Diagram (ERD)

```
┌─────────────────┐            ┌─────────────────┐
│     ROLES       │ (1:N)      │      USERS      │
├─────────────────┤◄───────────┼─────────────────┤
│ id (PK)         │            │ id (PK)         │
│ roleName        │            │ roleId (FK)     │
│ description     │            │ firstName       │
└────────┬────────┘            │ lastName        │
         │                     │ email (UNIQUE)  │
         │ (1:N)               │ passwordHash    │
┌────────▼────────┐            │ phoneNumber     │
│ ROLE_PERMISSION │            │ isActive        │
├─────────────────┤            │ lastLoginAt     │
│ roleId (PK,FK)  │            └────────┬───┬───┬┘
│ permissionId(PK)│                     │   │   │
└────────▲────────┘               (1:N) │   │   │ (1:1)   (1:N)
         │                              │   │   └──────────────┐
┌────────┴────────┐                     │   │                  │
│   PERMISSIONS   │                     │   │            ┌─────▼─────┐
├─────────────────┤                     │   │            │   CARTS   │
│ id (PK)         │                     │   │            ├───────────┤
│ actionName      │                     │   │            │ id (PK)   │
│ description     │                     │   │            │ userId(FK)│
└─────────────────┘                     │   │            └─────┬─────┘
                                  (1:N) │   │                  │ (1:N)
         ┌──────────────────────────────┘   │                  │
┌────────▼────────┐                         │            ┌─────▼─────┐
│ USER_ADDRESSES  │                         │            │ CART_ITEMS│
├─────────────────┤                         │            ├───────────┤
│ id (PK)         │                         │            │ id (PK)   │
│ userId (FK)     │                         │            │ cartId(FK)│
│ receiverName    │                         │            │productIdFK│
│ phone           │                         │            └─────┬─────┘
│ addressLine     │                         │                  │ (N:1)
│ ward            │                         │                  │
│ district        │                         │                  │
│ province        │                         │                  │
│ isDefault       │                         │                  │
└─────────────────┘                         │                  │
                                            │                  │
         ┌──────────────────────────────────┘                  │
┌────────▼────────┐                                            │
│     ORDERS      │                                            │
├─────────────────┤                                            │
│ id (PK)         │                                            │
│ userId (FK)     │                                            │
│ orderNumber     │                                            │
│ status          │                                            │
│ totalPrice      │                                            │
└────────┬───┬────┘                                            │
         │   │                                                 │
   (1:1) │   │ (1:N)                                           │
┌────────▼──┐└──────────────────────────┐                      │
│ SHIPMENTS │                           │                      │
├───────────┤                     ┌─────▼─────┐                │
│ id (PK)   │                     │ORDER_ITEMS│                │
│ orderId(FK│                     ├───────────┤                │
│ carrierNam│                     │ id (PK)   │                │
│ trackingNu│                     │ orderId(FK│                │
│ status    │                     │productIdFK│                │
└────┬──────┘                     └─────┬─────┘                │
     │ (1:N)                            │ (N:1)                │
┌────▼──────────┐                       │                      │
│SHIPMENT_EVENTS│                       │                      │
├───────────────┤                       │                      │
│ id (PK)       │                       │                      │
│ shipmentId(FK)│                       │                      │
│ status        │                       │                      │
│ location      │                       │                      │
│ note          │                       │                      │
└───────────────┘                       │                      │
                                        │                      │
         ┌──────────────────────────────┴──────────────────────┘
┌────────▼────────┐
│    PRODUCTS     │
├─────────────────┤ (1:N) ┌─────────────────┐
│ id (PK)         │──────►│   INVENTORIES   │
│ name            │       ├─────────────────┤
│ description     │ (1:N) │ id (PK)         │
│ price           │──────►│ productId (FK)  │
│ sku (UNIQUE)    │       │ quantity        │
│ brand           │ (1:N) │ reservedQuantity│
│ categoryId (FK) │──┐    └─────────────────┘
└────────┬────────┘  │    ┌─────────────────┐
         │           │    │PRODUCT_VARIANTS │
         │           └───►├─────────────────┤
         │                │ id (PK)         │
         │                │ productId (FK)  │
         │                │ sku (UNIQUE)    │
         │                │ color, size     │
         │                │ priceOverride   │
         │                └─────────────────┘
         │                ┌─────────────────┐
         └───────────────►│ PRODUCT_IMAGES  │
                          ├─────────────────┤
                          │ id (PK)         │
                          │ productId (FK)  │
                          │ imageUrl        │
                          │ isThumbnail     │
                          └─────────────────┘
```

---

## 📊 Table Specifications

### ROLES Table
Stores authorization roles within the application.

| Column      | Type          | Constraints        | Notes                      |
| ----------- | ------------- | ------------------ | -------------------------- |
| id          | INT           | PK, Auto-increment | Primary Key                |
| roleName    | NVARCHAR(50)  | NOT NULL, UNIQUE   | Role name (e.g. Admin)     |
| description | NVARCHAR(MAX) | Nullable           | Role purpose / description |
| createdAt   | DATETIME2     | Default: UTC       | Creation timestamp         |
| updatedAt   | DATETIME2     | Nullable           | Last update timestamp      |
| isDeleted   | BIT           | Default: 0         | Soft delete flag           |

---

### PERMISSIONS Table
Stores granular authorization permissions.

| Column      | Type          | Constraints        | Notes                      |
| ----------- | ------------- | ------------------ | -------------------------- |
| id          | INT           | PK, Auto-increment | Primary Key                |
| actionName  | NVARCHAR(100) | NOT NULL, UNIQUE   | Permission tag (e.g. products.view) |
| description | NVARCHAR(MAX) | Nullable           | Description of permission  |
| createdAt   | DATETIME2     | Default: UTC       | Creation timestamp         |
| updatedAt   | DATETIME2     | Nullable           | Last update timestamp      |
| isDeleted   | BIT           | Default: 0         | Soft delete flag           |

---

### ROLE_PERMISSIONS Table
Junction table implementing the Many-to-Many relationship between Roles and Permissions.

| Column       | Type | Constraints    | Notes                          |
| ------------ | ---- | -------------- | ------------------------------ |
| roleId       | INT  | PK, FK         | Reference to Roles table       |
| permissionId | INT  | PK, FK         | Reference to Permissions table |

---

### USERS Table
Stores application users (customers, staff, admins).

| Column                   | Type          | Constraints           | Notes                        |
| ------------------------ | ------------- | --------------------- | ---------------------------- |
| id                       | INT           | PK, Auto-increment    | Primary Key                  |
| roleId                   | INT           | NOT NULL, FK, Default | 1=Admin, 2=Customer, 3=Staff, 4=Manager |
| firstName                | NVARCHAR(100) | NOT NULL              | User's first name            |
| lastName                 | NVARCHAR(100) | NOT NULL              | User's last name             |
| email                    | NVARCHAR(255) | NOT NULL, UNIQUE      | Email address (login username) |
| passwordHash             | NVARCHAR(255) | NOT NULL              | Hashed password              |
| passwordResetToken       | NVARCHAR(255) | Nullable              | Token for password resetting |
| passwordResetTokenExpiry | DATETIME2     | Nullable              | Reset token expiry timestamp |
| phoneNumber              | NVARCHAR(20)  | Nullable              | Contact phone number         |
| isActive                 | BIT           | Default: 1            | Active status flag           |
| emailVerifiedAt          | DATETIME2     | Nullable              | Email verification timestamp |
| lastLoginAt              | DATETIME2     | Nullable              | Last login timestamp         |
| createdAt                | DATETIME2     | Default: UTC          | Creation timestamp         |
| updatedAt                | DATETIME2     | Nullable              | Last update timestamp      |
| isDeleted                | BIT           | Default: 0            | Soft delete flag             |

---

### USER_ADDRESSES Table
Stores multiple addresses for users (customers).

| Column       | Type          | Constraints        | Notes                       |
| ------------ | ------------- | ------------------ | --------------------------- |
| id           | INT           | PK, Auto-increment | Primary Key                 |
| userId       | INT           | NOT NULL, FK       | Reference to Users table    |
| receiverName | NVARCHAR(100) | NOT NULL           | Consignee name              |
| phone        | NVARCHAR(20)  | NOT NULL           | Delivery contact number     |
| addressLine  | NVARCHAR(500) | NOT NULL           | Street address detail       |
| ward         | NVARCHAR(100) | Nullable           | Ward name                   |
| district     | NVARCHAR(100) | Nullable           | District name               |
| province     | NVARCHAR(100) | Nullable           | Province/City name          |
| isDefault    | BIT           | Default: 0         | Marks primary user address  |
| createdAt    | DATETIME2     | Default: UTC       | Creation timestamp          |
| updatedAt    | DATETIME2     | Nullable           | Last update timestamp       |
| isDeleted    | BIT           | Default: 0         | Soft delete flag            |

---

### CATEGORIES Table
Stores product categories in a hierarchical structure.

| Column           | Type          | Constraints           | Notes                             |
| ---------------- | ------------- | --------------------- | --------------------------------- |
| id               | INT           | PK, Auto-increment    | Primary Key                       |
| name             | NVARCHAR(100) | NOT NULL              | Category name                     |
| description      | NVARCHAR(500) | Nullable              | Category description              |
| imageUrl         | NVARCHAR(MAX) | Nullable              | Category visual banner            |
| parentCategoryId | INT           | FK (Nullable)         | Self-referential FK for hierarchy |
| isActive         | BIT           | Default: 1            | Category activation status        |
| createdAt        | DATETIME2     | Default: UTC          | Creation timestamp                |
| updatedAt        | DATETIME2     | Nullable              | Last update timestamp             |
| isDeleted        | BIT           | Default: 0            | Soft delete flag                  |

---

### PRODUCTS Table
Stores clothing items and primary product metadata.

| Column        | Type           | Constraints           | Notes                         |
| ------------- | -------------- | --------------------- | ----------------------------- |
| id            | INT            | PK, Auto-increment    | Primary Key                   |
| name          | NVARCHAR(255)  | NOT NULL              | Product name                  |
| description   | NVARCHAR(1000) | Nullable              | Detailed details              |
| price         | DECIMAL(10,2)  | NOT NULL              | Base retail price             |
| discountPrice | DECIMAL(10,2)  | Nullable              | Sale price                    |
| categoryId    | INT            | NOT NULL, FK          | Reference to Categories table |
| sku           | NVARCHAR(100)  | Nullable, UNIQUE      | Base product SKU              |
| brand         | NVARCHAR(50)   | Nullable              | Brand name                    |
| color         | NVARCHAR(50)   | Nullable              | Default variant color         |
| size          | NVARCHAR(50)   | Nullable              | Default variant size          |
| material      | NVARCHAR(50)   | Nullable              | Product fabric / material     |
| imageUrl      | NVARCHAR(MAX)  | Nullable              | Default product image URL     |
| rating        | DECIMAL(5,2)   | Range: 0-5            | Average rating                |
| reviewCount   | INT            | Default: 0            | Total reviews                 |
| isActive      | BIT            | Default: 1            | Availability toggle           |
| createdAt     | DATETIME2      | Default: UTC          | Creation timestamp            |
| updatedAt     | DATETIME2      | Nullable              | Last update timestamp         |
| isDeleted     | BIT            | Default: 0            | Soft delete flag              |

---

### PRODUCT_VARIANTS Table
Stores specific sizing, coloring, and SKU overrides for items.

| Column        | Type          | Constraints        | Notes                       |
| ------------- | ------------- | ------------------ | --------------------------- |
| id            | INT           | PK, Auto-increment | Primary Key                 |
| productId     | INT           | NOT NULL, FK       | Reference to Products table |
| sku           | NVARCHAR(100) | NOT NULL, UNIQUE   | Unique SKU for this variant |
| color         | NVARCHAR(50)  | Nullable           | Variant color               |
| size          | NVARCHAR(50)  | Nullable           | Variant size                |
| priceOverride | DECIMAL(10,2) | Nullable           | Alternative price tag       |
| createdAt     | DATETIME2     | Default: UTC       | Creation timestamp          |
| updatedAt     | DATETIME2     | Nullable           | Last update timestamp       |
| isDeleted     | BIT           | Default: 0         | Soft delete flag            |

---

### PRODUCT_IMAGES Table
Stores photo gallery urls for product listings.

| Column      | Type          | Constraints        | Notes                       |
| ----------- | ------------- | ------------------ | --------------------------- |
| id          | INT           | PK, Auto-increment | Primary Key                 |
| productId   | INT           | NOT NULL, FK       | Reference to Products table |
| imageUrl    | NVARCHAR(MAX) | NOT NULL           | Path or URL of the image    |
| isThumbnail | BIT           | Default: 0         | Primary image indicator     |
| createdAt   | DATETIME2     | Default: UTC       | Creation timestamp          |
| updatedAt   | DATETIME2     | Nullable           | Last update timestamp       |
| isDeleted   | BIT           | Default: 0         | Soft delete flag            |

---

### INVENTORIES Table
Manages stock levels of products across locations.

| Column           | Type          | Constraints           | Notes                         |
| ---------------- | ------------- | --------------------- | ----------------------------- |
| id               | INT           | PK, Auto-increment    | Primary Key                   |
| productId        | INT           | NOT NULL, FK          | Reference to Products table   |
| quantity         | INT           | NOT NULL              | Total items physical count    |
| reservedQuantity | INT           | Default: 0            | Reserved items (in pending orders) |
| warehouseId      | INT           | Nullable              | Warehouse reference           |
| location         | NVARCHAR(100) | Nullable              | Physical layout shelf/aisle   |
| lastRestockDate  | DATETIME2     | Nullable              | Last stock replenishment date |
| notes            | NVARCHAR(500) | Nullable              | Restock/audit details         |
| createdAt        | DATETIME2     | Default: UTC          | Creation timestamp            |
| updatedAt        | DATETIME2     | Nullable              | Last update timestamp         |
| isDeleted        | BIT           | Default: 0            | Soft delete flag              |

---

### CARTS Table
Shopping carts belonging to users.

| Column     | Type          | Constraints           | Notes                        |
| ---------- | ------------- | --------------------- | ---------------------------- |
| id         | INT           | PK, Auto-increment    | Primary Key                  |
| userId     | INT           | NOT NULL, FK, UNIQUE  | Reference to Users table     |
| totalPrice | DECIMAL(10,2) | Default: 0            | Dynamic sum value of items   |
| itemCount  | INT           | Default: 0            | Number of distinct items     |
| createdAt  | DATETIME2     | Default: UTC          | Creation timestamp           |
| updatedAt  | DATETIME2     | Nullable              | Last update timestamp        |
| isDeleted  | BIT           | Default: 0            | Soft delete flag             |

---

### CART_ITEMS Table
Shopping cart details listing products and quantities.

| Column     | Type          | Constraints           | Notes                         |
| ---------- | ------------- | --------------------- | ----------------------------- |
| id         | INT           | PK, Auto-increment    | Primary Key                   |
| cartId     | INT           | NOT NULL, FK          | Reference to Carts table      |
| productId  | INT           | NOT NULL, FK          | Reference to Products table   |
| quantity   | INT           | NOT NULL              | Item quantity                 |
| unitPrice  | DECIMAL(10,2) | NOT NULL              | Price per unit during addition|
| totalPrice | DECIMAL(10,2) | NOT NULL              | Product total price           |
| createdAt  | DATETIME2     | Default: UTC          | Creation timestamp            |
| updatedAt  | DATETIME2     | Nullable              | Last update timestamp         |
| isDeleted  | BIT           | Default: 0            | Soft delete flag              |

---

### ORDERS Table
Tracks consumer purchases and checkout outcomes.

| Column          | Type          | Constraints           | Notes                            |
| --------------- | ------------- | --------------------- | -------------------------------- |
| id              | INT           | PK, Auto-increment    | Primary Key                      |
| userId          | INT           | NOT NULL, FK          | Reference to Users table         |
| orderNumber     | NVARCHAR(50)  | NOT NULL, UNIQUE      | Generated purchase number        |
| status          | INT           | Default: 0            | Pending=0, Processing=1, Shipped=2, Delivered=3, Cancelled=4, Returned=5 |
| subTotal        | DECIMAL(10,2) | NOT NULL              | Sum of items value               |
| shippingCost    | DECIMAL(10,2) | Default: 0            | Delivery charge                  |
| taxAmount       | DECIMAL(10,2) | Default: 0            | Applied taxes                    |
| discountAmount  | DECIMAL(10,2) | Default: 0            | Promo code/rewards discount      |
| totalPrice      | DECIMAL(10,2) | NOT NULL              | Checkout total charge            |
| shippingAddress | NVARCHAR(500) | Nullable              | Delivery address string          |
| city            | NVARCHAR(100) | Nullable              | Shipping city                    |
| state           | NVARCHAR(100) | Nullable              | Shipping region                  |
| postalCode      | NVARCHAR(20)  | Nullable              | Shipping zip code                |
| country         | NVARCHAR(100) | Nullable              | Shipping nation                  |
| phoneNumber     | NVARCHAR(20)  | Nullable              | Shipping contact phone           |
| notes           | NVARCHAR(500) | Nullable              | Delivery note / requests         |
| shippedDate     | DATETIME2     | Nullable              | Timestamp shipped                |
| deliveredDate   | DATETIME2     | Nullable              | Timestamp delivered              |
| trackingNumber  | NVARCHAR(100) | Nullable              | Carrier tracing code             |
| createdAt       | DATETIME2     | Default: UTC          | Creation timestamp               |
| updatedAt       | DATETIME2     | Nullable              | Last update timestamp            |
| isDeleted       | BIT           | Default: 0            | Soft delete flag                 |

---

### ORDER_ITEMS Table
Items contained in customer orders.

| Column     | Type          | Constraints           | Notes                         |
| ---------- | ------------- | --------------------- | ----------------------------- |
| id         | INT           | PK, Auto-increment    | Primary Key                   |
| orderId    | INT           | NOT NULL, FK          | Reference to Orders table     |
| productId  | INT           | NOT NULL, FK          | Reference to Products table   |
| quantity   | INT           | NOT NULL              | Order quantity                |
| unitPrice  | DECIMAL(10,2) | NOT NULL              | Unit price during checkout    |
| totalPrice | DECIMAL(10,2) | NOT NULL              | Total cost for item line      |
| size       | NVARCHAR(100) | Nullable              | Selected variant size         |
| color      | NVARCHAR(100) | Nullable              | Selected variant color        |
| createdAt  | DATETIME2     | Default: UTC          | Creation timestamp            |
| updatedAt  | DATETIME2     | Nullable              | Last update timestamp         |
| isDeleted  | BIT           | Default: 0            | Soft delete flag              |

---

### SHIPMENTS Table
Stores logistics records mapping orders to carrier deliveries.

| Column                | Type          | Constraints           | Notes                          |
| --------------------- | ------------- | --------------------- | ------------------------------ |
| id                    | INT           | PK, Auto-increment    | Primary Key                    |
| orderId               | INT           | NOT NULL, FK, UNIQUE  | Reference to Orders table      |
| carrierName           | NVARCHAR(100) | NOT NULL              | Courier name (e.g. GHN, Viettel)|
| trackingNumber        | NVARCHAR(100) | Nullable              | Shipment tracking code         |
| status                | INT           | Default: 0            | Created=0, Packing=1, ReadyToShip=2, InTransit=3, OutForDelivery=4, Delivered=5, FailedDelivery=6, Returned=7, Cancelled=8 |
| shippingFee           | DECIMAL(10,2) | Default: 0            | Shipping cost                  |
| estimatedDeliveryDate | DATETIME2     | Nullable              | Target delivery date           |
| shippedAt             | DATETIME2     | Nullable              | Handover to carrier date       |
| deliveredAt           | DATETIME2     | Nullable              | Delivery success date          |
| notes                 | NVARCHAR(500) | Nullable              | Logistics notes                |
| createdAt             | DATETIME2     | Default: UTC          | Creation timestamp             |
| updatedAt             | DATETIME2     | Nullable              | Last update timestamp          |
| isDeleted             | BIT           | Default: 0            | Soft delete flag               |

---

### SHIPMENT_EVENTS Table
Stores chronological tracking points for shipments.

| Column     | Type          | Constraints        | Notes                         |
| ---------- | ------------- | ------------------ | ----------------------------- |
| id         | INT           | PK, Auto-increment | Primary Key                   |
| shipmentId | INT           | NOT NULL, FK       | Reference to Shipments table  |
| status     | INT           | NOT NULL           | Event status indicator        |
| location   | NVARCHAR(255) | Nullable           | Event location (e.g. Depot A) |
| note       | NVARCHAR(500) | Nullable           | Event remarks                 |
| occurredAt | DATETIME2     | Default: UTC       | Timeline occurrence timestamp |
| createdAt  | DATETIME2     | Default: UTC       | Record creation timestamp     |
| updatedAt  | DATETIME2     | Nullable           | Last update timestamp         |
| isDeleted  | BIT           | Default: 0         | Soft delete flag              |

---

## 🔑 Key Design Decisions

### 1. Soft Delete Pattern
- `isDeleted` bit flag instead of hard delete.
- Preserves referential integrity for sales analytics.

### 2. Audit Columns
- `createdAt` and `updatedAt` tracking across all entities.

### 3. Decimal for Money
- `DECIMAL(10,2)` for prices, discounts, fees, and costs to avoid IEEE-754 precision issues.

### 4. Granular RBAC (Roles & Permissions)
- Moving away from flat user roles to custom, permission-based middleware check (`roles.manage`, `products.manage`, etc.) to secure endpoints.

### 5. Multi-Location Address Book
- Relocating address listings to `UserAddresses` so customer profiles support multiple shipping destinations.

---

## 🔄 Database Workflows

### Checkout & Stock Control
1. Get User's `Cart` items.
2. In a transaction, for each item check available stock: `quantity - reservedQuantity >= cart.quantity`.
3. Increment `reservedQuantity` by order item quantity on the respective `Inventory` record.
4. Insert `Order` and `OrderItems` records.
5. Clear user's `CartItems` and reset `Cart` totals to `0`.

### Shipping Activation
1. Manager creates `Shipment` record for an order.
2. Initial `ShipmentEvent` added (Status: `Created`).
3. Update `Order.Status` to `Processing`.
4. As tracking progress is updated via `ShipmentEvents`:
   - Setting shipment status `InTransit` sets `Order.Status` to `Shipped`.
   - Setting shipment status `Delivered` sets `Order.Status` to `Delivered`, sets `deliveredAt`, and subtracts the order quantity from `Inventory.Quantity` while resetting `Inventory.ReservedQuantity` back down.

---

## 📈 Query Patterns

### Get Active Products with Variants & Images
```sql
SELECT p.*, pv.*, pi.*
FROM Products p
LEFT JOIN ProductVariants pv ON p.id = pv.productId AND pv.isDeleted = 0
LEFT JOIN ProductImages pi ON p.id = pi.productId AND pi.isDeleted = 0
WHERE p.isDeleted = 0 AND p.isActive = 1
```

### Get Order Shipment Timeline
```sql
SELECT s.*, se.*
FROM Shipments s
LEFT JOIN ShipmentEvents se ON s.id = se.shipmentId AND se.isDeleted = 0
WHERE s.orderId = @orderId AND s.isDeleted = 0
ORDER BY se.occurredAt DESC
```

---

## 🔐 Authentication & Authorization Support
- JSON Web Token (JWT) issued on `/api/Auth/login`.
- Role claims are parsed by authorization handlers.
- Explicit database join filters verify if the authenticated user owns resources (e.g. addresses, orders) or belongs to privileged groups (`Admin`, `Manager`, `Staff`).

**Document Version:** 2.0  
**Last Updated:** 2026-07-01  
