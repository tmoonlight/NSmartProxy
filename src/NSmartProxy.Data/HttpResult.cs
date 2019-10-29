using System;
using System.Collections.Generic;
using System.Text;

namespace NSmartProxy.Data
{
    public static class HttpResultExtension
    {
        /// <summary>
        /// 包装对象为返回结果，即使是null也能包装
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static HttpResult<T> Wrap<T>(this T obj)
        {
            if (obj is Exception)
            {
                Exception ex = obj as Exception;
                return new HttpResult<T>()
                {
                    State = 0,
                    Msg = ex.ToString()
                };
            }

            return new HttpResult<T>()
            {
                State = 1,
                Msg = "",
                Data = obj
            };
        }
    }

    public class HttpResult<T>
    {
        public HttpResult()
        {

        }

        //1代表成功 0代表失败
        private int state;
        private string msg;
        private T data;

        public int State { get => state; set => state = value; }

        /// <summary>
        /// 附加信息，一般是错误信息，或者是提示信息
        /// </summary>
        public string Msg { get => msg; set => msg = value; }
        public T Data { get => data; set => data = value; }
        public static HttpResult<T> NullSuccessResult { get; } = new HttpResult<T>() { state = 1 };
    }
}
