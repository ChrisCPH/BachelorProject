USE [RunningDB]
GO

SET ANSI_PADDING ON
GO

/****** Object:  Index [Nonclustered Index Email]    Script Date: 26-05-2025 21:36:24 ******/
CREATE UNIQUE NONCLUSTERED INDEX [Nonclustered Index Email] ON [dbo].[User]
(
	[Email] ASC
)
INCLUDE([UserID],[UserName],[Password],[CreatedAt],[PreferredDistance],[PreferredWeight]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

