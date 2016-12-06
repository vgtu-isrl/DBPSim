CREATE TABLE [dbo].[Resource] (
    [ID]                INT      IDENTITY (1, 1) NOT NULL,
    [ResourceKind_ID]   INT      NULL,
    [Title]             TEXT     NOT NULL,
    [Price per hour]    MONEY    NOT NULL,
    [Accessible_From]   DATETIME NOT NULL,
    [Accessible_Untill] DATETIME NULL,
    CONSTRAINT [PK_RESOURCE] PRIMARY KEY NONCLUSTERED ([ID] ASC),
    CONSTRAINT [FK_RESOURCE_ALLWAYSHA_RESOURCE] FOREIGN KEY ([ResourceKind_ID]) REFERENCES [dbo].[ResourceKind] ([ID])
);


GO
CREATE NONCLUSTERED INDEX [AllwaysHave_FK]
    ON [dbo].[Resource]([ResourceKind_ID] ASC);

