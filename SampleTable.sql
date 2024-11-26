USE [TSTD]
GO

/****** Object:  Table [dbo].[TSTDTable]    Script Date: 2024/11/26 下午 04:09:59 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[TSTDTable](
	[TSTDTableId] [int] IDENTITY(1,1) NOT NULL,
	[CreatedAt] [datetime] NULL,
	[Username] [nvarchar](50) NOT NULL,
	[Message] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[TSTDTableId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[TSTDTable] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO

ALTER TABLE [dbo].[TSTDTable] ADD  DEFAULT ('Hello World') FOR [Message]
GO




INSERT INTO TSTDTable (Username)
VALUES 
    ('User1'),
    ('User2'),
    ('Admin');
GO