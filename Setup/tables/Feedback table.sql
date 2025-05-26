USE [RunningDB]
GO

ALTER TABLE [dbo].[Feedback] DROP CONSTRAINT [FK_Feedback_Run]
GO

/****** Object:  Table [dbo].[Feedback]    Script Date: 26-05-2025 21:51:02 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Feedback]') AND type in (N'U'))
DROP TABLE [dbo].[Feedback]
GO

/****** Object:  Table [dbo].[Feedback]    Script Date: 26-05-2025 21:51:02 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Feedback](
	[FeedbackID] [int] IDENTITY(1,1) NOT NULL,
	[RunID] [int] NOT NULL,
	[EffortRating] [int] NOT NULL,
	[FeelRating] [int] NOT NULL,
	[Pace] [int] NULL,
	[Duration] [int] NULL,
	[Distance] [float] NULL,
	[CreatedAt] [datetime] NOT NULL,
	[Comment] [text] NULL,
 CONSTRAINT [PK_Feedback] PRIMARY KEY CLUSTERED 
(
	[FeedbackID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Feedback]  WITH CHECK ADD  CONSTRAINT [FK_Feedback_Run] FOREIGN KEY([RunID])
REFERENCES [dbo].[Run] ([RunID])
GO

ALTER TABLE [dbo].[Feedback] CHECK CONSTRAINT [FK_Feedback_Run]
GO

