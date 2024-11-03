CREATE TABLE [dbo].[UserMetaData] (
    [Id]             INT                IDENTITY (1, 1) NOT NULL,
    [UserId]         INT                NOT NULL,
    [MetaKey]        NVARCHAR (64)      NOT NULL,
    [MetaValue]      NVARCHAR (MAX)     NULL,
    [Created]        DATETIMEOFFSET (7) NULL,
    [CreatedBy]      NVARCHAR (8)       NULL,
    [LastModified]   DATETIMEOFFSET (7) NULL,
    [LastModifiedBy] NVARCHAR (8)       NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);
GO

ALTER TABLE [dbo].[UserMetaData]
    ADD CONSTRAINT [FK_UserMetaData_User] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE;
GO

