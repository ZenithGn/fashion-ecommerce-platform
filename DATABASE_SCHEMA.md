# 📊 Database Schema Documentation

## Entity Relationship Diagram (ERD)

```
┌─────────────────────────────────────┐
│           USERS                     │
├─────────────────────────────────────┤
│ id (PK)                             │
│ firstName                           │
│ lastName                            │
│ email (UNIQUE)                      │
│ passwordHash                        │
│ passwordResetToken                  │
│ passwordResetTokenExpiry            │
│ phoneNumber                         │
│ address                             │
│ city                                │
│ state                               │
│ postalCode                          │
│ country                             │
│ role (0=Customer, 1=Admin, 2=Staff)│
│ isActive                            │
│ lastLoginAt                         │
│ createdAt                           │
│ updatedAt                           │
│ isDeleted                           │
└─────────────────────────────────────┘
         ▲              ▲
         │              │
         │ (1:N)        │ (1:1)
         │              │
    ┌────┴──────┐   ┌──┴────────────────┐
    │            │   │                   │
    │            │   │                   │
┌───┴────────┐  │   │  ┌────────────────┴────┐
│   ORDERS   │  │   │  │      CARTS          │
├────────────┤  │   │  ├─────────────────────┤
│ id (PK)    │◄─┘   └─►│ id (PK)             │
│ userId★───┼─────────│ userId★ (UNIQUE)   │
│orderNumber│         │ totalPrice          │
│ status    │         │ itemCount           │
│subTotal   │         │ createdAt           │
│...(price) │         │ updatedAt           │
│...        │         │ isDeleted           │
│ createdAt │         └────────────────────┬┘
│ ...       │                              │
└───┬───────┘                              │ (1:N)
    │                                      │
    │ (1:N)                                │
    │                           ┌──────────┴─────────┐
    │          ┌────────────────│                    │
    │          │                │                    │
┌───┴─────────▼──────┐  ┌───────┴──────────────┐   │
│   ORDER ITEMS      │  │   CART ITEMS        │   │
├────────────────────┤  ├─────────────────────┤   │
│ id (PK)            │  │ id (PK)             │   │
│ orderId★ (FK)      │  │ cartId★ (FK)────────┼───┘
│ productId★ (FK)────┬──┼─productId★ (FK)────┬┐
│ quantity           │  │ quantity            ││
│ unitPrice          │  │ unitPrice           ││
│ totalPrice         │  │ totalPrice          ││
│ size               │  │ createdAt           ││
│ color              │  │ updatedAt           ││
│ ...                │  │ isDeleted           ││
└────────┬───────────┘  └─────────────────────┘│
         │                                      │
         │ (N:1)                                │ (N:1)
         │                                      │
         └──────────────────────┬───────────────┘
                                │
                    ┌───────────▼──────────┐
                    │     PRODUCTS        │
                    ├─────────────────────┤
                    │ id (PK)             │
                    │ name                │
                    │ description         │
                    │ price (decimal)     │
                    │ discountPrice       │
                    │ categoryId★ (FK)────┬──┐
                    │ sku (UNIQUE)        │  │ (N:1)
                    │ brand               │  │
                    │ color               │  │
                    │ size                │  │
                    │ material            │  │
                    │ imageUrl            │  │
                    │ rating (0-5)        │  │
                    │ reviewCount         │  │
                    │ isActive            │  │
                    │ createdAt           │  │
                    │ updatedAt           │  │
                    │ isDeleted           │  │
                    └───┬─────────────────┘  │
                        │                    │
                        │ (1:N)              │
                        │                    │
                    ┌───┴────────────────┐   │
                    │   INVENTORIES      │   │
                    ├────────────────────┤   │
                    │ id (PK)            │   │
                    │ productId★ (FK)────┘   │
                    │ quantity                │
                    │ reservedQuantity       │
                    │ warehouseId            │
                    │ location               │
                    │ lastRestockDate        │
                    │ notes                  │
                    │ createdAt              │
                    │ updatedAt              │
                    │ isDeleted              │
                    └────────────────────┘

        ┌────────────────────────────────┐
        │      CATEGORIES                │
        ├────────────────────────────────┤
        │ id (PK)                        │
        │ name                           │
        │ description                    │
        │ imageUrl                       │
        │ parentCategoryId★ (FK, NULL)   │◄─┐ (1:N)
        │ isActive                       │  │ (1:1)
        │ createdAt                      │  │
        │ updatedAt                      │  │
        │ isDeleted                      │──┘
        └────────────────────────────────┘
```

