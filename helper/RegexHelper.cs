using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CommonHelper
{
    public class RegexHelper
    {
        //身份证号码，包括15位和18位
        public const string REG_IDCARD = @"^(^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$)|(^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$)$";

        public const string REG_MOBILEPHONE = @"^1[3|4|5|7|8]\d{9}$";

        public const string REG_SIMPLE_TELEPHONE = @"^0[1-9]\d{9}$";

        public static bool ValidateRegex(string totest, string pattern)
        {
            Regex rx = new Regex(pattern);
            return rx.IsMatch(totest);
        }

        /// <summary>
        /// 身份证号码验证
        /// -------------------------------------------------
        /// 身份证15位编码规则：dddddd yymmdd xx p
        /// dddddd：6位地区编码
        /// yymmdd: 出生年(两位年)月日，如：910215
        /// xx: 顺序编码，系统产生，无法确定
        /// p: 性别，奇数为男，偶数为女
        /// 
        /// 身份证18位编码规则：dddddd yyyymmdd xxx y
        /// dddddd：6位地区编码
        /// yyyymmdd: 出生年(四位年)月日，如：19910215
        /// xxx：顺序编码，系统产生，无法确定，奇数为男，偶数为女
        /// y: 校验码，该位数值可通过前17位计算获得
        /// 
        /// 前17位号码加权因子为 Wi = [ 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 ]
        /// 验证位 Y = [ 1, 0, 10, 9, 8, 7, 6, 5, 4, 3, 2 ]
        /// 如果验证码恰好是10，为了保证身份证是十八位，那么第十八位将用X来代替
        /// 校验位计算公式：Y_P = mod( ∑(Ai×Wi),11 )
        /// i为身份证号码1...17 位; Y_P为校验码Y所在校验码数组位置
        /// </summary>
        /// <param name="IDCard"></param>
        /// <returns></returns>
        public static bool ValidateIDCard(string IDCard)
        {
            if (!ValidateRegex(IDCard, REG_IDCARD))
            {
                return false;
            }
            if (IDCard.Length == 15) //15位身份证无需验证校验位
            {
                return true;
            }

            return ValidateIDCardCheckBit(IDCard);
        }

        public static bool ValidateIDCardCheckBit(string IDCard)
        {
            var idCardWi = new int[] { 7, 9, 10, 5, 8, 4, 2, 1, 6, 3, 7, 9, 10, 5, 8, 4, 2 }; //将前17位加权因子保存在数组里
            var idCardY = new int[] { 1, 0, 10, 9, 8, 7, 6, 5, 4, 3, 2 }; //这是除以11后，可能产生的11位余数、验证码，也保存成数组
            var idCardWiSum = 0;
            for (var i = 0; i < 17; i++)
            {
                idCardWiSum += Convert.ToInt32(IDCard.Substring(i, 1)) * idCardWi[i];
            }
            var idCardMod = idCardWiSum % 11;//计算出校验码所在数组的位置
            var idCardLast = IDCard.Substring(17);//得到最后一位身份证号码

            //如果等于2，则说明校验码是10，身份证号码最后一位应该是X
            if (idCardMod == 2)
            {
                if (idCardLast == "X" || idCardLast == "x")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                //用计算出的验证码与最后一位身份证号码匹配，如果一致，说明通过，否则是无效的身份证号码
                if (idCardLast == idCardY[idCardMod].ToString())
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool IsEncryptPhoneNumber(string phoneNumber)
        {
            var tripleDESCryptoHelper = new TripleDESCryptoHelper(MobileEncrypKey.KEY, MobileEncrypKey.IV);
            try
            {
                var decrypt = tripleDESCryptoHelper.Decrypt(phoneNumber);
                if (ValidateRegex(decrypt, REG_MOBILEPHONE) || ValidateRegex(decrypt, REG_SIMPLE_TELEPHONE))
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteError(ex.ToString());
                return false;
            }

            return false;
        }

        public static bool IsPhoneNumber(string phoneNumber)
        {
            try
            {
                return ValidateRegex(phoneNumber, REG_MOBILEPHONE) || ValidateRegex(phoneNumber, REG_SIMPLE_TELEPHONE);
            }
            catch
            {
                return false;
            }
        }

        public static int TryGetNumber(string str)
        {
            try
            {
                if (ValidateRegex(str, @"^\d+$"))
                {
                    return Convert.ToInt32(str);
                }
                else
                {
                    return 0;
                }
            }
            catch
            {
                return 0;
            }
        }
    }
}
