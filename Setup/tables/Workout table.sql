USE [RunningDB]
GO

ALTER TABLE [dbo].[Workout] DROP CONSTRAINT [FK_Workout_TrainingPlan]
GO

/****** Object:  Table [dbo].[Workout]    Script Date: 26-05-2025 21:53:34 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Workout]') AND type in (N'U'))
DROP TABLE [dbo].[Workout]
GO

/****** Object:  Table [dbo].[Workout]    Script Date: 26-05-2025 21:53:34 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Workout](
	[WorkoutID] [int] IDENTITY(1,1) NOT NULL,
	[TrainingPlanID] [int] NOT NULL,
	[Type] [varchar](50) NOT NULL,
	[WeekNumber] [int] NOT NULL,
	[DayOfWeek] [int] NOT NULL,
	[TimeOfDay] [time](0) NULL,
	[Duration] [int] NULL,
	[Notes] [text] NULL,
	[Completed] [bit] NOT NULL,
	[CreatedAt] [datetime] NOT NULL,
 CONSTRAINT [PK_Workout] PRIMARY KEY CLUSTERED 
(
	[WorkoutID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [dbo].[Workout]  WITH CHECK ADD  CONSTRAINT [FK_Workout_TrainingPlan] FOREIGN KEY([TrainingPlanID])
REFERENCES [dbo].[TrainingPlan] ([TrainingPlanID])
GO

ALTER TABLE [dbo].[Workout] CHECK CONSTRAINT [FK_Workout_TrainingPlan]
GO

