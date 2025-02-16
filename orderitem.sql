BEGIN TRANSACTION;
CREATE INDEX [IX_OrderItem_ProductId] ON [OrderItem] ([ProductId]);

ALTER TABLE [OrderItem] ADD CONSTRAINT [FK_OrderItem_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20250216154947_add migration', N'9.0.2');

COMMIT;
GO

