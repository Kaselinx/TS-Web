-- Create the table
CREATE TABLE [dbo].[SecretData] (
    [SecretId] INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [EmployeeId] NVARCHAR(255) NOT NULL,
    [Label] NVARCHAR(255) NOT NULL,
    [Secret] NVARCHAR(255) NOT NULL,
    [IsActive] BIT NOT NULL,
    [Create_time] DATETIME2 NOT NULL DEFAULT GETDATE(),
    [Updated_Time] DATETIME2 NOT NULL DEFAULT GETDATE()
);

-- Add extended properties for each column
EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Secret Id as primary key', 
    @level0type = N'SCHEMA', @level0name = dbo, 
    @level1type = N'TABLE',  @level1name = SecretData, 
    @level2type = N'COLUMN', @level2name = SecretId;

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'The issuer of the TOTP (Time-based One-Time Password) 2FA (Two-Factor Authentication) token. This is actually Employee Number from active directory', 
    @level0type = N'SCHEMA', @level0name = dbo, 
    @level1type = N'TABLE',  @level1name = SecretData, 
    @level2type = N'COLUMN', @level2name = EmployeeId;

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'The label associated with the TOTP 2FA token, typically the user''s account name.', 
    @level0type = N'SCHEMA', @level0name = dbo, 
    @level1type = N'TABLE',  @level1name = SecretData, 
    @level2type = N'COLUMN', @level2name = Label;

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'The secret key used to generate the TOTP 2FA token.', 
    @level0type = N'SCHEMA', @level0name = dbo, 
    @level1type = N'TABLE',  @level1name = SecretData, 
    @level2type = N'COLUMN', @level2name = Secret;

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Indicates if the TOTP token is currently active or not.', 
    @level0type = N'SCHEMA', @level0name = dbo, 
    @level1type = N'TABLE',  @level1name = SecretData, 
    @level2type = N'COLUMN', @level2name = isActive;

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Timestamp when the record was created.', 
    @level0type = N'SCHEMA', @level0name = dbo, 
    @level1type = N'TABLE',  @level1name = SecretData, 
    @level2type = N'COLUMN', @level2name = create_time;

EXEC sp_addextendedproperty 
    @name = N'MS_Description', 
    @value = N'Timestamp when the record was last updated.', 
    @level0type = N'SCHEMA', @level0name = dbo, 
    @level1type = N'TABLE',  @level1name = SecretData, 
    @level2type = N'COLUMN', @level2name = updated_Time;

GO

-- Create the trigger to update the updated_Time column
CREATE TRIGGER trg_UpdateTimestamp
ON [dbo].[SecretData]
AFTER UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE [dbo].[SecretData]
    SET [updated_Time] = GETDATE()
    FROM [dbo].[SecretData] AS sd
    INNER JOIN inserted AS i ON sd.[SecretId] = i.[SecretId];
END;