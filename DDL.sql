CREATE DATABASE LaCastellana;
USE LaCastellana;

-- =============== PARA PRUEBAS ===============
-- 1. Apagas la revisión de llaves foráneas
SET FOREIGN_KEY_CHECKS = 0;

-- 2. Eliminas o truncas lo que necesites sin que MariaDB te bloquee
TRUNCATE TABLE items; 
DELETE FROM users WHERE user_id = 5;

-- 3. Vuelves a encender la protección
SET FOREIGN_KEY_CHECKS = 1;
-- ===========================================

CREATE TABLE roles (
    rol_id INT AUTO_INCREMENT NOT NULL,
    name VARCHAR(128) NOT NULL,
    description VARCHAR(255) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_by INT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by INT NULL,
    updated_at DATETIME NULL,
    PRIMARY KEY(rol_id)
);

CREATE TABLE users (
    user_id INT AUTO_INCREMENT NOT NULL,
    username VARCHAR(128) NOT NULL,
    password_hash VARCHAR(255) NOT NULL,
    name VARCHAR(128) NOT NULL,
    middlename VARCHAR(128) NULL,
    pat_surname VARCHAR(128) NOT NULL,
    mat_surname VARCHAR(128) NOT NULL,
    rol_id INT NOT NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_by INT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by INT NULL,
    updated_at DATETIME NULL,
    PRIMARY KEY (user_id),
    CONSTRAINT fk_users_roles FOREIGN KEY(rol_id) REFERENCES roles(rol_id)
); -- PENDIENTE: Agregar restricción UNIQUE a username.

CREATE TABLE item_categories (
    category_id INT AUTO_INCREMENT NOT NULL,
    name VARCHAR(128) NOT NULL,
    description VARCHAR(255) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_by INT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by INT NULL,
    updated_at DATETIME NULL,
    PRIMARY KEY (category_id),
    CONSTRAINT fk_itemCat_createdBy FOREIGN KEY(created_by) REFERENCES users(user_id),
    CONSTRAINT fk_itemCat_updatedBy FOREIGN KEY(updated_by) REFERENCES users(user_id)
);

CREATE TABLE item_types (
    type_id INT AUTO_INCREMENT NOT NULL,
    name VARCHAR(128) NOT NULL,
    description VARCHAR(255) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_by INT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by INT NULL,
    updated_at DATETIME NULL,
    PRIMARY KEY (type_id),
    CONSTRAINT fk_itemType_createdBy FOREIGN KEY(created_by) REFERENCES users(user_id),
    CONSTRAINT fk_itemType_updatedBy FOREIGN KEY(updated_by) REFERENCES users(user_id)
);

CREATE TABLE items (
    item_id INT AUTO_INCREMENT NOT NULL,
    name VARCHAR(128) NOT NULL,
    description VARCHAR(255) NULL,
    unit_price DECIMAL(7,2) NOT NULL,
    category_id INT NOT NULL,
    type_id INT NOT NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_by INT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by INT NULL,
    updated_at DATETIME NULL,
    PRIMARY KEY (item_id),
    CONSTRAINT fk_item_category FOREIGN KEY(category_id) REFERENCES item_categories(category_id),
    CONSTRAINT fk_item_type FOREIGN KEY(type_id) REFERENCES item_types(type_id),
    CONSTRAINT fk_item_createdBy FOREIGN KEY(created_by) REFERENCES users(user_id),
    CONSTRAINT fk_item_updatedBy FOREIGN KEY(updated_by) REFERENCES users(user_id)
);

CREATE TABLE tables (
    table_id INT AUTO_INCREMENT NOT NULL,
    number TINYINT NOT NULL,
    description VARCHAR(255) NULL,
    is_occupied TINYINT(1) NOT NULL DEFAULT 0,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_by INT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by INT NULL,
    updated_at DATETIME NULL,
    PRIMARY KEY (table_id),
    CONSTRAINT fk_table_createdBy FOREIGN KEY(created_by) REFERENCES users(user_id),
    CONSTRAINT fk_table_updatedBy FOREIGN KEY(updated_by) REFERENCES users(user_id)
);

CREATE TABLE orders (
    order_id INT AUTO_INCREMENT NOT NULL,
    subtotal DECIMAL(7,2) NOT NULL,
    tips DECIMAL(7,2) NOT NULL DEFAULT 0,
    total DECIMAL(7,2) NULL,
    payment VARCHAR(64) NULL,
    table_id INT NOT NULL,
    is_active TINYINT(1) NOT NULL DEFAULT 1,
    is_completed TINYINT(1) NOT NULL DEFAULT 0,
    completed_at DATETIME NULL,
    is_canceled TINYINT(1) NOT NULL DEFAULT 0,
    canceled_at DATETIME NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    deleted_at DATETIME NULL,
    created_by INT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by INT NULL,
    updated_at DATETIME NULL,
    PRIMARY KEY (order_id),
    CONSTRAINT fk_order_table FOREIGN KEY(table_id) REFERENCES tables(table_id),
    CONSTRAINT fk_order_createdBy FOREIGN KEY(created_by) REFERENCES users(user_id),
    CONSTRAINT fk_order_updatedBy FOREIGN KEY(updated_by) REFERENCES users(user_id)
);

CREATE TABLE order_items (
    order_item_id INT AUTO_INCREMENT NOT NULL,
    name VARCHAR(128) NOT NULL,
    description VARCHAR(255) NULL,
    quantity INT NOT NULL DEFAULT 1,
    unit_price DECIMAL(7,2) NOT NULL,
    order_id INT NOT NULL,
    item_id INT NOT NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_by INT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by INT NULL,
    updated_at DATETIME NULL,
    PRIMARY KEY (order_item_id),
    CONSTRAINT fk_orderItem_order FOREIGN KEY(order_id) REFERENCES orders(order_id),
    CONSTRAINT fk_orderItem_item FOREIGN KEY(item_id) REFERENCES items(item_id),
    CONSTRAINT fk_orderItem_createdBy FOREIGN KEY(created_by) REFERENCES users(user_id),
    CONSTRAINT fk_orderItem_updatedBy FOREIGN KEY(updated_by) REFERENCES users(user_id)
);

-- ============ Índices Únicos.
CREATE UNIQUE INDEX idx_users_username ON users(username);

-- ============ Índices de Búsqueda.
CREATE INDEX idx_items_name ON items(name);
CREATE INDEX idx_users_name ON users(name, pat_surname);

-- ============ Índices de Baja Cardinalidad (Filtros Comunes).
CREATE INDEX idx_orders_status ON orders(is_active, is_completed);
CREATE INDEX idx_tables_occupied ON tables(is_occupied);

-- ============ Consultas de Prueba.
SELECT * FROM users;