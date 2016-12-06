CREATE TABLE [dbo].[ResourceKindAttribute] (
    [ID]              INT IDENTITY (1, 1) NOT NULL,
    [Attribute_ID]    INT NULL,
    [ResourceKind_ID] INT NULL,
    [Value] TEXT NOT NULL, 
    [dimension] TEXT NULL, 
    CONSTRAINT [PK_RESOURCEKINDATTRIBUTE] PRIMARY KEY NONCLUSTERED ([ID] ASC),
    CONSTRAINT [FK_RESOURCE_BELONGSTO_ATTRIBUT] FOREIGN KEY ([Attribute_ID]) REFERENCES [dbo].[Attribute] ([ID]),
    CONSTRAINT [FK_RESOURCE_HAS_RESOURCE] FOREIGN KEY ([ResourceKind_ID]) REFERENCES [dbo].[ResourceKind] ([ID])
);


GO
CREATE NONCLUSTERED INDEX [BelongsTo_FK]
    ON [dbo].[ResourceKindAttribute]([Attribute_ID] ASC);


GO
CREATE NONCLUSTERED INDEX [Has_FK]
    ON [dbo].[ResourceKindAttribute]([ResourceKind_ID] ASC);

