-- disable referential integrity
EXEC sp_MSForEachTable 'ALTER TABLE ? NOCHECK CONSTRAINT ALL' 
GO 

EXEC sp_MSForEachTable 'DELETE FROM ?' 
GO 

-- enable referential integrity again 
EXEC sp_MSForEachTable 'ALTER TABLE ? WITH CHECK CHECK CONSTRAINT ALL' 
GO

EXEC sp_MSForEachTable 'DBCC CHECKIDENT(''?'', RESEED, 0)'
GO

DBCC CHECKIDENT('ConsumeType', RESEED, 0)
DBCC CHECKIDENT('Activity', RESEED, 0)
DBCC CHECKIDENT('Attribute', RESEED, 0)
DBCC CHECKIDENT('ConsumedResourceAttribute', RESEED, 0)
DBCC CHECKIDENT('Process', RESEED, 0)
DBCC CHECKIDENT('Resource', RESEED, 0)
DBCC CHECKIDENT('ResourceAllocation', RESEED, 0)
DBCC CHECKIDENT('ResourceAttribute', RESEED, 0)
DBCC CHECKIDENT('ResourceKind', RESEED, 0)
DBCC CHECKIDENT('ResourceKindAttribute', RESEED, 0)