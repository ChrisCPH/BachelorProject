USE [RunningDB]
GO

/****** Object:  Index [Nonclustered Index WorkoutID]    Script Date: 26-05-2025 21:37:12 ******/
CREATE NONCLUSTERED INDEX [Nonclustered Index WorkoutID] ON [dbo].[Comment]
(
	[WorkoutID] ASC
)
INCLUDE([CommentID],[UserID],[RunID],[CreatedAt]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

