USE [RunningDB]
GO

ALTER TABLE [dbo].[Comment] DROP CONSTRAINT [FK_Comment_Workout]
GO

ALTER TABLE [dbo].[Comment] DROP CONSTRAINT [FK_Comment_User]
GO

ALTER TABLE [dbo].[Comment] DROP CONSTRAINT [FK_Comment_Run]
GO

/****** Object:  Table [dbo].[Comment]    Script Date: 26-05-2025 21:50:18 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Comment]') AND type in (N'U'))
DROP TABLE [dbo].[Comment]
GO

/****** Object:  Table [dbo].[Comment]    Script Date: 26-05-2025 21:50:18 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Comment](
	[CommentID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[RunID] [int] NULL,
	[WorkoutID] [int] NULL,
	[Text] [text] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_Comment] PRIMARY KEY CLUSTERED 
(
	[CommentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Comment]  WITH CHECK ADD  CONSTRAINT [FK_Comment_Run] FOREIGN KEY([RunID])
REFERENCES [dbo].[Run] ([RunID])
GO

ALTER TABLE [dbo].[Comment] CHECK CONSTRAINT [FK_Comment_Run]
GO

ALTER TABLE [dbo].[Comment]  WITH CHECK ADD  CONSTRAINT [FK_Comment_User] FOREIGN KEY([UserID])
REFERENCES [dbo].[User] ([UserID])
GO

ALTER TABLE [dbo].[Comment] CHECK CONSTRAINT [FK_Comment_User]
GO

ALTER TABLE [dbo].[Comment]  WITH CHECK ADD  CONSTRAINT [FK_Comment_Workout] FOREIGN KEY([WorkoutID])
REFERENCES [dbo].[Workout] ([WorkoutID])
GO

ALTER TABLE [dbo].[Comment] CHECK CONSTRAINT [FK_Comment_Workout]
GO

