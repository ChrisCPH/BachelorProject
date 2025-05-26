USE [RunningDB]
GO

/****** Object:  Index [Nonclustered Index UserID]    Script Date: 26-05-2025 21:42:45 ******/
CREATE NONCLUSTERED INDEX [Nonclustered Index UserID] ON [dbo].[UserTrainingPlan]
(
	[UserID] ASC
)
INCLUDE([UserTrainingPlanID],[TrainingPlanID],[Permission]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

