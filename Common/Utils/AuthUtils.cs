using JWT;
using JWT.Exceptions;
using JWT.Serializers;
using System;

namespace TickQuant.Common
{
    //这儿写入一些静态方法 供全局调用
    public static class AuthUtils
    {
        public static AuthUserInfo GetJWTToken(string jwtToken)
        {
            try
            {
                const string secret = "mmserver2019";                              //PHP加密JWT的密钥
                IJsonSerializer serializer = new JsonNetSerializer();              //初始化一个序列类
                IDateTimeProvider provider = new UtcDateTimeProvider();            //初始化UTC时间类
                IJwtValidator validator = new JwtValidator(serializer, provider);  //初始化一个验证类
                IBase64UrlEncoder urlEncoder = new JwtBase64UrlEncoder();
                IJwtDecoder decoder = new JwtDecoder(serializer, urlEncoder);
                //IJwtDecoder decoder = new JwtDecoder(serializer, validator, urlEncoder);//对传过来的JWT 进行验证 
                var jwt = decoder.DecodeToObject(jwtToken, secret, true);
                return serializer.Deserialize<AuthUserInfo>(jwt["data"].ToString());//反序列化还原成AuthUserInfo
            }
            catch (TokenExpiredException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
            catch (SignatureVerificationException ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }
    }
}
