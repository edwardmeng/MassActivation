<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="MassActivation.Web.Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
    <table>
        <tr>
            <td><strong>Application Name</strong></td>
            <td>
                <asp:Label runat="server" ID="labelName"></asp:Label>
            </td>
        </tr>
        <tr>
            <td><strong>Application Version</strong></td>
            <td>
                <asp:Label runat="server" ID="labelVersion"></asp:Label>
            </td>
        </tr>
    </table>
    </div>
    </form>
</body>
</html>
