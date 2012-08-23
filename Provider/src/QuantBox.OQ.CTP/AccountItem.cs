using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace QuantBox.OQ.CTP
{
    [DefaultPropertyAttribute("Label")]
    public class AccountItem
    {
        [DescriptionAttribute("账号")]
        public string InvestorId
        {
            get;
            set;
        }
        [PasswordPropertyText(true)]
        [DescriptionAttribute("密码")]
        public string Password
        {
            get;
            set;
        }

        [CategoryAttribute("标签"),
        DescriptionAttribute("标签不能重复")]
        public string Label
        {
            get;
            set;
        }

        public override string ToString()
        {
            return "标签不能重复";
        }

        [BrowsableAttribute(false)]
        public string Name
        {
            get { return Label; }
        }
    }
}