## 📊 Table Specifications

### USERS Table

| Column                   | Type          | Constraints           | Notes                        |
| ------------------------ | ------------- | --------------------- | ---------------------------- |
| id                       | INT           | PK, Auto-increment    | Primary Key                  |
| firstName                | NVARCHAR(100) | NOT NULL              | User's first name            |
| lastName                 | NVARCHAR(100) | NOT NULL              | User's last name             |
| email                    | NVARCHAR(255) | NOT NULL, UNIQUE      | Email address                |
| passwordHash             | NVARCHAR(255) | NOT NULL              | Hashed password              |
| passwordResetToken       | NVARCHAR(255) | Nullable              | Reset token                  |
| passwordResetTokenExpiry | DATETIME2     | Nullable              | Reset token expiry           |
| phoneNumber              | NVARCHAR(20)  | Nullable              | Phone number                 |
| address                  | NVARCHAR(500) | Nullable              | Street address               |
| city                     | NVARCHAR(100) | Nullable              | City                         |
| state                    | NVARCHAR(100) | Nullable              | State/Province               |
| postalCode               | NVARCHAR(20)  | Nullable              | Postal code                  |
| country                  | NVARCHAR(100) | Nullable              | Country                      |
| role                     | INT           | Default: 0            | 0=Customer, 1=Admin, 2=Staff |
| isActive                 | BIT           | Default: 1            | Account status               |
| lastLoginAt              | DATETIME2     | Nullable              | Last login timestamp         |
| createdAt                | DATETIME2     | Default: GETUTCDATE() | Record creation time         |
| updatedAt                | DATETIME2     | Nullable              | Last update time             |
| isDeleted                | BIT           | Default: 0            | Soft delete flag             |

**Indexes:**

- PK: id
- UNIQUE: email

---

### CATEGORIES Table

| Column           | Type          | Constraints           | Notes                             |
| ---------------- | ------------- | --------------------- | --------------------------------- |
| id               | INT           | PK, Auto-increment    | Primary Key                       |
| name             | NVARCHAR(100) | NOT NULL              | Category name                     |
| description      | NVARCHAR(500) | Nullable              | Category description              |
| imageUrl         | NVARCHAR(MAX) | Nullable              | Category image                    |
| parentCategoryId | INT           | FK (NULL for main)    | Parent category for subcategories |
| isActive         | BIT           | Default: 1            | Active status                     |
| createdAt        | DATETIME2     | Default: GETUTCDATE() | Creation time                     |
| updatedAt        | DATETIME2     | Nullable              | Update time                       |
| isDeleted        | BIT           | Default: 0            | Soft delete flag                  |

**Relationships:**

- FK: parentCategoryId → Categories(id)

---

### PRODUCTS Table

| Column        | Type           | Constraints           | Notes               |
| ------------- | -------------- | --------------------- | ------------------- |
| id            | INT            | PK, Auto-increment    | Primary Key         |
| name          | NVARCHAR(255)  | NOT NULL              | Product name        |
| description   | NVARCHAR(1000) | Nullable              | Product description |
| price         | DECIMAL(10,2)  | NOT NULL              | Product price       |
| discountPrice | DECIMAL(10,2)  | Nullable              | Discounted price    |
| categoryId    | INT            | NOT NULL, FK          | Product category    |
| sku           | NVARCHAR(100)  | UNIQUE                | Stock keeping unit  |
| brand         | NVARCHAR(50)   | Nullable              | Brand name          |
| color         | NVARCHAR(50)   | Nullable              | Color               |
| size          | NVARCHAR(50)   | Nullable              | Size                |
| material      | NVARCHAR(50)   | Nullable              | Material type       |
| imageUrl      | NVARCHAR(MAX)  | Nullable              | Product image       |
| rating        | DECIMAL(5,2)   | Range: 0-5            | Product rating      |
| reviewCount   | INT            | Default: 0            | Number of reviews   |
| isActive      | BIT            | Default: 1            | Active status       |
| createdAt     | DATETIME2      | Default: GETUTCDATE() | Creation time       |
| updatedAt     | DATETIME2      | Nullable              | Update time         |
| isDeleted     | BIT            | Default: 0            | Soft delete flag    |

**Relationships:**

