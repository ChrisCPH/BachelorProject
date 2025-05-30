USE [master]
GO

/****** Object:  Database [RunningDB]    Script Date: 26-05-2025 21:49:25 ******/
DROP DATABASE [RunningDB]
GO

/****** Object:  Database [RunningDB]    Script Date: 26-05-2025 21:49:25 ******/
CREATE DATABASE [RunningDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'RunningDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\RunningDB.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'RunningDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.MSSQLSERVER\MSSQL\DATA\RunningDB_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [RunningDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [RunningDB] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [RunningDB] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [RunningDB] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [RunningDB] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [RunningDB] SET ARITHABORT OFF 
GO

ALTER DATABASE [RunningDB] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [RunningDB] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [RunningDB] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [RunningDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [RunningDB] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [RunningDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [RunningDB] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [RunningDB] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [RunningDB] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [RunningDB] SET  DISABLE_BROKER 
GO

ALTER DATABASE [RunningDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [RunningDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [RunningDB] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [RunningDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [RunningDB] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [RunningDB] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [RunningDB] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [RunningDB] SET RECOVERY FULL 
GO

ALTER DATABASE [RunningDB] SET  MULTI_USER 
GO

ALTER DATABASE [RunningDB] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [RunningDB] SET DB_CHAINING OFF 
GO

ALTER DATABASE [RunningDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [RunningDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO

ALTER DATABASE [RunningDB] SET DELAYED_DURABILITY = DISABLED 
GO

ALTER DATABASE [RunningDB] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO

ALTER DATABASE [RunningDB] SET QUERY_STORE = ON
GO

ALTER DATABASE [RunningDB] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO

ALTER DATABASE [RunningDB] SET  READ_WRITE 
GO

