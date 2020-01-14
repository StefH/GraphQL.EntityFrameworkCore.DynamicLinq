IF OBJECT_ID('Orders') IS NOT NULL
	DROP TABLE Orders;
GO

IF OBJECT_ID('Customers') IS NOT NULL
	DROP TABLE Customers;
GO


CREATE TABLE Customers
(
	CustomerID	 int NOT NULL IDENTITY (1, 1) PRIMARY KEY,
	CustomerName nvarchar(200)
)

GO

CREATE TABLE Orders
(
	OrderID	   int			 NOT NULL IDENTITY (1, 1) PRIMARY KEY,
	OrderDate  datetime,
	Comments   nvarchar(max) NULL,
	CustomerID int			 NOT NULL REFERENCES Customers (CustomerID)
)

GO

INSERT INTO Customers
(
	CustomerName
)
	VALUES
		(N'Facebook'),
		(N'Microsoft');
GO

INSERT INTO Orders
(
	OrderDate,
	CustomerID,
	Comments
)
	VALUES
			(GETDATE(), (SELECT CustomerID FROM Customers WHERE CustomerName = 'Facebook'), 'Order1 from Facebook');
GO

INSERT INTO Orders
(
	OrderDate,
	CustomerID,
	Comments
)
	VALUES
			(GETDATE(), (SELECT CustomerID FROM Customers WHERE CustomerName = 'Facebook'), 'Order2 from Facebook');
GO

INSERT INTO Orders
(
	OrderDate,
	CustomerID,
	Comments
)
	VALUES
			(GETDATE(), (SELECT CustomerID FROM Customers WHERE CustomerName = 'Microsoft'), 'Order1 from Microsoft');
GO