- FK: categoryId → Categories(id) [ON DELETE RESTRICT]

---

### INVENTORIES Table

| Column           | Type          | Constraints           | Notes                          |
| ---------------- | ------------- | --------------------- | ------------------------------ |
| id               | INT           | PK, Auto-increment    | Primary Key                    |
| productId        | INT           | NOT NULL, FK          | Product reference              |
| quantity         | INT           | NOT NULL              | Total quantity in stock        |
| reservedQuantity | INT           | Default: 0            | Reserved for orders            |
| warehouseId      | INT           | Nullable              | Warehouse reference            |
| location         | NVARCHAR(100) | Nullable              | Physical location in warehouse |
| lastRestockDate  | DATETIME2     | Nullable              | Last restock date              |
| notes            | NVARCHAR(500) | Nullable              | Additional notes               |
| createdAt        | DATETIME2     | Default: GETUTCDATE() | Creation time                  |
| updatedAt        | DATETIME2     | Nullable              | Update time                    |
| isDeleted        | BIT           | Default: 0            | Soft delete flag               |

**Calculated Fields:**

- availableQuantity = quantity - reservedQuantity

**Relationships:**

- FK: productId → Products(id) [ON DELETE CASCADE]

---

### CARTS Table

| Column     | Type          | Constraints           | Notes            |
| ---------- | ------------- | --------------------- | ---------------- |
| id         | INT           | PK, Auto-increment    | Primary Key      |
| userId     | INT           | NOT NULL, FK, UNIQUE  | User cart        |
| totalPrice | DECIMAL(10,2) | Default: 0            | Total cart value |
| itemCount  | INT           | Default: 0            | Number of items  |
| createdAt  | DATETIME2     | Default: GETUTCDATE() | Creation time    |
| updatedAt  | DATETIME2     | Nullable              | Update time      |
| isDeleted  | BIT           | Default: 0            | Soft delete flag |

**Relationships:**

- FK: userId → Users(id) [ON DELETE CASCADE]

---

### CART_ITEMS Table

| Column     | Type          | Constraints           | Notes             |
| ---------- | ------------- | --------------------- | ----------------- |
| id         | INT           | PK, Auto-increment    | Primary Key       |
| cartId     | INT           | NOT NULL, FK          | Cart reference    |
| productId  | INT           | NOT NULL, FK          | Product reference |
| quantity   | INT           | NOT NULL              | Quantity in cart  |
| unitPrice  | DECIMAL(10,2) | NOT NULL              | Price per unit    |
| totalPrice | DECIMAL(10,2) | NOT NULL              | Total for item    |
| createdAt  | DATETIME2     | Default: GETUTCDATE() | Creation time     |
| updatedAt  | DATETIME2     | Nullable              | Update time       |
| isDeleted  | BIT           | Default: 0            | Soft delete flag  |

**Unique Constraint:**

- (cartId, productId)

**Relationships:**

- FK: cartId → Carts(id) [ON DELETE CASCADE]
- FK: productId → Products(id) [ON DELETE CASCADE]

---

### ORDERS Table

| Column          | Type          | Constraints           | Notes              |
| --------------- | ------------- | --------------------- | ------------------ |
| id              | INT           | PK, Auto-increment    | Primary Key        |
| userId          | INT           | NOT NULL, FK          | Customer reference |
| orderNumber     | NVARCHAR(50)  | NOT NULL, UNIQUE      | Order identifier   |
| status          | INT           | Default: 0            | Order status (0-5) |
| subTotal        | DECIMAL(10,2) | NOT NULL              | Items subtotal     |
| shippingCost    | DECIMAL(10,2) | Default: 0            | Shipping fee       |
| taxAmount       | DECIMAL(10,2) | Default: 0            | Tax amount         |
| discountAmount  | DECIMAL(10,2) | Default: 0            | Discount amount    |
| totalPrice      | DECIMAL(10,2) | NOT NULL              | Final order total  |
| shippingAddress | NVARCHAR(500) | Nullable              | Delivery address   |
| city            | NVARCHAR(100) | Nullable              | City               |
| state           | NVARCHAR(100) | Nullable              | State/Province     |
| postalCode      | NVARCHAR(20)  | Nullable              | Postal code        |
| country         | NVARCHAR(100) | Nullable              | Country            |
| phoneNumber     | NVARCHAR(20)  | Nullable              | Contact number     |
| notes           | NVARCHAR(500) | Nullable              | Order notes        |
| shippedDate     | DATETIME2     | Nullable              | Shipping date      |
| deliveredDate   | DATETIME2     | Nullable              | Delivery date      |
| trackingNumber  | NVARCHAR(100) | Nullable              | Tracking number    |
| createdAt       | DATETIME2     | Default: GETUTCDATE() | Creation time      |
| updatedAt       | DATETIME2     | Nullable              | Update time        |
| isDeleted       | BIT           | Default: 0            | Soft delete flag   |

