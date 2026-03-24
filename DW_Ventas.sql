CREATE DATABASE DW_Ventas;
GO

USE DW_Ventas;
GO

-- Dimensión Cliente
CREATE TABLE DimCustomer (
    CustomerID INT PRIMARY KEY,
    FirstName VARCHAR(100),
    LastName VARCHAR(100),
    Email VARCHAR(150),
    Phone VARCHAR(50),
    City VARCHAR(100),
    Country VARCHAR(100)
);

-- Dimensión Producto
CREATE TABLE DimProduct (
    ProductID INT PRIMARY KEY,
    ProductName VARCHAR(150),
    Category VARCHAR(100),
    Price DECIMAL(10,2)
);

-- Dimensión Fecha
CREATE TABLE DimDate (
    DateID INT PRIMARY KEY,
    FullDate DATE,
    Year INT,
    Month INT,
    Day INT
);

CREATE TABLE FactSales (
    SaleID INT IDENTITY(1,1) PRIMARY KEY,
    
    OrderID INT,
    
    CustomerID INT,
    ProductID INT,
    DateID INT,
    
    Quantity INT,
    TotalPrice DECIMAL(12,2),

    -- Relaciones con las dimensiones
    CONSTRAINT FK_FactSales_Customer
        FOREIGN KEY (CustomerID)
        REFERENCES DimCustomer(CustomerID),

    CONSTRAINT FK_FactSales_Product
        FOREIGN KEY (ProductID)
        REFERENCES DimProduct(ProductID),

    CONSTRAINT FK_FactSales_Date
        FOREIGN KEY (DateID)
        REFERENCES DimDate(DateID)
);