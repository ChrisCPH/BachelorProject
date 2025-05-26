USE [RunningDB]
GO

ALTER TABLE [dbo].[UserTrainingPlan] DROP CONSTRAINT [FK_UserTrainingPlan_User]
GO

ALTER TABLE [dbo].[UserTrainingPlan] DROP CONSTRAINT [FK_UserTrainingPlan_TrainingPlan]
GO

/****** Object:  Table [dbo].[UserTrainingPlan]    Script Date: 26-05-2025 21:53:19 ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UserTrainingPlan]') AND type in (N'U'))
DROP TABLE [dbo].[UserTrainingPlan]
GO

/****** Object:  Table [dbo].[UserTrainingPlan]    Script Date: 26-05-2025 21:53:19 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[UserTrainingPlan](
	[UserTrainingPlanID] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NOT NULL,
	[TrainingPlanID] [int] NOT NULL,
	[Permission] [varchar](20) NOT NULL,
 CONSTRAINT [PK_UserTrainingPlan] PRIMARY KEY CLUSTERED 
(
	[UserTrainingPlanID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[UserTrainingPlan]  WITH CHECK ADD  CONSTRAINT [FK_UserTrainingPlan_TrainingPlan] FOREIGN KEY([TrainingPlanID])
REFERENCES [dbo].[TrainingPlan] ([TrainingPlanID])
GO

ALTER TABLE [dbo].[UserTrainingPlan] CHECK CONSTRAINT [FK_UserTrainingPlan_TrainingPlan]
GO

ALTER TABLE [dbo].[UserTrainingPlan]  WITH CHECK ADD  CONSTRAINT [FK_UserTrainingPlan_User] FOREIGN KEY([UserID])
REFERENCES [dbo].[User] ([UserID])
GO

ALTER TABLE [dbo].[UserTrainingPlan] CHECK CONSTRAINT [FK_UserTrainingPlan_User]
GO

