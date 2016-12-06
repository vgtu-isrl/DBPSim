CREATE TABLE [dbo].[ResourceAttribute] (
    [ID]                           INT  IDENTITY (1, 1) NOT NULL,
    [Attribute_ID]                 INT  NULL,
    [Resource_ID]                  INT  NULL,
    [ConsumedResourceAttribute_ID] INT  NULL,
    [Value]                        INT NOT NULL,
    [Max Value]                    INT NOT NULL,
    CONSTRAINT [PK_RESOURCEATTRIBUTE] PRIMARY KEY NONCLUSTERED ([ID] ASC),
    CONSTRAINT [FK_RESOURCE_CANHAVE_RESOURCE] FOREIGN KEY ([Resource_ID]) REFERENCES [dbo].[Resource] ([ID]),
    CONSTRAINT [FK_RESOURCE_RELATIONS_ATTRIBUT] FOREIGN KEY ([Attribute_ID]) REFERENCES [dbo].[Attribute] ([ID]),
    CONSTRAINT [FK_RESOURCE_RELATIONS_CONSUMED] FOREIGN KEY ([ConsumedResourceAttribute_ID]) REFERENCES [dbo].[ConsumedResourceAttribute] ([ID])
);


GO
CREATE NONCLUSTERED INDEX [Relationship_12_FK]
    ON [dbo].[ResourceAttribute]([ConsumedResourceAttribute_ID] ASC);


GO
CREATE NONCLUSTERED INDEX [CanHave_FK]
    ON [dbo].[ResourceAttribute]([Resource_ID] ASC);


GO
CREATE NONCLUSTERED INDEX [Relationship_2_FK]
    ON [dbo].[ResourceAttribute]([Attribute_ID] ASC);

