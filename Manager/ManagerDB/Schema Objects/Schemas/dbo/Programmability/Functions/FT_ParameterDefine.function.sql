CREATE FUNCTION [dbo].[FT_ParameterDefine]()
RETURNS TABLE
AS
RETURN
(
	SELECT 
	[ParameterKey]
	,[SettingType]
	,[SqlDbType]
    FROM [dbo].[ParameterDefine]
)
