using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy.Infrastructure.Extension
{

    /// <summary>
    /// 返回的是个json
    /// </summary>
    public class APIAttribute : Attribute
    {

    }

    /// <summary>
    /// 返回的是个页面
    /// </summary>
    public class FormAPIAttribute : Attribute
    {

    }

    /// <summary>
    /// 返回的是个文件
    /// </summary>
    public class FileAPIAttribute : Attribute
    {

    }

    /// <summary>
    /// 代表上传文件接口，第一个函数必须是File
    /// </summary>
    public class FileUploadAttribute : Attribute
    {

    }


    /// <summary>
    /// 代表接口需要校验权限
    /// </summary>
    public class SecureAttribute : Attribute
    {
    }

    /// <summary>
    /// 表示一个远程校验类
    /// </summary>
    public class ValidateAPIAttribute : Attribute
    {
    }
}
