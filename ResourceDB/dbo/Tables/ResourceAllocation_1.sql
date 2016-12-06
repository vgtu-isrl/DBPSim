CREATE TABLE [dbo].[ResourceAllocation] (
    [ID]              INT      IDENTITY (1, 1) NOT NULL,
    [Resource_ID]     INT      NULL,
    [Res_Resource_ID] INT      NULL,
    [Activity_ID]     INT      NULL,
    [Occupied_From]   DATETIME NOT NULL,
    [Occupied_Untill] DATETIME NULL,
    CONSTRAINT [PK_RESOURCEALLOCATION] PRIMARY KEY NONCLUSTERED ([ID] ASC),
    CONSTRAINT [FK_RESOURCE_CONSUMES_RESOURCE] FOREIGN KEY ([Res_Resource_ID]) REFERENCES [dbo].[Resource] ([ID]),
    CONSTRAINT [FK_RESOURCE_PROVIDES_RESOURCE] FOREIGN KEY ([Resource_ID]) REFERENCES [dbo].[Resource] ([ID]),
    CONSTRAINT [FK_RESOURCE_RELATIONS_ACTIVITY] FOREIGN KEY ([Activity_ID]) REFERENCES [dbo].[Activity] ([ID])
);


GO
CREATE NONCLUSTERED INDEX [Relationship_9_FK]
    ON [dbo].[ResourceAllocation]([Activity_ID] ASC);


GO
CREATE NONCLUSTERED INDEX [Consumes_FK]
    ON [dbo].[ResourceAllocation]([Res_Resource_ID] ASC);


GO
CREATE NONCLUSTERED INDEX [Provides_FK]
    ON [dbo].[ResourceAllocation]([Resource_ID] ASC);

