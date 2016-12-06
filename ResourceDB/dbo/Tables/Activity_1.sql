CREATE TABLE [dbo].[Activity] (
    [ID]         INT  IDENTITY (1, 1) NOT NULL,
    [Process_ID] INT  NULL,
    [Title]      TEXT NOT NULL,
    CONSTRAINT [PK_ACTIVITY] PRIMARY KEY NONCLUSTERED ([ID] ASC),
    CONSTRAINT [FK_ACTIVITY_RELATIONS_PROCESS] FOREIGN KEY ([Process_ID]) REFERENCES [dbo].[Process] ([ID])
);


GO
CREATE NONCLUSTERED INDEX [Relationship_13_FK]
    ON [dbo].[Activity]([Process_ID] ASC);

