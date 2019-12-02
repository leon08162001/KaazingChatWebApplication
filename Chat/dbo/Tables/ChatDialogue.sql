CREATE TABLE [dbo].[ChatDialogue] (
    [id]           NVARCHAR (30)  NOT NULL,
    [name]         NVARCHAR (30)  NOT NULL,
    [receiver]     NVARCHAR(400)    NOT NULL,
    [htmlMessage]  NVARCHAR (MAX) NOT NULL,
    [date]         DATETIME       NOT NULL,
    [oprTime]      DATETIME2 (7)  NOT NULL,
    [oprIpAddress] NVARCHAR (15)  NULL,
    CONSTRAINT [PK_ChatDialogue] PRIMARY KEY CLUSTERED ([id] ASC, [receiver] ASC, [date] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'用戶ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChatDialogue', @level2type = N'COLUMN', @level2name = N'id';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'用戶名稱', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChatDialogue', @level2type = N'COLUMN', @level2name = N'name';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'訊息接收者', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChatDialogue', @level2type = N'COLUMN', @level2name = N'receiver';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'聊天訊息', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChatDialogue', @level2type = N'COLUMN', @level2name = N'htmlMessage';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'聊天訊息日期', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChatDialogue', @level2type = N'COLUMN', @level2name = N'date';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'異動時間', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChatDialogue', @level2type = N'COLUMN', @level2name = N'oprTime';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'操作用戶端IP', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'ChatDialogue', @level2type = N'COLUMN', @level2name = N'oprIpAddress';

