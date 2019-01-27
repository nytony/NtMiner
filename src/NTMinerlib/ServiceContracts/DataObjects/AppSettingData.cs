﻿using System.Runtime.Serialization;
using System.Text;

namespace NTMiner.ServiceContracts.DataObjects {
    [DataContract]
    public class AppSettingData : IAppSetting {
        [DataMember]
        public string Key { get; set; }

        [DataMember]
        public object Value { get; set; }

        public string GetSignData() {
            StringBuilder sb = new StringBuilder();
            sb.Append(nameof(Key)).Append(Key)
                .Append(nameof(Value)).Append(Value);
            return sb.ToString();
        }
    }
}
