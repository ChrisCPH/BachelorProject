USE [RunningDB]
GO

ALTER TABLE [dbo].[Run] DROP CONSTRAINT [FK_Runs_TrainingPlans]
GO

/****** Object:  Table [dbo].[Run]    Script Date: 26-05-2025 21:51:11 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Run]') AND type in (N'U'))
DROP TABLE [dbo].[Run]
GO

/****** Object:  Table [dbo].[Run]    Script Date: 26-05-2025 21:51:11 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Run](
	[RunID] [int] IDENTITY(1,1) NOT NULL,
	[TrainingPlanID] [int] NOT NULL,
	[Type] [varchar](50) NULL,
	[WeekNumber] [int] NOT NULL,
	[DayOfWeek] [int] NOT NULL,
	[TimeOfDay] [time](0) NULL,
	[Pace] [int] NULL,
	[Duration] [int] NULL,
	[Distance] [float] NULL,
	[Notes] [text] NULL,
	[Completed] [bit] NOT NULL,
	[RouteID] [nvarchar](max) NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_Runs] PRIMARY KEY CLUSTERED 
(
	[RunID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Run]  WITH CHECK ADD  CONSTRAINT [FK_Runs_TrainingPlans] FOREIGN KEY([TrainingPlanID])
REFERENCES [dbo].[TrainingPlan] ([TrainingPlanID])
GO

ALTER TABLE [dbo].[Run] CHECK CONSTRAINT [FK_Runs_TrainingPlans]
GO

