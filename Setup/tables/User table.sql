USE [RunningDB]
GO

/****** Object:  Table [dbo].[User]    Script Date: 26-05-2025 21:51:47 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[User]') AND type in (N'U'))
DROP TABLE [dbo].[User]
GO

/****** Object:  Table [dbo].[User]    Script Date: 26-05-2025 21:51:47 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[User](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [varchar](50) NOT NULL,
	[Email] [varchar](255) NOT NULL,
	[Password] [varchar](max) NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
	[PreferredDistance] [varchar](15) NOT NULL,
	[PreferredWeight] [varchar](15) NOT NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

