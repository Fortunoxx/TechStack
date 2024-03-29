CREATE TABLE [dbo].[Users] (
    [Id]             INT                IDENTITY (1, 1) NOT NULL,
    [AboutMe]        NVARCHAR (MAX)     NULL,
    [Age]            INT                NULL,
    [CreationDate]   DATETIME           CONSTRAINT [DF_CreationDate] DEFAULT (getutcdate()) NOT NULL,
    [DisplayName]    NVARCHAR (40)      NOT NULL,
    [DownVotes]      INT                NOT NULL,
    [EmailHash]      NVARCHAR (40)      NULL,
    [LastAccessDate] DATETIME           CONSTRAINT [DF_LastAccessDate] DEFAULT (getutcdate()) NOT NULL,
    [Location]       NVARCHAR (100)     NULL,
    [Reputation]     INT                NOT NULL,
    [UpVotes]        INT                NOT NULL,
    [Views]          INT                NOT NULL,
    [WebsiteUrl]     NVARCHAR (200)     NULL,
    [AccountId]      INT                NULL,
    [Created]        DATETIMEOFFSET (7) NULL,
    [CreatedBy]      NVARCHAR (8)       NULL,
    [LastModified]   DATETIMEOFFSET (7) NULL,
    [LastModifiedBy] NVARCHAR (8)       NULL,
    CONSTRAINT [PK_Users_Id] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO

