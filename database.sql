/****** Object:  Table [dbo].[UserCloudManager_FmDirectory]    Script Date: 17.05.2020 05:46:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserCloudManager_FmDirectory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[ParentDirectoryId] [int] NULL,
	[Title] [nvarchar](128) NOT NULL,
	[Description] [nvarchar](512) NULL,
	[Path] [nvarchar](4000) NOT NULL,
	[FsKey] [uniqueidentifier] NOT NULL,
	[Size] [bigint] NOT NULL,
	[FileCount] [int] NOT NULL,
	[Status] [bit] NULL,
	[AddingDate] [datetime] NOT NULL,
	[UpdatingDate] [datetime] NOT NULL,
	[AddingUserId] [int] NULL,
	[UpdatingUserId] [int] NULL,
 CONSTRAINT [PK_UserCloudManager_FmDirectory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserCloudManager_FmFile]    Script Date: 17.05.2020 05:46:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserCloudManager_FmFile](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[DirectoryId] [int] NOT NULL,
	[Title] [nvarchar](128) NOT NULL,
	[Description] [nvarchar](512) NULL,
	[FsKey] [uniqueidentifier] NOT NULL,
	[Path] [nvarchar](4000) NOT NULL,
	[Size] [bigint] NOT NULL,
	[Status] [bit] NULL,
	[AddingDate] [datetime] NOT NULL,
	[UpdatingDate] [datetime] NOT NULL,
	[AddingUserId] [int] NULL,
	[UpdatingUserId] [int] NULL,
 CONSTRAINT [PK_UserCloudManager_FmFile] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UserCloudManager_FmShare]    Script Date: 17.05.2020 05:46:41 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserCloudManager_FmShare](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FileId] [int] NULL,
	[DirectoryId] [int] NULL,
	[SharedGroupId] [int] NULL,
	[SharedUserId] [int] NULL,
	[Status] [bit] NULL,
	[AddingDate] [datetime] NOT NULL,
	[UpdatingDate] [datetime] NOT NULL,
	[AddingUserId] [int] NULL,
	[UpdatingUserId] [int] NULL,
 CONSTRAINT [PK_UserCloudManager_FmShare] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_UserCloudManager_FmDirectory]    Script Date: 17.05.2020 05:46:41 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserCloudManager_FmDirectory] ON [dbo].[UserCloudManager_FmDirectory]
(
	[UserId] ASC,
	[ParentDirectoryId] ASC,
	[Title] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = ON, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_UserCloudManager_FmFile]    Script Date: 17.05.2020 05:46:41 ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserCloudManager_FmFile] ON [dbo].[UserCloudManager_FmFile]
(
	[UserId] ASC,
	[DirectoryId] ASC,
	[Title] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = ON, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[UserCloudManager_FmDirectory] ADD  CONSTRAINT [DF_UserCloudManager_FmDirectory_FsKey]  DEFAULT (newid()) FOR [FsKey]
GO
ALTER TABLE [dbo].[UserCloudManager_FmDirectory] ADD  CONSTRAINT [DF_UserCloudManager_FmDirectory_Size]  DEFAULT ((0)) FOR [Size]
GO
ALTER TABLE [dbo].[UserCloudManager_FmDirectory] ADD  CONSTRAINT [DF_UserCloudManager_FmDirectory_FileCount]  DEFAULT ((0)) FOR [FileCount]
GO
ALTER TABLE [dbo].[UserCloudManager_FmDirectory] ADD  CONSTRAINT [DF_UserCloudManager_FmDirectory_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[UserCloudManager_FmDirectory] ADD  CONSTRAINT [DF_UserCloudManager_FmDirectory_AddingDate]  DEFAULT (getdate()) FOR [AddingDate]
GO
ALTER TABLE [dbo].[UserCloudManager_FmDirectory] ADD  CONSTRAINT [DF_UserCloudManager_FmDirectory_UpdatingDate]  DEFAULT (getdate()) FOR [UpdatingDate]
GO
ALTER TABLE [dbo].[UserCloudManager_FmFile] ADD  CONSTRAINT [DF_UserCloudManager_FmFile_FsKey]  DEFAULT (newid()) FOR [FsKey]
GO
ALTER TABLE [dbo].[UserCloudManager_FmFile] ADD  CONSTRAINT [DF_UserCloudManager_FmFile_Size]  DEFAULT ((0)) FOR [Size]
GO
ALTER TABLE [dbo].[UserCloudManager_FmFile] ADD  CONSTRAINT [DF_UserCloudManager_FmFile_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[UserCloudManager_FmFile] ADD  CONSTRAINT [DF_UserCloudManager_FmFile_AddingDate]  DEFAULT (getdate()) FOR [AddingDate]
GO
ALTER TABLE [dbo].[UserCloudManager_FmFile] ADD  CONSTRAINT [DF_UserCloudManager_FmFile_UpdatingDate]  DEFAULT (getdate()) FOR [UpdatingDate]
GO
ALTER TABLE [dbo].[UserCloudManager_FmShare] ADD  CONSTRAINT [DF_UserCloudManager_FmShare_Status]  DEFAULT ((1)) FOR [Status]
GO
ALTER TABLE [dbo].[UserCloudManager_FmShare] ADD  CONSTRAINT [DF_UserCloudManager_FmShare_AddingDate]  DEFAULT (getdate()) FOR [AddingDate]
GO
ALTER TABLE [dbo].[UserCloudManager_FmShare] ADD  CONSTRAINT [DF_UserCloudManager_FmShare_UpdatingDate]  DEFAULT (getdate()) FOR [UpdatingDate]
GO
ALTER TABLE [dbo].[UserCloudManager_FmFile]  WITH CHECK ADD  CONSTRAINT [FK_UserCloudManager_FmFile_UserCloudManager_FmDirectory] FOREIGN KEY([DirectoryId])
REFERENCES [dbo].[UserCloudManager_FmDirectory] ([Id])
GO
ALTER TABLE [dbo].[UserCloudManager_FmFile] CHECK CONSTRAINT [FK_UserCloudManager_FmFile_UserCloudManager_FmDirectory]
GO
ALTER TABLE [dbo].[UserCloudManager_FmShare]  WITH CHECK ADD  CONSTRAINT [FK_UserCloudManager_FmShare_UserCloudManager_FmDirectory] FOREIGN KEY([DirectoryId])
REFERENCES [dbo].[UserCloudManager_FmDirectory] ([Id])
GO
ALTER TABLE [dbo].[UserCloudManager_FmShare] CHECK CONSTRAINT [FK_UserCloudManager_FmShare_UserCloudManager_FmDirectory]
GO
ALTER TABLE [dbo].[UserCloudManager_FmShare]  WITH CHECK ADD  CONSTRAINT [FK_UserCloudManager_FmShare_UserCloudManager_FmFile] FOREIGN KEY([FileId])
REFERENCES [dbo].[UserCloudManager_FmFile] ([Id])
GO
ALTER TABLE [dbo].[UserCloudManager_FmShare] CHECK CONSTRAINT [FK_UserCloudManager_FmShare_UserCloudManager_FmFile]
GO
