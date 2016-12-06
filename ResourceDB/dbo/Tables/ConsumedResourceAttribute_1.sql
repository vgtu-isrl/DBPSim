CREATE TABLE [dbo].[ConsumedResourceAttribute] (
    [ID]                    INT IDENTITY (1, 1) NOT NULL,
    [ConsumeType_ID]        INT NULL,
    [ResourceAllocation_ID] INT NULL,
    [Amount]                INT NOT NULL,
    CONSTRAINT [PK_CONSUMEDRESOURCEATTRIBUTE] PRIMARY KEY NONCLUSTERED ([ID] ASC),
    CONSTRAINT [FK_CONSUMED_RELATIONS_CONSUMET] FOREIGN KEY ([ConsumeType_ID]) REFERENCES [dbo].[ConsumeType] ([ID]),
    CONSTRAINT [FK_CONSUMED_RELATIONS_RESOURCE] FOREIGN KEY ([ResourceAllocation_ID]) REFERENCES [dbo].[ResourceAllocation] ([ID])
);


GO
CREATE NONCLUSTERED INDEX [Relationship_14_FK]
    ON [dbo].[ConsumedResourceAttribute]([ConsumeType_ID] ASC);


GO
CREATE NONCLUSTERED INDEX [Relationship_10_FK]
    ON [dbo].[ConsumedResourceAttribute]([ResourceAllocation_ID] ASC);

