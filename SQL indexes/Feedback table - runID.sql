USE [RunningDB]
GO

/****** Object:  Index [Nonclustered Index RunID]    Script Date: 26-05-2025 21:39:55 ******/
CREATE NONCLUSTERED INDEX [Nonclustered Index RunID] ON [dbo].[Feedback]
(
	[RunID] ASC
)
INCLUDE([FeedbackID],[EffortRating],[FeelRating],[Pace],[Duration],[Distance],[CreatedAt]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

