USE [RunningDB]
GO

ALTER TABLE [dbo].[Exercise] DROP CONSTRAINT [FK_Exercise_Workout]
GO

/****** Object:  Table [dbo].[Exercise]    Script Date: 26-05-2025 21:50:50 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Exercise]') AND type in (N'U'))
DROP TABLE [dbo].[Exercise]
GO

/****** Object:  Table [dbo].[Exercise]    Script Date: 26-05-2025 21:50:50 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Exercise](
	[ExerciseID] [int] IDENTITY(1,1) NOT NULL,
	[WorkoutID] [int] NOT NULL,
	[Name] [varchar](50) NOT NULL,
	[Sets] [int] NULL,
	[Reps] [int] NULL,
	[Weight] [int] NULL,
 CONSTRAINT [PK_Exercise] PRIMARY KEY CLUSTERED 
(
	[ExerciseID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Exercise]  WITH CHECK ADD  CONSTRAINT [FK_Exercise_Workout] FOREIGN KEY([WorkoutID])
REFERENCES [dbo].[Workout] ([WorkoutID])
GO

ALTER TABLE [dbo].[Exercise] CHECK CONSTRAINT [FK_Exercise_Workout]
GO

