USE [master]
GO
/****** Object:  Database [HMS]    Script Date: 6/28/2025 6:54:04 PM ******/
CREATE DATABASE [HMS]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'HMS', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\HMS.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'HMS_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\HMS_log.ldf' , SIZE = 73728KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [HMS] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [HMS].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [HMS] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [HMS] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [HMS] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [HMS] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [HMS] SET ARITHABORT OFF 
GO
ALTER DATABASE [HMS] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [HMS] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [HMS] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [HMS] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [HMS] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [HMS] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [HMS] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [HMS] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [HMS] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [HMS] SET  DISABLE_BROKER 
GO
ALTER DATABASE [HMS] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [HMS] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [HMS] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [HMS] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [HMS] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [HMS] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [HMS] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [HMS] SET RECOVERY FULL 
GO
ALTER DATABASE [HMS] SET  MULTI_USER 
GO
ALTER DATABASE [HMS] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [HMS] SET DB_CHAINING OFF 
GO
ALTER DATABASE [HMS] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [HMS] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [HMS] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [HMS] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
EXEC sys.sp_db_vardecimal_storage_format N'HMS', N'ON'
GO
ALTER DATABASE [HMS] SET QUERY_STORE = ON
GO
ALTER DATABASE [HMS] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [HMS]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 6/28/2025 6:54:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Appointments]    Script Date: 6/28/2025 6:54:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Appointments](
	[AppointmentID] [int] IDENTITY(1,1) NOT NULL,
	[PatientID] [int] NOT NULL,
	[DoctorID] [int] NOT NULL,
	[AppointmentDateTime] [datetime2](7) NULL,
	[Status] [nvarchar](20) NOT NULL,
	[CreatedAt] [datetime] NULL,
	[Reason] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[AppointmentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Doctors]    Script Date: 6/28/2025 6:54:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Doctors](
	[DoctorID] [int] IDENTITY(1,1) NOT NULL,
	[FirstName] [nvarchar](100) NOT NULL,
	[LastName] [nvarchar](100) NOT NULL,
	[Specialization] [nvarchar](100) NOT NULL,
	[Phone] [nvarchar](15) NULL,
	[Email] [nvarchar](100) NULL,
	[Address] [nvarchar](200) NULL,
	[CreatedAt] [datetime] NULL,
	[UserID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[DoctorID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MedicalRecords]    Script Date: 6/28/2025 6:54:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MedicalRecords](
	[RecordID] [int] IDENTITY(1,1) NOT NULL,
	[PatientID] [int] NOT NULL,
	[DoctorID] [int] NOT NULL,
	[AppointmentID] [int] NOT NULL,
	[Diagnosis] [nvarchar](255) NOT NULL,
	[Prescription] [nvarchar](max) NULL,
	[Notes] [nvarchar](max) NULL,
	[RecordDate] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[RecordID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Notifications]    Script Date: 6/28/2025 6:54:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Notifications](
	[Id] [uniqueidentifier] NOT NULL,
	[UserID] [int] NOT NULL,
	[Message] [nvarchar](500) NOT NULL,
	[IsRead] [bit] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Patients]    Script Date: 6/28/2025 6:54:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Patients](
	[PatientID] [int] IDENTITY(1,1) NOT NULL,
	[First Name] [nvarchar](50) NOT NULL,
	[Last Name] [nvarchar](50) NOT NULL,
	[Date Of Birth] [date] NOT NULL,
	[Gender] [nvarchar](10) NOT NULL,
	[Phone] [nvarchar](15) NULL,
	[Email] [nvarchar](100) NULL,
	[Address] [nvarchar](225) NULL,
	[CreatedAt] [datetime] NULL,
	[UserID] [int] NULL,
 CONSTRAINT [PK_Patients] PRIMARY KEY CLUSTERED 
(
	[PatientID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 6/28/2025 6:54:04 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[UserID] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](100) NOT NULL,
	[PasswordHash] [nvarchar](max) NOT NULL,
	[Email] [nvarchar](255) NOT NULL,
	[Role] [nvarchar](50) NOT NULL,
	[CreatedAt] [datetime] NULL,
PRIMARY KEY CLUSTERED 
(
	[UserID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Appointments] ON 

INSERT [dbo].[Appointments] ([AppointmentID], [PatientID], [DoctorID], [AppointmentDateTime], [Status], [CreatedAt], [Reason]) VALUES (47, 13, 9, CAST(N'2025-06-26T18:00:00.0000000' AS DateTime2), N'Completed', CAST(N'2025-06-26T21:29:11.340' AS DateTime), N'dummy')
INSERT [dbo].[Appointments] ([AppointmentID], [PatientID], [DoctorID], [AppointmentDateTime], [Status], [CreatedAt], [Reason]) VALUES (48, 13, 9, CAST(N'2025-06-26T18:00:00.0000000' AS DateTime2), N'Accepted', CAST(N'2025-06-26T21:29:31.500' AS DateTime), N'tyutyu')
INSERT [dbo].[Appointments] ([AppointmentID], [PatientID], [DoctorID], [AppointmentDateTime], [Status], [CreatedAt], [Reason]) VALUES (49, 13, 9, CAST(N'2025-06-27T18:00:00.0000000' AS DateTime2), N'Accepted', CAST(N'2025-06-26T22:35:08.653' AS DateTime), N'tyuty')
INSERT [dbo].[Appointments] ([AppointmentID], [PatientID], [DoctorID], [AppointmentDateTime], [Status], [CreatedAt], [Reason]) VALUES (50, 13, 9, CAST(N'2025-06-27T15:30:00.0000000' AS DateTime2), N'Pending', CAST(N'2025-06-26T23:26:57.183' AS DateTime), N'tytryt')
SET IDENTITY_INSERT [dbo].[Appointments] OFF
GO
SET IDENTITY_INSERT [dbo].[Doctors] ON 

INSERT [dbo].[Doctors] ([DoctorID], [FirstName], [LastName], [Specialization], [Phone], [Email], [Address], [CreatedAt], [UserID]) VALUES (9, N'James', N'Anderson', N'Cardiologist', N'9876543210', N'james.anderson@example.com', N'123 Heartbeat Street, NY', CAST(N'2025-03-05T20:39:24.423' AS DateTime), 31)
INSERT [dbo].[Doctors] ([DoctorID], [FirstName], [LastName], [Specialization], [Phone], [Email], [Address], [CreatedAt], [UserID]) VALUES (10, N'Sarah', N'Williams', N'Dermatologist', N'9234567890', N'sarah.williams@example.com', N'789 SkinCare Ave, SF', CAST(N'2025-03-05T20:44:52.980' AS DateTime), 34)
SET IDENTITY_INSERT [dbo].[Doctors] OFF
GO
SET IDENTITY_INSERT [dbo].[MedicalRecords] ON 

INSERT [dbo].[MedicalRecords] ([RecordID], [PatientID], [DoctorID], [AppointmentID], [Diagnosis], [Prescription], [Notes], [RecordDate]) VALUES (10, 13, 9, 47, N'dummy', N'dummy', N'dummy', CAST(N'2025-06-26T16:05:37.883' AS DateTime))
SET IDENTITY_INSERT [dbo].[MedicalRecords] OFF
GO
INSERT [dbo].[Notifications] ([Id], [UserID], [Message], [IsRead], [CreatedAt]) VALUES (N'1fe61afb-4638-44b1-bea5-3e62670fb650', 31, N'{"Type":"test","Title":"Test Notification \uD83E\uDDEA","Message":"\uD83E\uDDEA \uD83E\uDDEA Test notification from Doctor Dashboard!","Icon":"\uD83E\uDDEA","Priority":"normal","AppointmentToken":null,"AdditionalData":{}}', 0, CAST(N'2025-06-24T09:53:48.4933129' AS DateTime2))
INSERT [dbo].[Notifications] ([Id], [UserID], [Message], [IsRead], [CreatedAt]) VALUES (N'88b6cde8-5753-4033-8cb2-766c0e0842ff', 31, N'{"Type":"test","Title":"Test Notification \uD83E\uDDEA","Message":"\uD83E\uDDEA \uD83E\uDDEA Test notification from Doctor Dashboard!","Icon":"\uD83E\uDDEA","Priority":"normal","AppointmentToken":null,"AdditionalData":{}}', 0, CAST(N'2025-06-24T09:55:09.3356257' AS DateTime2))
GO
SET IDENTITY_INSERT [dbo].[Patients] ON 

INSERT [dbo].[Patients] ([PatientID], [First Name], [Last Name], [Date Of Birth], [Gender], [Phone], [Email], [Address], [CreatedAt], [UserID]) VALUES (10, N'Michael', N'Brown', CAST(N'1988-11-20' AS Date), N'Male', N'9345678901', N'michael.brown@example.com', N'321 Healthy Way, TX', CAST(N'2025-03-05T20:44:17.010' AS DateTime), 33)
INSERT [dbo].[Patients] ([PatientID], [First Name], [Last Name], [Date Of Birth], [Gender], [Phone], [Email], [Address], [CreatedAt], [UserID]) VALUES (12, N'Piyush', N'Gohil', CAST(N'2002-06-06' AS Date), N'Male', N'9723666302', N'gohilpiyush@gmail.com', N'bhavnagar, gujarat, india', CAST(N'2025-06-22T10:44:01.293' AS DateTime), 36)
INSERT [dbo].[Patients] ([PatientID], [First Name], [Last Name], [Date Of Birth], [Gender], [Phone], [Email], [Address], [CreatedAt], [UserID]) VALUES (13, N'Emily', N'Clark', CAST(N'1995-06-15' AS Date), N'Female', N'9123456789', N'emily.clark@example.com', N'456 Wellness Road, LA', CAST(N'2025-06-26T21:28:00.423' AS DateTime), 38)
SET IDENTITY_INSERT [dbo].[Patients] OFF
GO
SET IDENTITY_INSERT [dbo].[Users] ON 

INSERT [dbo].[Users] ([UserID], [Username], [PasswordHash], [Email], [Role], [CreatedAt]) VALUES (29, N'Darshit', N'$2a$11$oxUOGt0hQxe2szIFUjNkw.J7IySzXN/GAO4hzFdhEOxwnLWEmiC/2', N'Darshitgohil123@gmail.com', N'Admin', CAST(N'2025-03-05T19:22:14.280' AS DateTime))
INSERT [dbo].[Users] ([UserID], [Username], [PasswordHash], [Email], [Role], [CreatedAt]) VALUES (31, N'james_anderson', N'$2a$11$8v54L/7CeI9YwMNNvST5O.z7oYAEtlc4f5HfKmFRILEym6N0Xu4da', N'james.anderson@example.com', N'Doctor', CAST(N'2025-03-05T20:39:24.350' AS DateTime))
INSERT [dbo].[Users] ([UserID], [Username], [PasswordHash], [Email], [Role], [CreatedAt]) VALUES (33, N'michael_brown', N'$2a$11$aD9wNgCbECdpSjk1QD8neO5SnWbJWTuFKnmJa77w6tAYzsKs2eJ1.', N'michael.brown@example.com', N'Patient', CAST(N'2025-03-05T20:44:17.000' AS DateTime))
INSERT [dbo].[Users] ([UserID], [Username], [PasswordHash], [Email], [Role], [CreatedAt]) VALUES (34, N'sarah_williams', N'$2a$11$QLm4fY5daguMkHYGXZucpupBH8fOEQ0SZpFtzzK9Cw5bkTUjcJ6fW', N'sarah.williams@example.com', N'Doctor', CAST(N'2025-03-05T20:44:52.970' AS DateTime))
INSERT [dbo].[Users] ([UserID], [Username], [PasswordHash], [Email], [Role], [CreatedAt]) VALUES (36, N'piyush_gohil', N'$2a$11$IIN8eNgI5JawXgotcbX7u.I4hhBU9zMkWlUPmnkMMB678qcZ2ePcm', N'gohilpiyush@gmail.com', N'Patient', CAST(N'2025-06-22T10:44:01.247' AS DateTime))
INSERT [dbo].[Users] ([UserID], [Username], [PasswordHash], [Email], [Role], [CreatedAt]) VALUES (37, N'Piyush', N'$2a$11$dYo9kdjde6Cg4HymgwZ8heSLT0mkzjrc5r0mNuZr/Z5p0MyOVMPVG', N'Piyushgohil123@gmail.com', N'Admin', CAST(N'2025-06-26T21:12:13.180' AS DateTime))
INSERT [dbo].[Users] ([UserID], [Username], [PasswordHash], [Email], [Role], [CreatedAt]) VALUES (38, N'emily_emily', N'$2a$11$M48n5VXiC.Xi/AhGlPj1de/fDaMly/LvLKm7nleOJ1SqJZ5CVe2a6', N'emily.clark@example.com', N'Patient', CAST(N'2025-06-26T21:28:00.400' AS DateTime))
SET IDENTITY_INSERT [dbo].[Users] OFF
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Doctors__A9D10534A93AE71A]    Script Date: 6/28/2025 6:54:04 PM ******/
ALTER TABLE [dbo].[Doctors] ADD UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Users__536C85E4A707DAE7]    Script Date: 6/28/2025 6:54:04 PM ******/
ALTER TABLE [dbo].[Users] ADD UNIQUE NONCLUSTERED 
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UQ__Users__A9D10534C6E462CA]    Script Date: 6/28/2025 6:54:04 PM ******/
ALTER TABLE [dbo].[Users] ADD UNIQUE NONCLUSTERED 
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Appointments] ADD  DEFAULT ('Scheduled') FOR [Status]
GO
ALTER TABLE [dbo].[Appointments] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Doctors] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[MedicalRecords] ADD  DEFAULT (getdate()) FOR [RecordDate]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT (newid()) FOR [Id]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT ((0)) FOR [IsRead]
GO
ALTER TABLE [dbo].[Notifications] ADD  DEFAULT (getutcdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Patients] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Users] ADD  DEFAULT (getdate()) FOR [CreatedAt]
GO
ALTER TABLE [dbo].[Appointments]  WITH CHECK ADD  CONSTRAINT [FK_Appointments_Patients] FOREIGN KEY([PatientID])
REFERENCES [dbo].[Patients] ([PatientID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Appointments] CHECK CONSTRAINT [FK_Appointments_Patients]
GO
ALTER TABLE [dbo].[MedicalRecords]  WITH CHECK ADD  CONSTRAINT [FK_MedicalRecords_Doctors] FOREIGN KEY([DoctorID])
REFERENCES [dbo].[Doctors] ([DoctorID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MedicalRecords] CHECK CONSTRAINT [FK_MedicalRecords_Doctors]
GO
ALTER TABLE [dbo].[MedicalRecords]  WITH CHECK ADD  CONSTRAINT [FK_MedicalRecords_Patients] FOREIGN KEY([PatientID])
REFERENCES [dbo].[Patients] ([PatientID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[MedicalRecords] CHECK CONSTRAINT [FK_MedicalRecords_Patients]
GO
ALTER TABLE [dbo].[Notifications]  WITH CHECK ADD FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Patients]  WITH CHECK ADD  CONSTRAINT [FK_Patients_Users] FOREIGN KEY([UserID])
REFERENCES [dbo].[Users] ([UserID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Patients] CHECK CONSTRAINT [FK_Patients_Users]
GO
ALTER TABLE [dbo].[Appointments]  WITH CHECK ADD  CONSTRAINT [CK_Appointment_Status] CHECK  (([Status]='Completed' OR [Status]='Rejected' OR [Status]='Accepted' OR [Status]='Pending'))
GO
ALTER TABLE [dbo].[Appointments] CHECK CONSTRAINT [CK_Appointment_Status]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'primary key with auto increament' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Patients', @level2type=N'COLUMN',@level2name=N'PatientID'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Patient''s First Name' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Patients', @level2type=N'COLUMN',@level2name=N'First Name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Patient''s Last Name' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Patients', @level2type=N'COLUMN',@level2name=N'Last Name'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Date of Birth' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Patients', @level2type=N'COLUMN',@level2name=N'Date Of Birth'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Male/Female/Other' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Patients', @level2type=N'COLUMN',@level2name=N'Gender'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Contact Number' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Patients', @level2type=N'COLUMN',@level2name=N'Phone'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Email Address' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Patients', @level2type=N'COLUMN',@level2name=N'Email'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Home Address' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Patients', @level2type=N'COLUMN',@level2name=N'Address'
GO
USE [master]
GO
ALTER DATABASE [HMS] SET  READ_WRITE 
GO
