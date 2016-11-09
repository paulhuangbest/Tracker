<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="WMS.aspx.cs" Inherits="WebApp.WMS" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        
        <asp:Button ID="btnOperate" runat="server" OnClick="btnOperate_Click" Text="OperateLog" />

        

        <hr />
        <asp:Button ID="btnException" runat="server" Text="ExceptionLog" OnClick="btnException_Click" />
            
        <asp:FileUpload ID="FileUpload1" runat="server" />
            
    </div>
    </form>
</body>
</html>
