-- Optional: This script will be executed when MySQL container starts
-- You can add initial data here if needed

-- Create application user if not exists
-- Note: This is handled by docker-compose environment variables
-- But you can add custom initialization here

-- Example: Create indexes for better performance
-- ALTER TABLE Products ADD INDEX idx_category (CategoryId);
-- ALTER TABLE Orders ADD INDEX idx_customer (CustomerId);
-- ALTER TABLE StockMovements ADD INDEX idx_product (ProductId);