**Order Status Values:**

- 0 = Pending
- 1 = Processing
- 2 = Shipped
- 3 = Delivered
- 4 = Cancelled
- 5 = Returned

**Relationships:**

- FK: userId → Users(id) [ON DELETE RESTRICT]

---

### ORDER_ITEMS Table

| Column     | Type          | Constraints           | Notes             |
| ---------- | ------------- | --------------------- | ----------------- |
| id         | INT           | PK, Auto-increment    | Primary Key       |
| orderId    | INT           | NOT NULL, FK          | Order reference   |
| productId  | INT           | NOT NULL, FK          | Product reference |
| quantity   | INT           | NOT NULL              | Quantity ordered  |
| unitPrice  | DECIMAL(10,2) | NOT NULL              | Price per unit    |
| totalPrice | DECIMAL(10,2) | NOT NULL              | Total for item    |
| size       | NVARCHAR(100) | Nullable              | Size ordered      |
| color      | NVARCHAR(100) | Nullable              | Color ordered     |
| createdAt  | DATETIME2     | Default: GETUTCDATE() | Creation time     |
| updatedAt  | DATETIME2     | Nullable              | Update time       |
| isDeleted  | BIT           | Default: 0            | Soft delete flag  |

**Relationships:**

- FK: orderId → Orders(id) [ON DELETE CASCADE]
- FK: productId → Products(id) [ON DELETE CASCADE]

---

## 🔑 Key Design Decisions

### 1. Soft Delete Pattern

- `isDeleted` bit flag instead of hard delete
- Preserves data integrity and audit trails
- Benefits:
  - Data recovery
  - Historical analysis
  - Referential integrity

### 2. Audit Columns

- `createdAt`: Record creation timestamp
- `updatedAt`: Last modification timestamp
- `isDeleted`: Logical delete flag

### 3. Decimal for Money

- `DECIMAL(10,2)` for all prices
- Prevents floating-point precision issues

### 4. Foreign Key Constraints

- ON DELETE RESTRICT: Prevent orphaned records (Users → Orders)
- ON DELETE CASCADE: Auto-delete related items (Carts → CartItems)

### 5. Unique Constraints

- Email must be unique (prevent duplicate accounts)
- SKU must be unique (product identifiers)
- OrderNumber unique (tracking)
- CartId + ProductId unique (one item per product per cart)

---

## 🔄 Database Workflows

### Adding a Product to Cart

1. Get Cart by UserId
2. Check Product inventory
3. Create CartItem with (CartId, ProductId)
4. Update Cart totals

### Creating an Order

1. Get Cart items
2. Reserve inventory for each item
3. Create Order record
4. Create OrderItems from CartItems
5. Clear cart

### Inventory Management

1. Check available quantity = total - reserved
2. Reserve when order created
3. Release if order cancelled
4. Decrease actual quantity when shipped

---

## 📈 Query Patterns

### Get Active Products with Inventory

```sql
SELECT p.*, i.quantity, i.reservedQuantity
FROM Products p
JOIN Inventories i ON p.id = i.productId
WHERE p.isDeleted = 0 AND p.isActive = 1
```

### Get User Orders with Details

```sql
SELECT o.*, oi.*, p.name, p.price
FROM Orders o
JOIN OrderItems oi ON o.id = oi.orderId
JOIN Products p ON oi.productId = p.id
WHERE o.userId = @userId AND o.isDeleted = 0
ORDER BY o.createdAt DESC
```

### Get Cart Total

```sql
SELECT SUM(ci.totalPrice) as totalPrice, COUNT(*) as itemCount
FROM CartItems ci
JOIN Carts c ON ci.cartId = c.id
WHERE c.userId = @userId AND ci.isDeleted = 0
```

---

## 🔐 Authentication Support

- JWT register/login flow
- Password reset token flow

**Document Version:** 1.0  
**Last Updated:** 2026-05-26
