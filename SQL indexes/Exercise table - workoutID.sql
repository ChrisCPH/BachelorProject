USE [RunningDB]
GO

/****** Object:  Index [Nonclustered Index]    Script Date: 26-05-2025 21:38:34 ******/
CREATE NONCLUSTERED INDEX [Nonclustered Index] ON [dbo].[Exercise]
(
	[WorkoutID] ASC
)
INCLUDE([ExerciseID],[Name],[Sets],[Reps],[Weight]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

