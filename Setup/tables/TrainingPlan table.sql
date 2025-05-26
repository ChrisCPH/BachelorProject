USE [RunningDB]
GO

/****** Object:  Table [dbo].[TrainingPlan]    Script Date: 26-05-2025 21:51:24 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[TrainingPlan]') AND type in (N'U'))
DROP TABLE [dbo].[TrainingPlan]
GO

/****** Object:  Table [dbo].[TrainingPlan]    Script Date: 26-05-2025 21:51:24 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TrainingPlan](
	[TrainingPlanID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](255) NOT NULL,
	[StartDate] [date] NULL,
	[Duration] [int] NOT NULL,
	[Event] [varchar](50) NULL,
	[GoalTime] [varchar](50) NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_TrainingPlans] PRIMARY KEY CLUSTERED 
(
	[TrainingPlanID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

