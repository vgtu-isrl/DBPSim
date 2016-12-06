CREATE TABLE [dbo].[ResourceKind] (
    [ID]                  INT  IDENTITY (1, 1) NOT NULL,
    [Res_ResourceKind_ID] INT  NULL,
    [Title]               TEXT NOT NULL,
    CONSTRAINT [PK_RESOURCEKIND] PRIMARY KEY NONCLUSTERED ([ID] ASC),
    CONSTRAINT [FK_RESOURCE_CONSIST_RESOURCE] FOREIGN KEY ([Res_ResourceKind_ID]) REFERENCES [dbo].[ResourceKind] ([ID])
);


GO
CREATE NONCLUSTERED INDEX [Consist_FK]
    ON [dbo].[ResourceKind]([Res_ResourceKind_ID] ASC);

