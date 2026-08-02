CREATE DATABASE LaCastellana;
USE LaCastellana;

CREATE TABLE roles (
    rol_id INT AUTO_INCREMENT NOT NULL,
    name VARCHAR(128) NOT NULL,
    description VARCHAR(255) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_by INT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by INT NULL,
    updated_at DATETIME NULL,
    PRIMARY KEY rol_id
)

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
    PRIMARY KEY user_id,
    CONSTRAINT fk_users_roles FOREIGN KEY(rol_id) REFERENCES roles(rol_id)
)

CREATE TABLE item_categories (
    category_id INT AUTO_INCREMENT NOT NULL,
    name VARCHAR(128) NOT NULL,
    description VARCHAR(255) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_by INT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by INT NULL,
    updated_at DATETIME NULL,
    PRIMARY KEY category_id,
    CONSTRAINT fk_itemCat_createdBy FOREIGN KEY(created_by) REFERENCES users(user_id),
    CONSTRAINT fk_itemCat_updatedBy FOREIGN KEY(updated_by) REFERENCES users(user_id)
)

CREATE TABLE item_types (
    type_id INT AUTO_INCREMENT NOT NULL,
    name VARCHAR(128) NOT NULL,
    description VARCHAR(255) NULL,
    is_deleted TINYINT(1) NOT NULL DEFAULT 0,
    created_by INT NOT NULL,
    created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_by INT NULL,
    updated_at DATETIME NULL,
    PRIMARY_KEY type_id,
    CONSTRAINT fk_itemType_createdBy FOREIGN KEY(created_by) REFERENCES users(user_id),
    CONSTRAINT fk_itemType_updatedBy FOREIGN KEY(updated_by) REFERENCES users(user_id)
)

