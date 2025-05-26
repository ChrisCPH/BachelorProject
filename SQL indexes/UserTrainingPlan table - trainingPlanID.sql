USE [RunningDB]
GO

/****** Object:  Index [Nonclustered Index TrainingPlanID]    Script Date: 26-05-2025 21:43:02 ******/
CREATE NONCLUSTERED INDEX [Nonclustered Index TrainingPlanID] ON [dbo].[UserTrainingPlan]
(
	[TrainingPlanID] ASC
)
INCLUDE([UserTrainingPlanID],[UserID],[Permission]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

